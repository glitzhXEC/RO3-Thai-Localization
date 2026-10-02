using RO3.ThaiLocalization;
using System.Text.RegularExpressions;
string root=Path.GetFullPath(args.Length>0?args[0]:".");
string config=Path.Combine(root,"runtime-build/payload/BepInEx/config");
var dict=SkillDictionary.Load(Path.Combine(config,"RO3.SkillTranslations.tsv"),Path.Combine(config,"RO3.SkillRules.tsv"),Path.Combine(config,"RO3.LanguageOrigins.tsv"));
var token=new Regex(@"[$@^]\{\d+\}");
int checks=0;
void Check(bool yes,string context) { if(!yes)throw new Exception(context);checks++; }
var rows=File.ReadAllLines(Path.Combine(config,"RO3.SkillTranslations.tsv")).Select(s=>s.Split('\t',3)).ToDictionary(r=>r[0]);
Check(dict.Count==rows.Count,"Dictionary count");
var mergedSource=File.ReadAllLines(Path.Combine(root,"translations/RO3.LocalizationMerged.tsv")).Select(s=>s.Split('\t',3)).ToDictionary(r=>r[0]);
Check(mergedSource.Count==rows.Count,"Runtime source count matches canonical merged table");
foreach(var row in rows.Values)
 Check(mergedSource.TryGetValue(row[0],out var faithful)&&row[1]==faithful[1]&&row[2]==faithful[2],"Byte-faithful English/Thai from merged source "+row[0]);
var englishOnly=rows.Values.Where(r=>r[1]==r[2]).Select(r=>r[1]).ToHashSet();
foreach(var row in rows.Values)
{
 Check(dict.TryId(row[0],row[1],out var thai)&&thai==row[2],"Fresh ID "+row[0]);
 if(row[1]!=row[2]) Check(dict.Translate(row[1])==(row[1].Length>=45&&!englishOnly.Contains(row[1])?row[2]:row[1]),"Scoped Thai exact English "+row[0]);
 Check(!dict.TryId(row[0],"Different new game English text",out _),"Mismatched ID refused "+row[0]);
}
string Unwire(string s){var b=new System.Text.StringBuilder();for(int i=0;i<s.Length;i++){if(s[i]=='\\'&&i+1<s.Length&&"nrt\\".Contains(s[i+1])){char c=s[++i];b.Append(c=='n'?'\n':c=='r'?'\r':c=='t'?'\t':'\\');}else b.Append(s[i]);}return b.ToString();}
Check(Unwire(@"a\r\nb\tc\\n")=="a\r\nb\tc\\n","Wire controls decoded once; literal backslash-n retained");
var originals=File.ReadAllLines(Path.Combine(config,"RO3.LanguageOrigins.tsv")).Select(s=>s.Split('\t',3)).ToDictionary(r=>r[0]);
foreach(var pair in originals)
 foreach(string chinese in pair.Value.Skip(1))
 {
  if(string.IsNullOrEmpty(chinese)||chinese=="None")continue;
  Check(dict.TryId(pair.Key,chinese,out var restored)&&restored==(Unwire(chinese)==chinese?Unwire(rows[pair.Key][2]):rows[pair.Key][2]),"Verified Chinese by ID "+pair.Key);
 }
