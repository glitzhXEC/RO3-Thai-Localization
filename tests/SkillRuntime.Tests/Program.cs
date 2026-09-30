using RO3.ThaiLocalization;
using System.Text.RegularExpressions;
string root=Path.GetFullPath(args.Length>0?args[0]:".");
string config=Path.Combine(root,"runtime-build/payload/BepInEx/config");
var dict=SkillDictionary.Load(Path.Combine(config,"RO3.SkillTranslations.tsv"),Path.Combine(config,"RO3.SkillRules.tsv"));
var token=new Regex(@"[$@^]\{\d+\}");
int checks=0;
void Check(bool yes,string context) { if(!yes)throw new Exception(context);checks++; }
var rows=File.ReadAllLines(Path.Combine(config,"RO3.SkillTranslations.tsv")).Select(s=>s.Split('\t',3)).ToDictionary(r=>r[0]);
Check(dict.Count==rows.Count,"Dictionary count");
foreach(var row in rows.Values)
{
 Check(dict.TryId(row[0],row[1],out var thai)&&thai==row[2],"Fresh ID "+row[0]);
 Check(dict.Translate(row[1])==(row[1].Length>=45?row[2]:row[1]),"Scoped exact English "+row[0]);
 Check(!dict.TryId(row[0],"Different new game English text",out _),"Mismatched ID refused "+row[0]);
}
foreach(var line in File.ReadAllLines(Path.Combine(config,"RO3.SkillRules.tsv")))
{
 var rule=line.Split('\t',3);var row=rows[rule[0]];
 foreach(bool styled in new[]{false,true})
 {
  string Render(string s) => token.Replace(s.Replace(@"\n","\n"),m=>m.Value.StartsWith('^')?(styled?(int.Parse(Regex.Match(m.Value,@"\d+").Value)%2==1?"<color=#ff9900>":"</color>"):""):"123.5");
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
Check(!dict.TryId("10110300015","unknown",out _),"Ambiguous skill excluded");
Console.WriteLine($"PASS: {checks} checks; {dict.Count} fresh skill IDs, {dict.RuleCount} runtime rules. No game executed.");
