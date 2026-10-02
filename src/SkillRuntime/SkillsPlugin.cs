using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;

namespace RO3.ThaiLocalization
{
    [BepInPlugin("com.ro3.thailocalization.skills", "RO3 Thai Localization + Translation Updates", "0.4.3")]
    public sealed class SkillsPlugin : BaseUnityPlugin
    {
        private static SkillDictionary? dictionary;
        private static SkillDictionary? previousDictionary;
        private static int seedPending = 1, fallbackLogs;
        private static int gameThreadId, luaProbePending = 1;
        private static long nextLuaProbe;
        private static readonly List<object> luaEnvs = new List<object>();
        [ThreadStatic] private static bool patchingLua;
        private static Harmony? harmony;
        private static ManualLogSource? log;
        private static readonly HashSet<MethodBase> patched = new HashSet<MethodBase>();
        private static Timer? retry;
        private static int patching, attempts, idHooks, textHooks;
        [ThreadStatic] private static bool translating;
        private void Awake()
        {
            try
            {
                log = Logger;
                gameThreadId = Thread.CurrentThread.ManagedThreadId;
                dictionary = SkillDictionary.Load(Path.Combine(Paths.ConfigPath, "RO3.SkillTranslations.tsv"), Path.Combine(Paths.ConfigPath, "RO3.SkillRules.tsv"), Path.Combine(Paths.ConfigPath, "RO3.LanguageOrigins.tsv"));
                string cachedVersion = "";
                try
                {
                    var cached = TranslationUpdater.LoadCache(Paths.ConfigPath, out cachedVersion);
                    if (cached != null) dictionary = cached;
                }
                catch (Exception error) { log.LogWarning("Translation cache rejected; using embedded/offline files: " + error.Message); }
                harmony = new Harmony("com.ro3.thailocalization.skills.hooks");
                log.LogInfo("English-base localization dictionary loaded: " + dictionary.Count + " IDs (Thai differences=" + dictionary.ThaiCount + "), " + dictionary.RuleCount + " rules. No legacy dictionaries or online machine translation.");
                InstallHooks();
                foreach (Type type in FindTypes("LanguageMain")) SeedLanguageCache(type, "startup");
                bool autoUpdate = Config.Bind("Translations", "AutoUpdateOnStartup", true, "Check trusted GitHub main once on startup; downloads translation data only. Offline cache remains available.").Value;
                if (autoUpdate)
                {
                    string version = cachedVersion;
                    string configPath = Paths.ConfigPath;
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        var updated = TranslationUpdater.Refresh(configPath, version, message => log?.LogInfo(message));
                        if (updated != null)
                        {
                            Volatile.Write(ref previousDictionary, Volatile.Read(ref dictionary));
                            Volatile.Write(ref dictionary, updated); Interlocked.Exchange(ref seedPending, 1); Interlocked.Exchange(ref luaProbePending, 1);
                        }
                    });
                }
                AppDomain.CurrentDomain.AssemblyLoad += (_, __) => ThreadPool.QueueUserWorkItem(_ => InstallHooks());
                // Keep hooks alive when the game destroys bootstrap Unity components.
                retry = new Timer(_ => { if (++attempts > 60) { retry?.Dispose(); log?.LogInfo("Hook discovery ended: ID hooks=" + idHooks + ", text hooks=" + textHooks); return; } InstallHooks(); }, null, 2000, 2000);
            }
            catch (Exception error) { Logger.LogError("Skills patch initialization failed; leave text unchanged: " + error.Message); }
        }
        private static IEnumerable<Type> FindTypes(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? found = null;
                try { found = assembly.GetType(name, false); } catch { }
                if (found != null) { yield return found; continue; }
                if (name.IndexOf('.') >= 0) continue;
                Type?[] candidates;
                try { candidates = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException error) { candidates = error.Types; }
                catch { continue; }
                foreach (Type? candidate in candidates)
                    if (candidate != null && candidate.Name == name) yield return candidate;
            }
        }
        private static void InstallHooks()
        {
            if (harmony == null || dictionary == null || Interlocked.CompareExchange(ref patching, 1, 0) != 0) return;
            try
            {
                foreach (Type type in FindTypes("LanguageMain"))
                {
                    MethodInfo? method = type.GetMethod("Obf_gO", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(long) }, null);
                    if (method != null && method.ReturnType == typeof(string)) Patch(method, "LanguagePostfix", true, true);
                    var reset = type.GetMethod("Obf_FO", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);
                    if (reset != null) Patch(reset, "LanguageCacheResetPostfix", true, true);
                }
                foreach (Type type in FindTypes("LA_LuaManager"))
                {
                    var require = type.GetMethod("Obf_MD", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(string) }, null);
                    if (require != null && require.ReturnType == typeof(object[])) Patch(require, "ModuleRequirePostfix", true, false);
                    var getter = type.GetMethod("Obf_jD", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
                    if (getter != null && !getter.ReturnType.IsValueType && getter.ReturnType != typeof(void)) Patch(getter, "LuaEnvPostfix", true, false);
                    foreach (var init in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                        if (init.Name == "Obf_iD" && init.GetParameters().Length == 2) Patch(init, "LuaInitPostfix", true, false);
                }
                foreach (Type type in FindTypes("Obf_o"))
                {
                    var tick = type.GetMethod("Obf_ie", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
                    if (tick != null) Patch(tick, "LuaTickPostfix", true, false);
                }
                foreach (string name in new[] { "TMPro.TMP_Text", "UnityEngine.UI.Text", "HUDUber.Text", "HUDUber.Graphic", "MTextData", "TextMeshBuilder" })
                    foreach (Type type in FindTypes(name))
                        foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                        {
                            ParameterInfo[] args = method.GetParameters();
                            bool setter = method.Name == "SetText" || method.Name == "set_text" || (name == "MTextData" && method.Name == "set_Context") || (name == "TextMeshBuilder" && method.Name == "Append");
                            if (setter && args.Length > 0 && args[0].ParameterType == typeof(string) && !method.IsAbstract) Patch(method, "TextPrefix", false, false);
                            if ((method.Name == "OnEnable" || method.Name == "ParseInputText" || method.Name == "OnPopulateMesh") && !method.IsAbstract) Patch(method, "RefreshPrefix", false, false);
                        }
            }
            catch (Exception error) { log?.LogWarning("Hook discovery incomplete: " + error.Message); }
            finally { Volatile.Write(ref patching, 0); }
        }
        private static void Patch(MethodInfo method, string callback, bool postfix, bool isId)
        {
            if (patched.Contains(method)) return;
            try
            {
                var callbackMethod = typeof(SkillsPlugin).GetMethod(callback, BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException("Missing patch callback " + callback);
                var hook = new HarmonyMethod(callbackMethod);
                hook.priority = Priority.Last;
                if (postfix) harmony!.Patch(method, postfix: hook); else harmony!.Patch(method, prefix: hook);
                patched.Add(method);
                if (isId) idHooks++; else textHooks++;
                log?.LogInfo("Skill hook: " + method.DeclaringType?.FullName + "." + method.Name);
            }
            catch (Exception error) { log?.LogWarning("Cannot hook " + method.Name + ": " + error.Message); }
        }
        private static void SeedLanguageCache(Type type, string reason)
        {
            var current = Volatile.Read(ref dictionary); if (current == null) return;
            try
            {
                var field = type.GetField("Obf_Pk", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                var native = field?.GetValue(null) as Dictionary<long, string>;
                if (native == null) return;
                int changed = current.SeedCache(native, Volatile.Read(ref previousDictionary));
                Interlocked.Exchange(ref seedPending, 0);
                log?.LogInfo("English-base cache seeded after " + reason + ": changed=" + changed + ", source IDs=" + current.Count);
            }
            catch (Exception e) { log?.LogWarning("Cache seed skipped; verified ID hook stays active: " + e.Message); }
        }
        private static void LanguageCacheResetPostfix(MethodBase __originalMethod)
        {
            Interlocked.Exchange(ref luaProbePending, 1);
            if (Thread.CurrentThread.ManagedThreadId == gameThreadId && __originalMethod.DeclaringType != null) SeedLanguageCache(__originalMethod.DeclaringType, "language cache reset");
            CaptureLanguageEnv(__originalMethod.DeclaringType);
        }
        private static void LanguagePostfix(long __0, ref string __result, MethodBase __originalMethod)
        {
            if (dictionary == null || __result == null) return;
            try
            {
                if (Thread.CurrentThread.ManagedThreadId == gameThreadId && Volatile.Read(ref seedPending) != 0 && __originalMethod.DeclaringType != null) SeedLanguageCache(__originalMethod.DeclaringType, "ID lookup (game thread)");
                CaptureLanguageEnv(__originalMethod.DeclaringType);
                var current = Volatile.Read(ref dictionary); if (current == null) return;
                string translated;
                bool englishFallback;
                if (current.TryLanguageId(__0.ToString(CultureInfo.InvariantCulture), __result, out translated, out englishFallback))
                {
                    if (__result != translated && SkillDictionary.HasChinese(__result) && Interlocked.Increment(ref fallbackLogs) <= 16)
                        log?.LogInfo((englishFallback ? "Unverified Chinese restored to trusted English; review ID=" : "Verified language fallback restored by ID=") + __0.ToString(CultureInfo.InvariantCulture));
                    __result = translated;
                }
                else if (SkillDictionary.HasChinese(__result) && Interlocked.Increment(ref fallbackLogs) <= 16)
                    log?.LogWarning("Unmatched Chinese lookup kept unchanged (not guessed), ID=" + __0.ToString(CultureInfo.InvariantCulture));
            }
            catch { /* No lookup failure is allowed to break a game method. */ }
        }
        private static object? Field(object? instance, string name)
        {
            if (instance == null) return null;
            for (Type? t = instance.GetType(); t != null; t = t.BaseType)
            {
                var f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (f != null) return f.GetValue(instance);
            }
            return null;
        }
        private static void Capture(object? env)
        {
            if (env == null) return;
            lock (luaEnvs)
            {
                foreach (object known in luaEnvs) if (ReferenceEquals(known, env)) return;
                if (luaEnvs.Count >= 8) { log?.LogWarning("Additional Lua environment not retained; direct named-module hook remains active."); return; }
                luaEnvs.Add(env); Interlocked.Exchange(ref luaProbePending, 1);
            }
        }
        private static void CaptureLanguageEnv(Type? type)
        {
            if (type == null) return;
            try
            {
                var f = type.GetField("Obf_pk", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                Capture(Field(f?.GetValue(null), "Obf_blA"));
            }
            catch { }
        }
        private static void ApplyModule(string name, object? table, string reason)
        {
            var current = Volatile.Read(ref dictionary); if (current == null) return;
            var r = LocalizationTableBridge.PatchModule(name, table, current, Volatile.Read(ref previousDictionary));
            if (r.Tables == 0) return;
            log?.LogInfo("Language table " + name + " after " + reason + ": changed=" + r.Changed + ", unverified-English=" + r.EnglishRestored + ", preserved=" + r.Preserved + ", unknown-IDs=" + r.UnknownIds + ", errors=" + r.Errors);
            if (r.ReviewIds.Count > 0) log?.LogWarning("Language IDs requiring source review: " + String.Join(",", r.ReviewIds));
        }
        private static void ModuleRequirePostfix(object __instance, string __0, object[] __result)
        {
            try { Capture(Field(__instance, "Obf_Dc")); } catch { }
            if (patchingLua || Thread.CurrentThread.ManagedThreadId != gameThreadId || !LocalizationTableBridge.IsLanguageModule(__0)) return;
            try
            {
                patchingLua = true;
                if (__result != null) foreach (object value in __result) ApplyModule(__0, value, "require");
                Interlocked.Exchange(ref luaProbePending, 1);
            }
            catch (Exception e) { log?.LogWarning("Named language module patch deferred: " + e.Message); }
            finally { patchingLua = false; }
        }
        private static void LuaEnvPostfix(object __result) { try { Capture(__result); } catch { } }
        private static void LuaInitPostfix(object __instance) { try { Capture(Field(__instance, "Obf_Dc")); Interlocked.Exchange(ref luaProbePending, 1); } catch { } }
        private static void LuaTickPostfix()
        {
            if (patchingLua || Thread.CurrentThread.ManagedThreadId != gameThreadId || Volatile.Read(ref luaProbePending) == 0) return;
            long now = DateTime.UtcNow.Ticks;
            if (now < Interlocked.Read(ref nextLuaProbe)) return;
            try
            {
                patchingLua = true;
                object[] envs; lock (luaEnvs) envs = luaEnvs.ToArray();
                int found = 0;
                foreach (object env in envs)
                    foreach (var module in LocalizationTableBridge.LoadedModules(env)) { ApplyModule(module.Key, module.Value, "game-thread refresh"); found++; }
                if (found > 0) Interlocked.Exchange(ref luaProbePending, 0);
                // No Lua work from timers/network callbacks; retry unavailable globals
                // after initialization without probing every frame.
                Interlocked.Exchange(ref nextLuaProbe, now + TimeSpan.FromSeconds(2).Ticks);
            }
            catch (Exception e) { log?.LogWarning("Language table refresh deferred: " + e.Message); }
            finally { patchingLua = false; }
        }
        private static void TextPrefix(ref string __0)
        {
            if (translating || dictionary == null || __0 == null) return;
            // Generic text setters also carry player chat. Only allow O(1) whole-string
            // matches here; ID hooks handle templates, so chat never scans every regex rule.
            try { translating = true; __0 = dictionary.TranslateKnownText(__0); } catch { } finally { translating = false; }
        }
        private static void RefreshPrefix(object __instance)
        {
            if (translating || dictionary == null || __instance == null) return;
            try
            {
                translating = true;
                PropertyInfo? property = __instance.GetType().GetProperty("text", BindingFlags.Public | BindingFlags.Instance);
                if (property == null || property.PropertyType != typeof(string) || !property.CanRead || !property.CanWrite) return;
                string? old = property.GetValue(__instance, null) as string;
                if (old == null) return;
                string next = dictionary.TranslateKnownText(old);
                if (next != old) property.SetValue(__instance, next, null);
            }
            catch { } finally { translating = false; }
        }
    }
}
