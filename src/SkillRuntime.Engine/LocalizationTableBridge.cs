using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace RO3.ThaiLocalization
{
    // Reflection-only bridge: no Lua code execution, loaders, arbitrary global scans,
    // recovery-file edits, language-choice edits or serialization attributes.
    public static class LocalizationTableBridge
    {
        public sealed class Result
        {
            public int Tables, Changed, EnglishRestored, Preserved, Errors, UnknownIds;
            public readonly List<string> ReviewIds = new List<string>();
        }
        public static bool IsLanguageModule(string name)
        { return name == "Localization_en" || name == "Localization_zh_CN" || name == "Localization_zh_TW"; }
        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public new bool Equals(object? a, object? b) { return ReferenceEquals(a, b); }
            public int GetHashCode(object value) { return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value); }
        }
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static MethodInfo? Method(object table, string name, params Type[] parameters)
        { return table.GetType().GetMethod(name, Flags, null, parameters, null); }
        private static bool IsTable(object? table)
        { return table != null && !(table is string) && !table.GetType().IsPrimitive && Method(table, "Obf_NuA", typeof(object)) != null && Method(table, "Obf_MuA", typeof(string)) != null && Method(table, "Obf_ouA", typeof(object), typeof(object)) != null && Method(table, "Obf_nuA", typeof(string), typeof(object)) != null && Method(table, "Obf_PuA") != null; }
        private static object? Read(object table, object key)
        {
            var method = key is string ? Method(table, "Obf_MuA", typeof(string)) : Method(table, "Obf_NuA", typeof(object));
            return method?.Invoke(table, new[] { key });
        }
        private static void Write(object table, object key, string value)
        {
            var method = key is string ? Method(table, "Obf_nuA", typeof(string), typeof(object)) : Method(table, "Obf_ouA", typeof(object), typeof(object));
            method!.Invoke(table, new object[] { key, value });
        }
        private static string? Id(object key)
        {
            if (key is string text)
            {
                if (text.Length < 4 || text.Length > 11) return null;
                foreach (char c in text) if (c < '0' || c > '9') return null;
                return text;
            }
            if (!(key is long) && !(key is int) && !(key is double)) return null;
            double value = Convert.ToDouble(key, CultureInfo.InvariantCulture);
            if (Double.IsNaN(value) || Double.IsInfinity(value) || value < 1000 || value > 99999999999 || value != Math.Floor(value)) return null;
            return ((long)value).ToString(CultureInfo.InvariantCulture);
        }
        public static Result PatchModule(string moduleName, object? module, SkillDictionary dictionary, SkillDictionary? previous = null)
        {
            var result = new Result();
            if (!IsLanguageModule(moduleName) || !IsTable(module)) return result;
            var queue = new Queue<KeyValuePair<object, int>>();
            var seen = new HashSet<object>(new ReferenceComparer());
            queue.Enqueue(new KeyValuePair<object, int>(module!, 0));
            while (queue.Count > 0 && result.Tables < 32)
            {
                var next = queue.Dequeue(); object table = next.Key;
                if (!seen.Add(table)) continue;
                result.Tables++;
                try
                {
                    var keys = Method(table, "Obf_PuA")!.Invoke(table, null) as IEnumerable;
                    if (keys == null) continue;
                    // Snapshot bounded keys before writes; don't mutate an iterator.
                    var snapshot = new List<object>();
                    foreach (object key in keys) { if (snapshot.Count >= 50000) break; snapshot.Add(key); }
                    foreach (object key in snapshot)
                    {
                        try
                        {
                            object? value = Read(table, key);
                            if (IsTable(value))
                            {
                                if (next.Value < 3 && queue.Count + result.Tables < 32) queue.Enqueue(new KeyValuePair<object, int>(value!, next.Value + 1));
                                continue;
                            }
                            string? id = Id(key);
                            if (id == null || !(value is string input)) continue;
                            string target; bool english;
                            if (!dictionary.TryLanguageId(id, input, out target!, out english))
                            {
                                if (!dictionary.TryUpdatedTarget(id, input, previous, out target!))
                                {
                                    if (!dictionary.ContainsId(id)) result.UnknownIds++;
                                    result.Preserved++; Review(result, id); continue;
                                }
                                english = false;
                            }
                            if (target == input) continue;
                            Write(table, key, target);
                            if ((Read(table, key) as string) != target) { result.Errors++; continue; }
                            result.Changed++;
                            if (english) { result.EnglishRestored++; Review(result, id); }
                        }
                        catch { result.Errors++; }
                    }
                }
                catch { result.Errors++; }
            }
            return result;
        }
        private static void Review(Result result, string id)
        { if (result.ReviewIds.Count < 16 && !result.ReviewIds.Contains(id)) result.ReviewIds.Add(id); }
        public static IEnumerable<KeyValuePair<string, object>> LoadedModules(object? env)
        {
            if (env == null) yield break;
            object? globals = null;
            try { globals = Method(env, "Obf_PTA")?.Invoke(env, null); } catch { }
            if (!IsTable(globals)) yield break;
            object? loaded = null;
            try { var package = Read(globals!, "package"); if (IsTable(package)) loaded = Read(package!, "loaded"); } catch { }
            foreach (string name in new[] { "Localization_en", "Localization_zh_CN", "Localization_zh_TW" })
            {
                object? direct = null, cached = null;
                try { direct = Read(globals!, name); if (IsTable(loaded)) cached = Read(loaded!, name); } catch { }
                if (IsTable(direct)) yield return new KeyValuePair<string, object>(name, direct!);
                if (IsTable(cached) && !ReferenceEquals(cached, direct)) yield return new KeyValuePair<string, object>(name, cached!);
            }
        }
    }
}
