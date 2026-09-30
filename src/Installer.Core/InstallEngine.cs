using System.Security.Cryptography;
using System.Text.Json;

namespace RO3.Installer;

public sealed record PayloadFile(string Path, string Sha256);
public sealed record PayloadManifest(string Version, bool ReadyForInstallation, PayloadFile[] Files, string? TargetProfile = null);
public sealed record InstallProgress(int Completed, int Total, string Path);

public static class GameSelection
{
    // This method checks ONE explicit directory. No scan, registry or launcher lookup.
    public static string ValidateDirectory(string selected)
    {
        if (string.IsNullOrWhiteSpace(selected)) throw new InvalidDataException("กรุณาเลือกโฟลเดอร์เกม");
        string root = System.IO.Path.GetFullPath(selected);
        if (!File.Exists(System.IO.Path.Combine(root, "ro3.exe")))
            throw new InvalidDataException("เลือกโฟลเดอร์ไม่ถูกต้อง: ไม่พบ ro3.exe");
        return root;
    }
    public static string FromExe(string selected)
    {
        if (!File.Exists(selected) || !System.IO.Path.GetFileName(selected).Equals("ro3.exe", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("กรุณาเลือกไฟล์ ro3.exe");
        return ValidateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(selected))!);
    }
}

public static partial class InstallEngine
{
    public static string ResolveSafe(string root, string relative)
    {
        if (string.IsNullOrWhiteSpace(relative) || relative.Contains('\\') || relative.Contains(':') ||
            System.IO.Path.IsPathRooted(relative) || relative.Split('/').Any(p => p is "" or "." or ".."))
            throw new InvalidDataException("Invalid payload path: " + relative);
        // Do not let a patch replace the game EXE or arbitrary user files.
        bool allowed = relative.StartsWith("BepInEx/", StringComparison.Ordinal) ||
            relative is "winhttp.dll" or "doorstop_config.ini" or ".doorstop_version" or "arialuni_sdf_u2022";
        if (!allowed) throw new InvalidDataException("Payload path is not permitted: " + relative);
        string fullRoot = System.IO.Path.GetFullPath(root);
        string full = System.IO.Path.GetFullPath(System.IO.Path.Combine(fullRoot, relative.Replace('/', System.IO.Path.DirectorySeparatorChar)));
        if (!full.StartsWith(fullRoot + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Path escapes selected folder");
        // Inspect only ancestors of this explicit manifest path, never enumerate drives/folders.
        for (string? current = full; current != null && current != System.IO.Path.GetDirectoryName(fullRoot); current = System.IO.Path.GetDirectoryName(current))
        {
            if ((File.Exists(current) || Directory.Exists(current)) &&
                (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("Symbolic/reparse path not supported: " + current);
            if (current.Equals(fullRoot, StringComparison.OrdinalIgnoreCase)) break;
        }
        return full;
    }

    public static string Sha(string file) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))).ToLowerInvariant();

    public static void ValidatePayload(string payloadRoot, PayloadManifest manifest)
    {
        if (!manifest.ReadyForInstallation || manifest.Files.Length == 0)
            throw new InvalidDataException("แพตช์รุ่นนี้ยังไม่พร้อมติดตั้ง: ยังไม่มี runtime และ payload ที่ผ่านการตรวจครบ");
        if (manifest.Files.Length > 5000) throw new InvalidDataException("Payload too large");
        if (manifest.TargetProfile != null && manifest.TargetProfile != "ro3-mono-x64") throw new InvalidDataException("Unsupported target profile");
        if (manifest.TargetProfile == "ro3-mono-x64")
        {
            string[] required = { "BepInEx/config/RO3.LanguageOrigins.tsv", "winhttp.dll", "doorstop_config.ini", "arialuni_sdf_u2022", "BepInEx/core/BepInEx.dll", "BepInEx/core/BepInEx.Preloader.dll", "BepInEx/core/0Harmony.dll", "BepInEx/plugins/RO3.ThaiLocalization.Skills.dll", "BepInEx/plugins/SkillRuntime.Engine.dll", "BepInEx/config/RO3.SkillTranslations.tsv", "BepInEx/config/RO3.SkillRules.tsv", "BepInEx/config/AutoTranslatorConfig.ini" };
            foreach (string path in required) if (!manifest.Files.Any(f => f.Path == path)) throw new InvalidDataException("Incomplete skills payload: " + path);
        }
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in manifest.Files)
        {
            if (!names.Add(entry.Path)) throw new InvalidDataException("Duplicate payload path");
            string file = ResolveSafe(payloadRoot, entry.Path);
            if (!File.Exists(file) || new FileInfo(file).Length > 128 * 1024 * 1024 ||
                !Sha(file).Equals(entry.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Payload integrity check failed: " + entry.Path);
        }
    }

    public static void Install(string selected, string payloadRoot, PayloadManifest manifest, IProgress<InstallProgress>? progress = null)
    {
        string target = GameSelection.ValidateDirectory(selected);
        ValidatePayload(payloadRoot, manifest); // Must pass before ANY game writes.
        if (manifest.TargetProfile == "ro3-mono-x64")
        {
            ValidateMonoX64(target);
            if (File.Exists(System.IO.Path.Combine(target, "winhttp.dll")))
                throw new InvalidOperationException("พบ winhttp.dll เดิม: ไม่ทับ proxy หรือม็อดเดิมในรุ่น Alpha");
        }
        if (Directory.Exists(System.IO.Path.Combine(target, "BepInEx")))
            throw new InvalidOperationException("พบ BepInEx เดิม: รุ่นพัฒนานี้ไม่ทับไฟล์หรือม็อดเดิม กรุณารอระบบอัปเดตที่ผ่านการทดสอบ");
        var paths = manifest.Files.Select(e => (Entry: e, Source: ResolveSafe(payloadRoot, e.Path), Target: ResolveSafe(target, e.Path))).ToArray();
        string marker = System.IO.Path.Combine(target, ".ro3-thai-localization.json");
        if (File.Exists(marker)) throw new InvalidOperationException("พบข้อมูลการติดตั้งเดิม: ไม่ติดตั้งทับในรุ่นพัฒนานี้");
        string backup = System.IO.Path.Combine(target, ".ro3-thai-backup-" + Guid.NewGuid().ToString("N"));
        var written = new List<(string Target, string? Backup)>();
        var madeDirectories = new List<string>();
        try
        {
            Directory.CreateDirectory(backup);
            int completed = 0;
            foreach (var p in paths)
            {
                // Recheck immediately before writing, including reparse-point checks.
                ResolveSafe(target, p.Entry.Path);
                if (Directory.Exists(p.Target)) throw new IOException("Destination is a directory: " + p.Entry.Path);
                string? saved = null;
                if (File.Exists(p.Target))
                {
                    saved = System.IO.Path.Combine(backup, completed.ToString());
                    File.Copy(p.Target, saved, false);
                }
                var absent = new Stack<string>();
                string? parent = System.IO.Path.GetDirectoryName(p.Target);
                while (parent != null && !Directory.Exists(parent)) { absent.Push(parent); parent = System.IO.Path.GetDirectoryName(parent); }
                foreach (string dir in absent) { Directory.CreateDirectory(dir); madeDirectories.Add(dir); }
                written.Add((p.Target, saved));
                using (var source = File.OpenRead(p.Source))
                using (var dest = new FileStream(p.Target, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    source.CopyTo(dest);
                    dest.Flush(true);
                }
                if (!Sha(p.Target).Equals(p.Entry.Sha256, StringComparison.OrdinalIgnoreCase)) throw new IOException("Post-copy hash failed");
                progress?.Report(new InstallProgress(++completed, paths.Length, p.Entry.Path));
            }
            File.WriteAllText(marker, JsonSerializer.Serialize(new { manifest.Version, manifest.Files, BackupDirectory = backup, manifest.TargetProfile }));
        }
        catch (Exception installError)
        {
            var rollbackErrors = new List<Exception>();
            foreach (var p in written.AsEnumerable().Reverse())
            {
                try { if (p.Backup != null) File.Copy(p.Backup, p.Target, true); else File.Delete(p.Target); }
                catch (Exception error) { rollbackErrors.Add(error); }
            }
            foreach (var dir in madeDirectories.AsEnumerable().Reverse())
            {
                try { Directory.Delete(dir, false); } catch (Exception error) { rollbackErrors.Add(error); }
            }
            try { File.Delete(marker); } catch (Exception error) { rollbackErrors.Add(error); }
            // Retain backups and report their path when rollback is incomplete.
            if (rollbackErrors.Count != 0)
                throw new AggregateException("ติดตั้งล้มเหลวและ rollback ไม่ครบ เก็บ backup ไว้ที่ " + backup, new[] { installError }.Concat(rollbackErrors));
            if (Directory.Exists(backup)) Directory.Delete(backup, true);
            throw;
        }
    }
}
