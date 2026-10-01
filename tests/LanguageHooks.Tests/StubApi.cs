// API stubs for callback/registration tests only. Never packaged into the plugin.
namespace BepInEx
{
 [AttributeUsage(AttributeTargets.Class)] public sealed class BepInPlugin : Attribute { public BepInPlugin(string id,string name,string version){} }
 public class BaseUnityPlugin { public Logging.ManualLogSource Logger=new();public FakeConfig Config=new(); }
 public static class Paths { public static string ConfigPath="not-used-by-callback-tests"; }
 public class FakeConfig { public FakeEntry<T> Bind<T>(string section,string key,T value,string description)=>new(){Value=value}; }
 public class FakeEntry<T> { public T Value=default!; }
}
namespace BepInEx.Logging
{ public class ManualLogSource { public List<string> Lines=new();public void LogInfo(object text)=>Lines.Add(text.ToString()!);public void LogWarning(object text)=>Lines.Add(text.ToString()!);public void LogError(object text)=>Lines.Add(text.ToString()!); } }
namespace HarmonyLib
{
 public static class Priority {public const int Last=0;}
 public sealed class HarmonyMethod {public System.Reflection.MethodInfo Method;public int priority;public HarmonyMethod(System.Reflection.MethodInfo method){Method=method;} }
 public sealed class Harmony
 {
  public readonly List<(System.Reflection.MethodInfo Original,HarmonyMethod? Prefix,HarmonyMethod? Postfix)> Hooks=new();
  public Harmony(string id){}
  public void Patch(System.Reflection.MethodInfo original,HarmonyMethod? prefix=null,HarmonyMethod? postfix=null)=>Hooks.Add((original,prefix,postfix));
 }
}
