using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace RO3.ThaiLocalization
{
    [DataContract] public sealed class TranslationFile
    {
        [DataMember(IsRequired = true)] public string Name = "";
        [DataMember(IsRequired = true)] public string Sha256 = "";
        [DataMember(IsRequired = true)] public int Bytes;
    }
    [DataContract] public sealed class TranslationManifest
    {
        [DataMember(IsRequired = true)] public int Schema;
        [DataMember(IsRequired = true)] public int RuntimeSchema;
        [DataMember(IsRequired = true)] public string Version = "";
        [DataMember(IsRequired = true)] public string SourceSha256 = "";
        [DataMember(IsRequired = true)] public int TranslationIds;
        [DataMember(IsRequired = true)] public int RuntimeRules;
        [DataMember(IsRequired = true)] public TranslationFile[] Files = Array.Empty<TranslationFile>();
    }
    [DataContract] public sealed class TranslationCache
    {
        [DataMember(IsRequired = true)] public TranslationManifest Manifest = null!;
        [DataMember(IsRequired = true)] public string Table = "";
        [DataMember(IsRequired = true)] public string Rules = "";
    }
    public static class TranslationUpdater
    {
        public const string ManifestPath = "main/translations/live/manifest.json";
        public const string CacheRelativePath = "RO3.TranslationCache/cache.json";
        private const string TrustedOrigin = "https://raw.githubusercontent.com/glitzhXEC/RO3-Thai-Localization/";
        private const int MaxFileBytes = 8 * 1024 * 1024, MaxCacheBytes = 24 * 1024 * 1024;
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false, true);
        public static string Hash(byte[] value) { using (var h = SHA256.Create()) return BitConverter.ToString(h.ComputeHash(value)).Replace("-", "").ToLowerInvariant(); }
        private static bool IsHash(string s) { return s != null && Regex.IsMatch(s, @"\A[0-9a-f]{64}\z"); }
        public static void ValidateManifest(TranslationManifest m)
        {
            if (m == null || m.Schema != 1 || m.RuntimeSchema != 1 || !IsHash(m.Version) || !IsHash(m.SourceSha256) ||
                m.TranslationIds <= 0 || m.TranslationIds > 50000 || m.RuntimeRules < 0 || m.RuntimeRules > 50000 || m.Files == null || m.Files.Length != 2)
                throw new InvalidDataException("Unsupported translation manifest/schema");
            string[] names = { "RO3.LocalizationOverrides.tsv", "RO3.LocalizationRules.tsv" };
            for (int i = 0; i < 2; i++)
                if (m.Files[i] == null || m.Files[i].Name != names[i] || !IsHash(m.Files[i].Sha256) || m.Files[i].Bytes < 0 || m.Files[i].Bytes > MaxFileBytes || (i == 0 && m.Files[i].Bytes == 0))
                    throw new InvalidDataException("Unapproved translation data file");
            string version = Hash(Utf8.GetBytes(m.Files[0].Sha256 + "|" + m.Files[1].Sha256));
            if (version != m.Version) throw new InvalidDataException("Manifest version/hash mismatch");
        }
        public static string FilePath(TranslationManifest m, int index)
        { ValidateManifest(m); if (index < 0 || index > 1) throw new ArgumentOutOfRangeException(nameof(index)); return "main/translations/live/versions/" + m.Version + "/" + m.Files[index].Name; }
        private static T Parse<T>(byte[] bytes, int maximum)
        {
            if (bytes.Length > maximum) throw new InvalidDataException("JSON size limit exceeded");
            // Reject malformed UTF-8 even when the serializer would replace it.
            Utf8.GetString(bytes);
            using (var stream = new MemoryStream(bytes)) return (T)(new DataContractJsonSerializer(typeof(T)).ReadObject(stream) ?? throw new InvalidDataException("Empty JSON"));
        }
        private static byte[] Serialize<T>(T value)
        { using (var stream = new MemoryStream()) { new DataContractJsonSerializer(typeof(T)).WriteObject(stream, value); return stream.ToArray(); } }
        public static SkillDictionary ValidateBundle(TranslationManifest m, byte[] table, byte[] rules)
        {
            ValidateManifest(m);
            byte[][] files = { table, rules };
            for (int i = 0; i < 2; i++)
                if (files[i].Length != m.Files[i].Bytes || Hash(files[i]) != m.Files[i].Sha256) throw new InvalidDataException("Translation integrity check failed");
            SkillDictionary dictionary = SkillDictionary.LoadText(Utf8.GetString(table), Utf8.GetString(rules));
            if (dictionary.Count != m.TranslationIds || dictionary.RuleCount != m.RuntimeRules) throw new InvalidDataException("Translation counts mismatch");
            return dictionary;
        }
        private static string SafeCachePath(string config)
        {
            string root = Path.GetFullPath(config);
            string folder = Path.Combine(root, "RO3.TranslationCache"), path = Path.Combine(folder, "cache.json");
            for (string? current = path; current != null; current = Path.GetDirectoryName(current))
            {
                if ((File.Exists(current) || Directory.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) throw new InvalidDataException("Reparse cache path rejected");
                if (current == root) break;
            }
            return path;
        }
        public static SkillDictionary? LoadCache(string config, out string version)
        {
            version = "";
            string path = SafeCachePath(config);
            if (!File.Exists(path)) return null;
            if (new FileInfo(path).Length > MaxCacheBytes) throw new InvalidDataException("Cache too large");
            TranslationCache c = Parse<TranslationCache>(File.ReadAllBytes(path), MaxCacheBytes);
            var dictionary = ValidateBundle(c.Manifest, Utf8.GetBytes(c.Table), Utf8.GetBytes(c.Rules));
            version = c.Manifest.Version;
            return dictionary;
        }
        public static void SaveCache(string config, TranslationManifest manifest, byte[] table, byte[] rules)
        {
            ValidateBundle(manifest, table, rules);
            string path = SafeCachePath(config);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            byte[] contents = Serialize(new TranslationCache { Manifest = manifest, Table = Utf8.GetString(table), Rules = Utf8.GetString(rules) });
            if (contents.Length > MaxCacheBytes) throw new InvalidDataException("Cache too large");
            string temp = path + ".download-" + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var file = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None)) { file.Write(contents, 0, contents.Length); file.Flush(true); }
                SafeCachePath(config); // Recheck immediately before replacing data.
                if (File.Exists(path)) File.Replace(temp, path, null); else File.Move(temp, path);
            }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }
        public static byte[] FetchTrusted(string relative, int maximum)
        {
            if (relative != ManifestPath && !Regex.IsMatch(relative, @"\Amain/translations/live/versions/[0-9a-f]{64}/RO3\.Localization(?:Overrides|Rules)\.tsv\z")) throw new InvalidDataException("Untrusted update path");
#pragma warning disable SYSLIB0014
            var request = (HttpWebRequest)WebRequest.Create(TrustedOrigin + relative);
#pragma warning restore SYSLIB0014
            request.AllowAutoRedirect = false; request.Timeout = 15000; request.ReadWriteTimeout = 15000;
            request.UserAgent = "RO3-Thai-Localization/0.3 (translation-data-only)";
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK || response.ContentLength > maximum) throw new InvalidDataException("Unexpected translation download response");
                using (var input = response.GetResponseStream())
                using (var output = new MemoryStream())
                {
                    if (input == null) throw new InvalidDataException("Empty download");
                    byte[] buffer = new byte[16384]; int read;
                    while ((read = input.Read(buffer, 0, buffer.Length)) != 0) { if (output.Length + read > maximum) throw new InvalidDataException("Download size limit exceeded"); output.Write(buffer, 0, read); }
                    return output.ToArray();
                }
            }
        }
        public static SkillDictionary? Refresh(string config, string currentVersion, Action<string> report, Func<string, int, byte[]>? fetch = null)
        {
            try
            {
                var download = fetch ?? FetchTrusted;
                TranslationManifest manifest = Parse<TranslationManifest>(download(ManifestPath, 32768), 32768);
                ValidateManifest(manifest);
                if (manifest.Version == currentVersion) { report("Translations already current: " + currentVersion); return null; }
                byte[] table = download(FilePath(manifest, 0), manifest.Files[0].Bytes);
                byte[] rules = download(FilePath(manifest, 1), manifest.Files[1].Bytes);
                SkillDictionary dictionary = ValidateBundle(manifest, table, rules);
                SaveCache(config, manifest, table, rules);
                report("Translation data updated: " + manifest.Version + "; " + dictionary.Count + " IDs / " + dictionary.RuleCount + " rules. No DLL/EXE download.");
                return dictionary;
            }
            catch (Exception error) { report("Translation update skipped; keeping last validated/offline dictionary: " + error.Message); return null; }
        }
    }
}
