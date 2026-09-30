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
    [BepInPlugin("com.ro3.thailocalization.skills", "RO3 Thai Skills Partial", "0.2.0")]
    public sealed class SkillsPlugin : BaseUnityPlugin
    {
        private static SkillDictionary? dictionary;
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
                dictionary = SkillDictionary.Load(Path.Combine(Paths.ConfigPath, "RO3.SkillTranslations.tsv"), Path.Combine(Paths.ConfigPath, "RO3.SkillRules.tsv"));
                harmony = new Harmony("com.ro3.thailocalization.skills.hooks");
                log.LogInfo("Fresh English-only skill dictionary loaded: " + dictionary.Count + " IDs, " + dictionary.RuleCount + " rules. No old dictionaries or online lookup.");
                InstallHooks();
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
                var hook = new HarmonyMethod(typeof(SkillsPlugin).GetMethod(callback, BindingFlags.NonPublic | BindingFlags.Static));
                hook.priority = Priority.Last;
                if (postfix) harmony!.Patch(method, postfix: hook); else harmony!.Patch(method, prefix: hook);
                patched.Add(method);
                if (isId) idHooks++; else textHooks++;
                log?.LogInfo("Skill hook: " + method.DeclaringType?.FullName + "." + method.Name);
            }
            catch (Exception error) { log?.LogWarning("Cannot hook " + method.Name + ": " + error.Message); }
        }
        private static void LanguagePostfix(long __0, ref string __result)
        {
            if (dictionary == null || __result == null) return;
            try
            {
                string translated;
                if (dictionary.TryId(__0.ToString(CultureInfo.InvariantCulture), __result, out translated)) __result = translated;
            }
            catch { /* No lookup failure is allowed to break a game method. */ }
        }
        private static void TextPrefix(ref string __0)
        {
            if (translating || dictionary == null || __0 == null) return;
            try { translating = true; __0 = dictionary.Translate(__0); } catch { } finally { translating = false; }
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
                string next = dictionary.Translate(old);
                if (next != old) property.SetValue(__instance, next, null);
            }
            catch { } finally { translating = false; }
        }
    }
}
