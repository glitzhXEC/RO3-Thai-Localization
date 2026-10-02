using System.Security.Cryptography;
using System.Text.Json;

namespace RO3.Installer;

public sealed record PayloadFile(string Path, string Sha256);
public sealed record PayloadManifest(string Version, bool ReadyForInstallation, PayloadFile[] Files, string? TargetProfile = null);
public sealed record InstallProgress(int Completed, int Total, string Path);
public sealed record InstallationRecord(string Version, PayloadFile[] Files, string? BackupDirectory = null, string? TargetProfile = null, Dictionary<string, string>? OriginalFiles = null);

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
    public static InstallationRecord? ReadInstallation(string selected)
    {
        string root = GameSelection.ValidateDirectory(selected);
        string marker = Path.Combine(root, ".ro3-thai-localization.json");
        if (!File.Exists(marker)) return null;
        if ((File.GetAttributes(marker) & FileAttributes.ReparsePoint) != 0 || new FileInfo(marker).Length > 2 * 1024 * 1024)
            throw new InvalidDataException("ข้อมูล ownership ของแพตช์เสียหายหรือไม่ปลอดภัย");
        var record = JsonSerializer.Deserialize<InstallationRecord>(File.ReadAllText(marker)) ?? throw new InvalidDataException("ข้อมูล ownership ของแพตช์อ่านไม่ได้");
        ValidateRecord(root, record);
        return record;
    }

    public static int CompareVersions(string installed, string available)
    {
        static (Version Core, string Suffix) Parse(string value)
        {
            var match = System.Text.RegularExpressions.Regex.Match(value, @"\Av?(\d+)\.(\d+)\.(\d+)(?:[-+].*)?\z");
            if (!match.Success || !Version.TryParse(match.Groups[1].Value + "." + match.Groups[2].Value + "." + match.Groups[3].Value, out var parsed))
                throw new InvalidDataException("รูปแบบเวอร์ชันแพตช์ไม่ถูกต้อง: " + value);
            int separator = value.IndexOfAny(new[] { '-', '+' });
            string suffix = separator < 0 ? "" : value[(separator + 1)..];
            return (parsed, suffix);
        }
        if (String.Equals(installed, available, StringComparison.OrdinalIgnoreCase)) return 0;
        var a = Parse(installed); var b = Parse(available);
        int core = a.Core.CompareTo(b.Core); if (core != 0) return core;
        if (a.Suffix.Length == 0) return b.Suffix.Length == 0 ? 0 : 1;
        if (b.Suffix.Length == 0) return -1;
        var ai = System.Text.RegularExpressions.Regex.Match(a.Suffix, @"(?:^|\.)(\d+)\z");
        var bi = System.Text.RegularExpressions.Regex.Match(b.Suffix, @"(?:^|\.)(\d+)\z");
        if (ai.Success && bi.Success && a.Suffix[..ai.Index] == b.Suffix[..bi.Index] &&
            int.TryParse(ai.Groups[1].Value, out int an) && int.TryParse(bi.Groups[1].Value, out int bn)) return an.CompareTo(bn);
        return StringComparer.OrdinalIgnoreCase.Compare(a.Suffix, b.Suffix);
    }

    private static void ValidateRecord(string root, InstallationRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.Version) || record.Files == null || record.Files.Length == 0 || record.Files.Length > 5000)
            throw new InvalidDataException("รายการไฟล์ของแพตช์ไม่ถูกต้อง");
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in record.Files)
        {
            if (entry == null || !names.Add(entry.Path)) throw new InvalidDataException("รายการ ownership ซ้ำหรือไม่ถูกต้อง");
            ResolveSafe(root, entry.Path);
        }
        if (record.OriginalFiles != null)
        {
            foreach (var pair in record.OriginalFiles)
            {
                if (!names.Contains(pair.Key) || string.IsNullOrWhiteSpace(pair.Value) || pair.Value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                    pair.Value.Contains('/') || pair.Value.Contains('\\') || pair.Value is "." or "..")
                    throw new InvalidDataException("รายการไฟล์ต้นฉบับใน ownership ไม่ถูกต้อง");
            }
        }
        if (record.BackupDirectory != null)
        {
            string backup = Path.GetFullPath(record.BackupDirectory);
            if (Path.GetDirectoryName(backup) != root || !Path.GetFileName(backup).StartsWith(".ro3-thai-backup-", StringComparison.Ordinal) ||
                !Directory.Exists(backup) || (File.GetAttributes(backup) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("ข้อมูล backup เดิมไม่ถูกต้อง");
        }
    }

    internal static string? OriginalFilePath(InstallationRecord record, string path)
    {
        if (record.BackupDirectory == null) return null;
        string name;
        if (record.OriginalFiles != null)
        {
            if (!record.OriginalFiles.TryGetValue(path, out name!)) return null;
        }
        else
        {
            int index = Array.FindIndex(record.Files, f => f.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
            if (index < 0) return null;
            name = index.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        string full = Path.Combine(record.BackupDirectory, name);
        return File.Exists(full) ? full : null;
    }
    public static string ResolveSafe(string root, string relative)
    {
        if (string.IsNullOrWhiteSpace(relative) || relative.Contains('\\') || relative.Contains(':') ||
            System.IO.Path.IsPathRooted(relative) || relative.Split('/').Any(p => p is "" or "." or ".."))
            throw new InvalidDataException("Invalid payload path: " + relative);
        // Do not let a patch replace the game EXE or arbitrary user files.
        // arialuni_sdf_u2022 remains path-safe only so old ownership markers can
        // be read/uninstalled during migration; new payload validation rejects it.
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
            string[] required = { "BepInEx/config/RO3.LanguageOrigins.tsv", "winhttp.dll", "doorstop_config.ini", "BepInEx/core/BepInEx.dll", "BepInEx/core/BepInEx.Preloader.dll", "BepInEx/core/0Harmony.dll", "BepInEx/plugins/RO3.ThaiLocalization.Skills.dll", "BepInEx/plugins/SkillRuntime.Engine.dll", "BepInEx/config/RO3.SkillTranslations.tsv", "BepInEx/config/RO3.SkillRules.tsv" };
            foreach (string path in required) if (!manifest.Files.Any(f => f.Path == path)) throw new InvalidDataException("Incomplete skills payload: " + path);
        }
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in manifest.Files)
        {
            if (!names.Add(entry.Path)) throw new InvalidDataException("Duplicate payload path");
            if (manifest.TargetProfile == "ro3-mono-x64" &&
                (entry.Path.Contains("XUnity", StringComparison.OrdinalIgnoreCase) ||
                 entry.Path.Contains("AutoTranslator", StringComparison.OrdinalIgnoreCase) ||
                 entry.Path.Equals("arialuni_sdf_u2022", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidDataException("Minimal runtime payload must not include XUnity or its font bundle: " + entry.Path);
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
            if (File.Exists(System.IO.Path.Combine(target, "winhttp.dll")) && ReadInstallation(target)?.Files.All(f => f.Path != "winhttp.dll") != false)
                throw new InvalidOperationException("พบ winhttp.dll เดิม: ไม่ทับ proxy หรือม็อดเดิมในรุ่น Alpha");
        }
        var paths = manifest.Files.Select(e => (Entry: e, Source: ResolveSafe(payloadRoot, e.Path), Target: ResolveSafe(target, e.Path))).ToArray();
        string marker = System.IO.Path.Combine(target, ".ro3-thai-localization.json");
        InstallationRecord? previous = ReadInstallation(target);
        var previousByPath = previous?.Files.ToDictionary(f => f.Path, StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, PayloadFile>(StringComparer.OrdinalIgnoreCase);
        var newPaths = new HashSet<string>(paths.Select(p => p.Entry.Path), StringComparer.OrdinalIgnoreCase);
        if (previous == null && Directory.Exists(System.IO.Path.Combine(target, "BepInEx")))
            throw new InvalidOperationException("พบ BepInEx ที่ไม่ได้ติดตั้งโดยแพตช์นี้ โปรแกรมจะไม่เขียนทับม็อดหรือไฟล์ของโปรแกรมอื่น");
        foreach (var p in paths)
        {
            if (Directory.Exists(p.Target)) throw new IOException("ปลายทางเป็นโฟลเดอร์: " + p.Entry.Path);
            if (File.Exists(p.Target) && !previousByPath.ContainsKey(p.Entry.Path))
                throw new IOException("พบไฟล์ที่แพตช์นี้ไม่ได้เป็นเจ้าของ จึงไม่เขียนทับ: " + p.Entry.Path);
        }
        var obsolete = previousByPath.Values.Where(f => !newPaths.Contains(f.Path)).ToArray();
        foreach (var old in obsolete)
        {
            string full = ResolveSafe(target, old.Path);
            if (File.Exists(full) && !MutableOwnedFile(old.Path) && !Sha(full).Equals(old.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new IOException("ไฟล์แพตช์เก่าถูกแก้ไข จะไม่ลบอัตโนมัติ: " + old.Path);
        }
        // Temporary rollback copies live outside Client and are deleted on success.
        string rollback = System.IO.Path.Combine(Path.GetTempPath(), "ro3-thai-update-" + Guid.NewGuid().ToString("N"));
        var affected = new HashSet<string>(paths.Select(p => p.Target), StringComparer.OrdinalIgnoreCase);
        foreach (var old in obsolete) affected.Add(ResolveSafe(target, old.Path));
        var priorMarker = File.Exists(marker) ? File.ReadAllBytes(marker) : null;
        var existed = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        var madeDirectories = new List<string>();
        string? retainedBackup = null;
        var obsoleteOriginals = new List<string>();
        try
        {
            Directory.CreateDirectory(rollback);
            int snapshotIndex = 0;
            foreach (string path in affected)
            {
                if (File.Exists(path))
                {
                    string snapshot = Path.Combine(rollback, (snapshotIndex++).ToString(System.Globalization.CultureInfo.InvariantCulture));
                    File.Copy(path, snapshot, false); existed[path] = snapshot;
                }
                else existed[path] = null;
            }
            int completed = 0;
            foreach (var p in paths)
            {
                // Recheck immediately before writing, including reparse-point checks.
                ResolveSafe(target, p.Entry.Path);
                if (Directory.Exists(p.Target)) throw new IOException("Destination is a directory: " + p.Entry.Path);
                var absent = new Stack<string>();
                string? parent = System.IO.Path.GetDirectoryName(p.Target);
                while (parent != null && !Directory.Exists(parent)) { absent.Push(parent); parent = System.IO.Path.GetDirectoryName(parent); }
                foreach (string dir in absent) { Directory.CreateDirectory(dir); madeDirectories.Add(dir); }
                if (previousByPath.ContainsKey(p.Entry.Path) && MutableOwnedFile(p.Entry.Path) && File.Exists(p.Target))
                {
                    progress?.Report(new InstallProgress(++completed, paths.Length + obsolete.Length, p.Entry.Path));
                    continue; // Keep the user's settings and downloaded translation cache.
                }
                using (var source = File.OpenRead(p.Source))
                using (var dest = new FileStream(p.Target, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    source.CopyTo(dest);
                    dest.Flush(true);
                }
                if (!Sha(p.Target).Equals(p.Entry.Sha256, StringComparison.OrdinalIgnoreCase)) throw new IOException("Post-copy hash failed");
                progress?.Report(new InstallProgress(++completed, paths.Length + obsolete.Length, p.Entry.Path));
            }
            foreach (var old in obsolete)
            {
                string full = ResolveSafe(target, old.Path);
                if (!File.Exists(full)) continue;
                string? original = previous == null ? null : OriginalFilePath(previous, old.Path);
                if (original != null) File.Copy(original, full, true); else File.Delete(full);
                progress?.Report(new InstallProgress(++completed, paths.Length + obsolete.Length, old.Path));
            }
            var originalFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (previous?.BackupDirectory != null)
            {
                foreach (var old in previous.Files)
                {
                    if (!newPaths.Contains(old.Path)) continue;
                    string? original = OriginalFilePath(previous, old.Path);
                    if (original != null) originalFiles[old.Path] = Path.GetFileName(original);
                }
            }
            retainedBackup = originalFiles.Count > 0 ? previous?.BackupDirectory : null;
            File.WriteAllText(marker, JsonSerializer.Serialize(new InstallationRecord(manifest.Version, manifest.Files, retainedBackup, manifest.TargetProfile, originalFiles.Count > 0 ? originalFiles : null)));
            if (previous?.BackupDirectory != null)
            {
                foreach (var old in obsolete)
                {
                    string? original = OriginalFilePath(previous, old.Path);
                    if (original != null) obsoleteOriginals.Add(original);
                }
            }
        }
        catch (Exception installError)
        {
            var rollbackErrors = new List<Exception>();
            foreach (var pair in existed.Reverse())
            {
                try
                {
                    if (pair.Value != null) File.Copy(pair.Value, pair.Key, true);
                    else if (File.Exists(pair.Key)) File.Delete(pair.Key);
                }
                catch (Exception error) { rollbackErrors.Add(error); }
            }
            foreach (var dir in madeDirectories.AsEnumerable().Reverse())
            {
                try { Directory.Delete(dir, false); } catch (Exception error) { rollbackErrors.Add(error); }
            }
            try { if (priorMarker == null) File.Delete(marker); else File.WriteAllBytes(marker, priorMarker); } catch (Exception error) { rollbackErrors.Add(error); }
            if (rollbackErrors.Count != 0)
                throw new AggregateException("ติดตั้งหรืออัปเดตล้มเหลวและย้อนรายการที่แก้ไขได้ไม่ครบ", new[] { installError }.Concat(rollbackErrors));
            throw;
        }
        finally { try { if (Directory.Exists(rollback)) Directory.Delete(rollback, true); } catch { } }
        if (previous?.BackupDirectory != null)
        {
            if (retainedBackup == null) Directory.Delete(previous.BackupDirectory, true);
            else foreach (string original in obsoleteOriginals) if (File.Exists(original)) File.Delete(original);
        }
        // Remove only empty directories left by files that the prior ownership
        // marker assigned to this patch. Non-empty folders (other mods/user data)
        // are preserved because deletion is non-recursive.
        var obsoleteDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var old in obsolete)
        {
            for (string? folder = Path.GetDirectoryName(ResolveSafe(target, old.Path)); folder != null && folder != target; folder = Path.GetDirectoryName(folder))
                obsoleteDirectories.Add(folder);
        }
        foreach (string folder in obsoleteDirectories.OrderByDescending(p => p.Length))
        {
            try { Directory.Delete(folder, false); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }
}
