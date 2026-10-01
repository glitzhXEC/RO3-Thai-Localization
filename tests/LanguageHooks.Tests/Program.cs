using System.Reflection;
using RO3.ThaiLocalization;
using BepInEx.Logging;
using HarmonyLib;
var plugin=typeof(SkillsPlugin);
void Set(string name,object? value)=>plugin.GetField(name,BindingFlags.Static|BindingFlags.NonPublic)!.SetValue(null,value);
object? Call(string name,params object?[] args)=>plugin.GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic)!.Invoke(null,args);
int checks=0;void Check(bool yes,string label){if(!yes)throw new Exception(label);checks++;}
var d=SkillDictionary.LoadText("1002\tConfirm\tยืนยัน\n27003\tLog\tLog\n", "", "1002\t确定\t確定\n27003\t日志\t日誌\n");
var log=new ManualLogSource();var hooks=new Harmony("callback-tests");
Set("dictionary",d);Set("gameThreadId",Environment.CurrentManagedThreadId);Set("log",log);Set("harmony",hooks);
Call("InstallHooks");
foreach(var name in new[]{"LanguagePostfix","LanguageCacheResetPostfix","ModuleRequirePostfix","LuaInitPostfix","LuaEnvPostfix","LuaTickPostfix"})Check(hooks.Hooks.Any(h=>h.Postfix?.Method.Name==name),"Expected registration "+name);
Check(hooks.Hooks.Count(h=>h.Original.DeclaringType==typeof(LA_LuaManager)&&h.Original.Name=="Obf_MD")==1,"Correct require overload registered once");
Call("InstallHooks");Check(hooks.Hooks.Count(h=>h.Original.Name=="Obf_MD")==1,"Repeated discovery does not double-patch");
var cn=new FakeLuaTable();cn.Values[1002L]="确定";cn.Values[27003L]="日志";var manager=new LA_LuaManager();
Call("ModuleRequirePostfix",manager,"Localization_zh_CN",new object[]{cn});Check((string)cn.Values[1002L]=="ยืนยัน"&&(string)cn.Values[27003L]=="Log","Actual require callback restores named table before return");
cn.Values[1002L]="确定";Task.Run(()=>Call("ModuleRequirePostfix",manager,"Localization_zh_CN",new object[]{cn})).GetAwaiter().GetResult();Check((string)cn.Values[1002L]=="确定","No Lua mutation on worker thread");
Call("ModuleRequirePostfix",manager,"Chat",new object[]{cn});Check((string)cn.Values[1002L]=="确定","Unrelated modules untouched");
var loaded=new FakeLuaTable();loaded.Values["Localization_zh_CN"]=cn;var package=new FakeLuaTable();package.Values["loaded"]=loaded;manager.Obf_Dc.Globals.Values["package"]=package;
Set("nextLuaProbe",0L);Set("luaProbePending",1);Call("LuaTickPostfix");Check((string)cn.Values[1002L]=="ยืนยัน","Cached package.loaded table repaired on game-thread refresh");
for(int i=0;i<5;i++)
{
 LanguageMain.Obf_FO();Call("LanguageCacheResetPostfix",typeof(LanguageMain).GetMethod("Obf_FO"));Check(LanguageMain.Obf_Pk[1002L]=="ยืนยัน","Repeated native cache resets repaired");
 cn.Values[1002L]="确定";Set("nextLuaProbe",0L);Call("LuaTickPostfix");Check((string)cn.Values[1002L]=="ยืนยัน","Reset reopens Lua module refresh");
}
object?[] lookup={1002L,"新版确认",typeof(LanguageMain).GetMethod("Obf_gO")};Call("LanguagePostfix",lookup);Check((string)lookup[1]! =="Confirm","Actual ID callback restores unmatched Chinese to English");
Check(log.Lines.Any(s=>s.Contains("review ID=1002")),"Review diagnostic contains ID");
object?[] chatText={"你好玩家"};Call("TextPrefix",chatText);Check((string)chatText[0]! =="你好玩家","Text setter leaves Chinese chat unchanged");
var newer=SkillDictionary.LoadText("1002\tConfirm\tตกลง\n27003\tLog\tLog\n", "", "1002\t确定\t確定\n27003\t日志\t日誌\n");
Set("previousDictionary",d);Set("dictionary",newer);Set("luaProbePending",1);Set("nextLuaProbe",0L);Call("LuaTickPostfix");Check((string)cn.Values[1002L]=="ตกลง","Data swap upgrades owned Lua target on game thread");
Check(!log.Lines.Any(s=>s.Contains("新版确认")||s.Contains("你好玩家")),"No raw input/player text in logs");
Console.WriteLine($"PASS: {checks} compiled-plugin callback/registration checks; mocked Harmony/Unity APIs, no game or actual IL patches.");
public sealed class FakeLuaTable
{
 public readonly Dictionary<object,object> Values=new();
 public object? Obf_MuA(string key)=>Values.TryGetValue(key,out var v)?v:null;
 public object? Obf_NuA(object key)=>Values.TryGetValue(key,out var v)?v:null;
 public void Obf_nuA(string key,object value)=>Values[key]=value;
 public void Obf_ouA(object key,object value)=>Values[key]=value;
 public object[] Obf_PuA()=>Values.Keys.ToArray();
}
public sealed class FakeEnv {public FakeLuaTable Globals=new();public object Obf_PTA()=>Globals;}
public sealed class LA_LuaManager
{
 public FakeEnv Obf_Dc=new();
 public object[] Obf_MD(string name)=>Array.Empty<object>();
 public FakeEnv Obf_jD()=>Obf_Dc;
 public void Obf_iD(object first,object second){}
}
public sealed class Obf_o {public void Obf_ie(){}}
public static class LanguageMain
{
 public static Dictionary<long,string> Obf_Pk=new();
 public static object? Obf_pk;
 public static string Obf_gO(long id)=>Obf_Pk.TryGetValue(id,out var v)?v:"确定";
 public static void Obf_FO()=>Obf_Pk.Clear();
}
