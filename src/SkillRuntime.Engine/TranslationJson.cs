using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace RO3.ThaiLocalization
{
    // Explicit wire format: no serialization attributes, reflection, or game DLL replacement.
    internal static class TranslationJson
    {
        internal static T Parse<T>(string json)
        {
            var value = new Reader(json).Read();
            object result;
            if (typeof(T) == typeof(TranslationManifest)) result = Manifest(Object(value, "manifest"));
            else if (typeof(T) == typeof(TranslationCache))
            {
                var c = Object(value, "cache"); Keys(c, "Manifest", "Table", "Rules");
                result = new TranslationCache { Manifest = Manifest(Object(Get(c, "Manifest"), "Manifest")), Table = Text(c, "Table"), Rules = Text(c, "Rules") };
            }
            else throw new InvalidDataException("Unsupported wire type");
            return (T)result;
        }
        private static Dictionary<string, object?> Object(object? value, string name)
        { return value as Dictionary<string, object?> ?? throw new InvalidDataException("Expected JSON object: " + name); }
        private static object? Get(Dictionary<string, object?> o, string key)
        { object? v; if (!o.TryGetValue(key, out v)) throw new InvalidDataException("Missing JSON field: " + key); return v; }
        private static string Text(Dictionary<string, object?> o, string key)
        { return Get(o, key) as string ?? throw new InvalidDataException("Expected string: " + key); }
        private static int Number(Dictionary<string, object?> o, string key)
        { object? v = Get(o, key); if (!(v is int)) throw new InvalidDataException("Expected integer: " + key); return (int)v; }
        private static void Keys(Dictionary<string, object?> o, params string[] keys)
        { if (o.Count != keys.Length) throw new InvalidDataException("Unexpected JSON fields"); foreach (string key in keys) Get(o, key); }
        private static TranslationManifest Manifest(Dictionary<string, object?> o)
        {
            Keys(o, "Schema", "RuntimeSchema", "Version", "SourceSha256", "TranslationIds", "RuntimeRules", "Files");
            var values = Get(o, "Files") as List<object?> ?? throw new InvalidDataException("Expected files array");
            if (values.Count != 2) throw new InvalidDataException("Expected two data files");
            var files = new TranslationFile[2];
            for (int i = 0; i < 2; i++)
            {
                var f = Object(values[i], "file"); Keys(f, "Name", "Sha256", "Bytes");
                files[i] = new TranslationFile { Name = Text(f, "Name"), Sha256 = Text(f, "Sha256"), Bytes = Number(f, "Bytes") };
            }
            return new TranslationManifest { Schema = Number(o, "Schema"), RuntimeSchema = Number(o, "RuntimeSchema"), Version = Text(o, "Version"), SourceSha256 = Text(o, "SourceSha256"), TranslationIds = Number(o, "TranslationIds"), RuntimeRules = Number(o, "RuntimeRules"), Files = files };
        }
        private static string Quote(string s)
        {
            var b = new StringBuilder(s.Length + 2); b.Append('"');
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': b.Append("\\\""); break;
                    case '\\': b.Append("\\\\"); break;
                    case '\n': b.Append("\\n"); break;
                    case '\r': b.Append("\\r"); break;
                    case '\t': b.Append("\\t"); break;
                    default: if (c < 32) b.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture)); else b.Append(c); break;
                }
            }
            return b.Append('"').ToString();
        }
        internal static string WriteCache(TranslationCache c)
        {
            var m = c.Manifest; var b = new StringBuilder();
            b.Append("{\"Manifest\":{\"Schema\":").Append(m.Schema.ToString(CultureInfo.InvariantCulture));
            b.Append(",\"RuntimeSchema\":").Append(m.RuntimeSchema.ToString(CultureInfo.InvariantCulture));
            b.Append(",\"Version\":").Append(Quote(m.Version)).Append(",\"SourceSha256\":").Append(Quote(m.SourceSha256));
            b.Append(",\"TranslationIds\":").Append(m.TranslationIds.ToString(CultureInfo.InvariantCulture));
            b.Append(",\"RuntimeRules\":").Append(m.RuntimeRules.ToString(CultureInfo.InvariantCulture)).Append(",\"Files\":[");
            for (int i = 0; i < m.Files.Length; i++)
            {
                if (i > 0) b.Append(','); var f = m.Files[i];
                b.Append("{\"Name\":").Append(Quote(f.Name)).Append(",\"Sha256\":").Append(Quote(f.Sha256)).Append(",\"Bytes\":").Append(f.Bytes.ToString(CultureInfo.InvariantCulture)).Append('}');
            }
            b.Append("]},\"Table\":").Append(Quote(c.Table)).Append(",\"Rules\":").Append(Quote(c.Rules)).Append('}');
            return b.ToString();
        }
        private sealed class Reader
        {
            private readonly string input; private int position, nodes;
            internal Reader(string value) { input = value; }
            private void White() { while (position < input.Length && (input[position] == ' ' || input[position] == '\t' || input[position] == '\r' || input[position] == '\n')) position++; }
            private bool Take(char c) { White(); if (position < input.Length && input[position] == c) { position++; return true; } return false; }
            private void Need(char c) { if (!Take(c)) throw new InvalidDataException("Invalid JSON punctuation"); }
            internal object? Read() { var v = Value(0); White(); if (position != input.Length) throw new InvalidDataException("Trailing JSON data"); return v; }
            private object? Value(int depth)
            {
                if (depth > 16 || ++nodes > 10000) throw new InvalidDataException("JSON complexity limit");
                White(); if (position >= input.Length) throw new InvalidDataException("Truncated JSON"); char c = input[position];
                if (c == '"') return String();
                if (c == '{')
                {
                    position++; var o = new Dictionary<string, object?>(StringComparer.Ordinal); if (Take('}')) return o;
                    do { White(); if (position >= input.Length || input[position] != '"') throw new InvalidDataException("Expected JSON key"); string key = String(); Need(':'); var value = Value(depth + 1); if (o.ContainsKey(key)) throw new InvalidDataException("Duplicate JSON key"); o.Add(key, value); if (Take('}')) return o; Need(','); } while (true);
                }
                if (c == '[')
                {
                    position++; var a = new List<object?>(); if (Take(']')) return a;
                    do { a.Add(Value(depth + 1)); if (Take(']')) return a; Need(','); } while (true);
                }
                int first = position; if (c == '-') position++;
                if (position >= input.Length || input[position] < '0' || input[position] > '9') throw new InvalidDataException("Expected JSON integer");
                if (input[position] == '0') position++; else while (position < input.Length && input[position] >= '0' && input[position] <= '9') position++;
                int number; if (!int.TryParse(input.Substring(first, position - first), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out number)) throw new InvalidDataException("JSON integer out of range");
                return number;
            }
            private string String()
            {
                position++; var b = new StringBuilder(); bool closed = false;
                while (position < input.Length)
                {
                    char c = input[position++]; if (c == '"') { closed = true; break; }
                    if (c < 32) throw new InvalidDataException("Unescaped JSON control");
                    if (c == '\\')
                    {
                        if (position >= input.Length) throw new InvalidDataException("Truncated JSON escape"); c = input[position++];
                        switch (c)
                        {
                            case '"': case '\\': case '/': break;
                            case 'b': c = '\b'; break; case 'f': c = '\f'; break; case 'n': c = '\n'; break; case 'r': c = '\r'; break; case 't': c = '\t'; break;
                            case 'u':
                                if (position + 4 > input.Length) throw new InvalidDataException("Truncated Unicode escape");
                                int n; if (!int.TryParse(input.Substring(position, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out n)) throw new InvalidDataException("Invalid Unicode escape"); c = (char)n; position += 4; break;
                            default: throw new InvalidDataException("Invalid JSON escape");
                        }
                    }
                    b.Append(c);
                }
                if (!closed) throw new InvalidDataException("Unclosed JSON string");
                string s = b.ToString();
                for (int i = 0; i < s.Length; i++)
                    if (char.IsHighSurrogate(s[i])) { if (++i >= s.Length || !char.IsLowSurrogate(s[i])) throw new InvalidDataException("Invalid Unicode pair"); }
                    else if (char.IsLowSurrogate(s[i])) throw new InvalidDataException("Invalid Unicode pair");
                return s;
            }
        }
    }
}
