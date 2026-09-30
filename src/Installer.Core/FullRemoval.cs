using System.Text.Json;
using System.Security.Cryptography;

namespace RO3.Installer;

public sealed record FullRemovalResult(string BackupDirectory, int MovedFileCount, string[] PreservedRootPaths);

public static partial class InstallEngine
{
    public static bool CanRemoveBepInExAll(string selected) => Directory.Exists(Path.Combine(selected,"BepInEx")) || File.Exists(Path.Combine(selected,".ro3-thai-localization.json"));

    // Explicit, user-confirmed removal in ONE selected Client. Never scans drives or touches ro3_Data.
    // Rename to a same-volume backup rather than permanently deleting mods/config/logs.
    public static FullRemovalResult RemoveBepInExAll(string selected, IProgress<InstallProgress>? progress = null)
    {
        string root=GameSelection.ValidateDirectory(selected);
        ResolveSafe(root,"BepInEx/.ro3-removal-probe"); // Reject linked Client/ancestors/BepInEx before ANY writes.
        string marker=Path.Combine(root,".ro3-thai-localization.json");
        bool hasFolder=Directory.Exists(Path.Combine(root,"BepInEx"));
        if (!hasFolder && !File.Exists(marker)) throw new InvalidDataException("ไม่พบ BepInEx หรือข้อมูลแพตช์ใน Client ที่เลือก");
        if (File.Exists(marker) && (File.GetAttributes(marker)&FileAttributes.ReparsePoint)!=0) throw new InvalidDataException("Linked ownership marker rejected");
        var owned=new HashSet<string>(StringComparer.Ordinal);
        if (File.Exists(marker) && new FileInfo(marker).Length<=2*1024*1024)
        {
            try { var record=JsonSerializer.Deserialize<InstallationRecord>(File.ReadAllText(marker)); if(record?.Files is { Length: > 0 and <= 5000 }) foreach(var f in record.Files) if(f?.Path != null) owned.Add(f.Path); }
            catch(JsonException) { /* A damaged marker must not prevent an explicitly confirmed folder removal. */ }
        }
        string doorstop=ResolveSafe(root,"doorstop_config.ini");
        bool configured=File.Exists(doorstop) && new FileInfo(doorstop).Length<=256*1024 && File.ReadAllText(doorstop).Contains("BepInEx.Preloader",StringComparison.OrdinalIgnoreCase);
        bool proxyProvenance=configured || owned.Contains("winhttp.dll");
        var relative=new List<string>();var preserved=new List<string>();
        foreach(string name in new[]{"winhttp.dll","doorstop_config.ini",".doorstop_version"})
        {
            string path=ResolveSafe(root,name);
            if(Directory.Exists(path)) throw new IOException("ไฟล์ loader เป็นโฟลเดอร์: "+name);
            if(!File.Exists(path))continue;
            if(proxyProvenance)relative.Add(name);else preserved.Add(name);
        }
        string font=ResolveSafe(root,"arialuni_sdf_u2022");
        if(File.Exists(font))
        {
            bool known=owned.Contains("arialuni_sdf_u2022");
            if(!known){using var s=File.OpenRead(font);known=Convert.ToHexString(SHA256.HashData(s)).Equals("DE18A759D475E01F90CFFEE16BDC861BF3AA09BC501F3C622E5822622B5E1606",StringComparison.Ordinal);}
            if(known)relative.Add("arialuni_sdf_u2022");else preserved.Add("arialuni_sdf_u2022");
        }
        int insideFiles=0;
        if(hasFolder)
        {
            var stack=new Stack<(string Path,int Depth)>();stack.Push((Path.Combine(root,"BepInEx"),0));int entries=0;
            while(stack.Count!=0)
            {
                var dir=stack.Pop();if(dir.Depth>64)throw new InvalidDataException("BepInEx tree depth limit");
                foreach(string item in Directory.EnumerateFileSystemEntries(dir.Path))
                {
                    if(++entries>100000)throw new InvalidDataException("BepInEx tree entry limit");
                    var attrs=File.GetAttributes(item);
                    if((attrs&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException("ไม่ถอนผ่าน symlink/junction: "+item);
                    if((attrs&FileAttributes.Directory)!=0)stack.Push((item,dir.Depth+1));else insideFiles++;
                }
            }
            relative.Add("BepInEx");
        }
        if(File.Exists(marker))relative.Add(".ro3-thai-localization.json");
        string backup=Path.Combine(root,".ro3-bepinex-removed-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(backup);
        File.WriteAllText(Path.Combine(backup,"removal.json"),JsonSerializer.Serialize(new{Mode="all-confirmed",CreatedUtc=DateTime.UtcNow,Paths=relative,PreservedRootPaths=preserved,GameFilesModified=false}));
        var moved=new List<string>();
        try
        {
            foreach(string name in relative)
            {
                // Recheck immediately before each rename. No game EXE or arbitrary root DLL is in this list.
                if(name=="BepInEx")ResolveSafe(root,"BepInEx/.ro3-removal-probe");
                else if(name!=".ro3-thai-localization.json")ResolveSafe(root,name);
                string source=Path.Combine(root,name),target=Path.Combine(backup,name);
                if((File.GetAttributes(source)&FileAttributes.ReparsePoint)!=0)throw new InvalidDataException("Linked removal source rejected");
                if(name=="BepInEx")Directory.Move(source,target);else File.Move(source,target);
                moved.Add(name);progress?.Report(new InstallProgress(moved.Count,relative.Count,name));
            }
        }
        catch(Exception error)
        {
            var failures=new List<Exception>();
            foreach(string name in moved.AsEnumerable().Reverse())
            {
                try{string source=Path.Combine(backup,name),target=Path.Combine(root,name);if(name=="BepInEx")Directory.Move(source,target);else File.Move(source,target);}
                catch(Exception restore){failures.Add(restore);}
            }
            if(failures.Count>0)throw new AggregateException("ถอนทั้งหมดล้มเหลว rollback ไม่ครบ เก็บ backup ไว้ที่ "+backup,new[]{error}.Concat(failures));
            throw;
        }
        return new FullRemovalResult(backup,insideFiles+relative.Count(n=>n!="BepInEx"),preserved.ToArray());
    }
}
