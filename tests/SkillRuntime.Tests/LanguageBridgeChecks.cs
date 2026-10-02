using RO3.ThaiLocalization;
public static class LanguageBridgeChecks
{
 public static int Run()
 {
  int checks=0;void Check(bool yes,string message){if(!yes)throw new Exception(message);checks++;}
  var d=SkillDictionary.LoadText("1002\tConfirm\tยืนยัน\n27003\tLog\tLog\n11000\tUnlock ${1} slots\tปลดล็อก ${1} ช่อง\n", "", "1002\t确定\t確定\n27003\t日志\t日誌\n11000\t解锁 ${1} 个格子\t解鎖 ${1} 個格子\n");
  Check(!d.TryId("1002","另一个文字",out _),"Strict generic ID API still rejects unknown Chinese");
  Check(d.TryLanguageId("1002","新版确认",out var restored,out var fallback)&&restored=="Confirm"&&fallback,"Trusted language API restores unmatched Chinese to English, not guessed Thai");
  Check(d.TryLanguageId("1002","确定",out restored,out fallback)&&restored=="ยืนยัน"&&!fallback,"Verified Chinese uses approved Thai");
  Check(!d.TryLanguageId("9999","新文字",out _,out _),"Unknown IDs preserved");
  Check(!d.TryLanguageId("1002","Changed English",out _,out _),"New English preserved");
  Check(!d.TryLanguageId("11000","解锁 ${2} 个格子",out _,out _),"Changed placeholder shape preserved");
  Check(!d.TryLanguageId("11000","解锁 ${1} 个格子\n第二行",out _,out _),"Changed line layout preserved");
  Check(d.Translate("确定")=="ยืนยัน"&&d.Translate("你好玩家")=="你好玩家","Exact approved Chinese UI strings translate; arbitrary player text stays unchanged");
  var ambiguousText=SkillDictionary.LoadText("1002\tConfirm\tยืนยัน\n1003\tCancel\tยกเลิก\n", "", "1002\t相同\t相同\n1003\t相同\t相同\n");
  Check(ambiguousText.Translate("相同")=="相同","Conflicting exact Chinese source is left unchanged");
  Check(!SkillDictionary.HasChinese("New English (╯▔皿▔)╯")&&!d.TryLanguageId("1002","New English (╯▔皿▔)╯",out _,out _),"Chinese-looking emoticon does not overwrite changed English");
  var multiline=SkillDictionary.LoadText("11001\tUse ${1}\\nConfirm\tใช้ ${1}\\nยืนยัน\n", "");
  Check(multiline.TryLanguageId("11001","新版 ${1}\\n确认",out var wireFallback,out _)&&wireFallback=="Use ${1}\\nConfirm","Unknown Chinese wire controls retain escaped output style");
  Check(multiline.TryLanguageId("11001","新版 ${1}\n确认",out var plainFallback,out _)&&plainFallback=="Use ${1}\nConfirm","Unknown Chinese decoded controls retain physical output style");
  var native=new Dictionary<long,string>{{1002,"新版确认"},{27003,"日志"},{9999,"新玩家"}};
  d.SeedCache(native);Check(native[1002]=="Confirm"&&native[27003]=="Log"&&native[9999]=="新玩家","Native cache falls back to known English only");
  foreach(string module in new[]{"Localization_en","Localization_zh_CN","Localization_zh_TW"})
   foreach(int mode in new[]{0,1,2})
   {
    object Key(long id)=>mode==0?id.ToString():mode==1?(object)id:(double)id;
    var t=new FakeLuaTable();t.Values[Key(1002)]="确定";t.Values[Key(27003)]="日志";t.Values[Key(11000)]="解锁 ${1} 个格子";t.Values[Key(9999)]="保留未知";
    var r=LocalizationTableBridge.PatchModule(module,t,d);
    Check(r.Changed==3&&r.UnknownIds==1&&r.Errors==0,"Named module numeric/string keys "+module+mode);
    Check((string)t.Values[Key(1002)]=="ยืนยัน"&&(string)t.Values[Key(27003)]=="Log"&&(string)t.Values[Key(9999)]=="保留未知","Module values and unknown preservation");
    for(int i=0;i<4;i++){t.Values[Key(1002)]="确定";LocalizationTableBridge.PatchModule(module,t,d);Check((string)t.Values[Key(1002)]=="ยืนยัน","Repeated Chinese reload repaired");}
    t.Values[Key(1002)]="新版确认";LocalizationTableBridge.PatchModule(module,t,d);Check((string)t.Values[Key(1002)]=="Confirm","Unverified variant restores English first");
    t.Values[Key(1002)]="Changed newer English";LocalizationTableBridge.PatchModule(module,t,d);Check((string)t.Values[Key(1002)]=="Changed newer English","Changed game English not overwritten");
   }
  var chat=new FakeLuaTable();chat.Values[1002L]="确定";
  Check(LocalizationTableBridge.PatchModule("Chat",chat,d).Tables==0&&(string)chat.Values[1002L]=="确定","Untrusted Lua modules untouched");
  var nested=new FakeLuaTable();nested.Values["text"]=new FakeLuaTable();((FakeLuaTable)nested.Values["text"]).Values[1002L]="确定";nested.Values["loop"]=nested;
  Check(LocalizationTableBridge.PatchModule("Localization_en",nested,d).Changed==1,"Nested named table and cycles bounded");
  var env=new FakeEnv();var loaded=new FakeLuaTable();loaded.Values["Localization_zh_CN"]=nested;loaded.Values["Chat"]=chat;var package=new FakeLuaTable();package.Values["loaded"]=loaded;env.Globals.Values["package"]=package;
  Check(LocalizationTableBridge.LoadedModules(env).Count()==1,"Only allowlisted package.loaded modules discovered");
  var newer=SkillDictionary.LoadText("1002\tConfirm\tตกลง\n", "", "1002\t确定\t確定\n");
  var prior=new FakeLuaTable();prior.Values[1002L]="ยืนยัน";Check(LocalizationTableBridge.PatchModule("Localization_en",prior,newer,d).Changed==1&&(string)prior.Values[1002L]=="ตกลง","Owned prior Thai upgraded by ID in constant time");
  prior.Values[1002L]="คำไทยจากม็อดอื่น";LocalizationTableBridge.PatchModule("Localization_en",prior,newer,d);Check((string)prior.Values[1002L]=="คำไทยจากม็อดอื่น","Unowned native/Thai text preserved");
  // Exercise complete-table bounds and check that recovery files aren't involved.
  var large=new FakeLuaTable();for(long id=1000;id<35000;id++)large.Values[id]="未知";
  var summary=LocalizationTableBridge.PatchModule("Localization_zh_CN",large,d);
  Check(summary.Tables==1&&summary.Changed==2&&summary.ReviewIds.Count<=16,"Large table and review log limits");
  Console.WriteLine("PASS: "+checks+" language bridge/fallback checks; mocked Lua APIs, no game executed.");return checks;
 }
 public sealed class FakeLuaTable
 {
  public readonly Dictionary<object,object> Values=new();
  public object? Obf_MuA(string key)=>Values.TryGetValue(key,out var v)?v:null;
  public object? Obf_NuA(object key)=>Values.TryGetValue(key,out var v)?v:null;
  public void Obf_nuA(string key,object value)=>Values[key]=value;
  public void Obf_ouA(object key,object value)=>Values[key]=value;
  public object[] Obf_PuA()=>Values.Keys.ToArray();
 }
 public sealed class FakeEnv { public FakeLuaTable Globals=new(); public object Obf_PTA()=>Globals; }
}