var ruleLines=File.ReadAllLines(Path.Combine(config,"RO3.SkillRules.tsv"));
var ruleIds=ruleLines.Select(s=>s.Split('\t',3)[0]).ToHashSet();
Check(!ruleIds.Contains("10960000329"),"Adjacent decimal-capable placeholders retain ID translation only");
foreach(var line in ruleLines)
{
 var key=line.Split('\t',3)[0];var english=Unwire(rows[key][1]);
 var values=Regex.Matches(english,@"[$@]\{\d+\}").Cast<Match>().ToArray();
 for(int i=0;i+1<values.Length;i++)
 {
  string boundary=Regex.Replace(english.Substring(values[i].Index+values[i].Length,values[i+1].Index-values[i].Index-values[i].Length),@"\^\{\d+\}","");
  Check(boundary!=""&&boundary!="."&&boundary!=",","Numeric capture boundary unambiguous "+key);
 }
}
foreach(var line in ruleLines)
{
 var rule=line.Split('\t',3);var row=rows[rule[0]];
 foreach(bool styled in new[]{false,true})
 {
  string Render(string s) => token.Replace(Unwire(s),m=>m.Value.StartsWith('^')?(styled?(int.Parse(Regex.Match(m.Value,@"\d+").Value)%2==1?"<color=#ff9900>":"</color>"):""):"123.5");
  string english=Render(row[1]),expected=Render(row[2]);
  string actual=dict.Translate(english);
  Check(actual==expected,"Rendered skill rule "+rule[0]+" styled="+styled+"\nEXPECTED:"+expected+"\nACTUAL:"+actual);
 }
}
foreach(string label in new[]{"None","Stun","Burn","Slow","Blind","Endure","Lope","Mining","Logging","Collect"}) Check(dict.Translate(label)==label,"Short UI labels not replaced globally: "+label);
Check(dict.Translate("Orc Hero")=="Orc Hero","Monster names unchanged");
Check(dict.Translate("Player123")=="Player123","Player names unchanged");
Check(dict.Translate("PATK")=="PATK","No global word substitutions");
Check(dict.Translate("Hello from my party!")=="Hello from my party!","Unmatched chat unchanged");
Check(dict.Translate("ข้อความภาษาไทยเดิม")=="ข้อความภาษาไทยเดิม","Thai text not retranslated");
Check(!dict.TryId("99999999999","Hello",out _),"Unknown ID refused");
Check(!dict.TryId("10110300015","unknown",out _),"Ambiguous skill does not override mismatched text");
foreach(var sample in new[]{("1002","确定","ยืนยัน"),("1003","取消","ยกเลิก"),("1008","背包","กระเป๋า")})
 Check(dict.TryId(sample.Item1,sample.Item2,out var restored)&&restored==sample.Item3,"Approved UI Thai by verified Chinese ID "+sample.Item1);
Check(rows["27003"][1]=="Log" && rows["27003"][2]=="Log","Held ambiguous UI label stays English");
Check(dict.TryId("27003", originals["27003"][1], out var englishFallback) && englishFallback=="Log","Untranslated UI Chinese source restores English by ID");
Check(dict.Translate("Confirm")=="Confirm" && dict.Translate("Backpack")=="Backpack","Short UI translations do not leak globally");
Check(dict.Translate("确定")=="确定" && dict.Translate("你好")=="你好","No global arbitrary Chinese translations");
Check(!dict.TryId("1002","另一个玩家",out _),"Wrong Chinese for same ID preserved");
var native=new Dictionary<long,string>{{1002,"确定"},{1003,"取消"},{999999,"user-owned"}};
dict.SeedCache(native);Check(native[1002]=="ยืนยัน" && native[1003]=="ยกเลิก","Native Chinese cache restored to approved Thai");
for(int i=0;i<5;i++){native.Clear();native[1002]="确定";dict.SeedCache(native);Check(native[1002]=="ยืนยัน","Repeated language cache reset "+i);}
native[1002]="Changed newer English";dict.SeedCache(native);Check(native[1002]=="Changed newer English","Unknown/new English not forcibly overwritten");
var collision=SkillDictionary.LoadText("1002\tThis is a sufficiently long duplicated original English description.\tThis is a sufficiently long duplicated original English description.\n10110300001\tThis is a sufficiently long duplicated original English description.\tนี่เป็นคำแปลทดสอบที่มีต้นฉบับเดียวกัน\n","");
Check(collision.Translate("This is a sufficiently long duplicated original English description.")=="This is a sufficiently long duplicated original English description.","Shared text cannot change English-only UI IDs");
Check(collision.TryId("10110300001","This is a sufficiently long duplicated original English description.",out var special)&&special=="นี่เป็นคำแปลทดสอบที่มีต้นฉบับเดียวกัน","Shared text still translates by exact approved ID");
checks+=LanguageBridgeChecks.Run();
Console.WriteLine($"PASS: {checks} checks; {dict.Count} merged localization IDs, {dict.RuleCount} runtime rules. No game executed.");
