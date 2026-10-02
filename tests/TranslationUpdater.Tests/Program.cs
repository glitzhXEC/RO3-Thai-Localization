using RO3.ThaiLocalization;
using System.Text;
using System.Text.Json;
var root=Path.GetFullPath(args.Length>0?args[0]:".");
var temp=Path.Combine(Path.GetTempPath(),"ro3-update-tests-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(temp);
int passed=0;void Check(bool ok,string label){if(!ok)throw new Exception(label);passed++;Console.WriteLine("PASS "+label);}
void Reject(Action f,string label){try{f();}catch{Check(true,label);return;}throw new Exception("Expected rejection: "+label);}
var options=new JsonSerializerOptions{IncludeFields=true};
byte[] Json(TranslationManifest m)=>JsonSerializer.SerializeToUtf8Bytes(m,options);
TranslationManifest Clone(TranslationManifest m)=>JsonSerializer.Deserialize<TranslationManifest>(Json(m),options)!;
try{
 var engine=typeof(TranslationManifest).Assembly;
 Check(!engine.GetReferencedAssemblies().Any(a=>a.Name=="System.Runtime.Serialization"),"engine has no stripped serialization dependency");
 Check(engine.GetTypes().All(t=>!t.GetCustomAttributesData().Any(a=>a.AttributeType.Namespace=="System.Runtime.Serialization")),"game type scans do not see unsupported serializer attributes");
 var m=JsonSerializer.Deserialize<TranslationManifest>(File.ReadAllBytes(Path.Combine(root,"translations/live/manifest.json")),options)!;
 Check(TranslationUpdater.GetLatestVersion((_,_)=>Json(m))==m.Version,"latest translation feed version checked without downloading data");
 var dir=Path.Combine(root,"translations/live/versions",m.Version);
 byte[] table=File.ReadAllBytes(Path.Combine(dir,m.Files[0].Name)),rules=File.ReadAllBytes(Path.Combine(dir,m.Files[1].Name)),origins=File.ReadAllBytes(Path.Combine(dir,m.Files[2].Name));
 var seedConfig=Path.Combine(root,"runtime-build/payload/BepInEx/config");
 if(File.Exists(Path.Combine(seedConfig,TranslationUpdater.CacheRelativePath))){var seeded=TranslationUpdater.LoadCache(seedConfig,out var seedVersion);Check(seeded?.Count==m.TranslationIds && seedVersion==m.Version,"Python-generated installer seed loads in C#");}
 var dictionary=TranslationUpdater.ValidateBundle(m,table,rules,origins);Check(dictionary.Count==m.TranslationIds && dictionary.RuleCount==m.RuntimeRules,"real feed counts and all source invariants");
 int requests=0; byte[] Fetch(string path,int limit){requests++;byte[] b=path==TranslationUpdater.ManifestPath?Json(m):path==TranslationUpdater.FilePath(m,0)?table:path==TranslationUpdater.FilePath(m,1)?rules:path==TranslationUpdater.FilePath(m,2)?origins:throw new Exception("Unknown path");Check(b.Length<=limit,"bounded approved request");return b;}
 var config=Path.Combine(temp,"config");
 var updated=TranslationUpdater.Refresh(config,"",_=>{},Fetch);Check(updated?.Count==m.TranslationIds && requests==4,"validated online transaction");
 var loaded=TranslationUpdater.LoadCache(config,out var version);Check(version==m.Version && loaded?.Count==m.TranslationIds,"cache reload offline");
 var cache=Path.Combine(config,TranslationUpdater.CacheRelativePath);byte[] before=File.ReadAllBytes(cache);requests=0;
 Check(TranslationUpdater.Refresh(config,version,_=>{},Fetch)==null && requests==1,"unchanged manifest skips downloads");
 Check(before.SequenceEqual(File.ReadAllBytes(cache)),"unchanged cache untouched");
 Check(TranslationUpdater.Refresh(config,"",_=>{},(_,_)=>throw new IOException("offline"))==null,"offline retains dictionary");
 Check(before.SequenceEqual(File.ReadAllBytes(cache)),"offline cache unchanged");
 Check(TranslationUpdater.Refresh(config,"",_=>{},(_,_)=>Encoding.UTF8.GetBytes("not json"))==null,"malformed manifest rejected");
 foreach(var malformed in new[]{"{\"Schema\":1,\"Schema\":1}","[]","{\"Schema\":01}","{}garbage","{\"Schema\":1.0}","{\"Version\":\"\\ud800\"}"}) Check(TranslationUpdater.Refresh(config,"",_=>{},(_,_)=>Encoding.UTF8.GetBytes(malformed))==null,"strict JSON rejects duplicate/type/trailing/unicode faults");
 foreach(var kind in new[]{"schema","name","hash","count","size","version","source"}){
  var bad=Clone(m);switch(kind){case "schema":bad.RuntimeSchema=99;break;case "name":bad.Files[0].Name="../../plugin.dll";break;case "hash":bad.Files[0].Sha256=new string('0',64);break;case "count":bad.TranslationIds++;break;case "size":bad.Files[0].Bytes=9*1024*1024;break;case "version":bad.Version=new string('0',64);break;case "source":bad.SourceSha256="invalid";break;}
  Check(TranslationUpdater.Refresh(config,"",_=>{},(p,l)=>p==TranslationUpdater.ManifestPath?Json(bad):Fetch(p,l))==null,"reject "+kind);
  Check(before.SequenceEqual(File.ReadAllBytes(cache)),"preserve cache after "+kind);
 }
 Check(TranslationUpdater.Refresh(config,"",_=>{},(p,l)=>p==TranslationUpdater.ManifestPath?Json(m):p==TranslationUpdater.FilePath(m,0)?table[..^1]:p==TranslationUpdater.FilePath(m,1)?rules:origins)==null,"truncated download rejected");
 Check(before.SequenceEqual(File.ReadAllBytes(cache)),"truncation preserves cache");
 TranslationUpdater.SaveCache(config,m,table,rules,origins);Check(!Directory.GetFiles(Path.GetDirectoryName(cache)!,"*.tmp").Any(),"atomic replacement cleans temporary files");
 Reject(()=>TranslationUpdater.FetchTrusted("main/evil.exe",20),"executable download path rejected before network");
 Reject(()=>TranslationUpdater.FetchTrusted("../anything",20),"traversal download rejected before network");
 foreach(var pair in new[]{("Damage {1} at 30%","สร้างความเสียหาย {2} ที่ 30%"),("MATK*{1}%+{2}","MATK{1}%+{2}"),("Apply 【Frozen】","ติด [ Burning ]"),("Damage 30%","เสียหาย 31%"),("Damage now","แปล\uFFFD")})
  Reject(()=>SkillDictionary.LoadText("10110300001\t"+pair.Item1+"\t"+pair.Item2+"\n",""),"invalid pair rejected: "+pair.Item1);
 Reject(()=>SkillDictionary.LoadText("10110300001\tDamage now\tสร้างความเสียหาย\n","10110300001\t.*\tทุกข้อความ\n"),"unanchored rule rejected");
 var item=SkillDictionary.LoadText("12390100001\tItem gives 5 points\tไอเทมมอบ 5 แต้ม\n10050100001\tFood gives 3 points\tอาหารมอบ 3 แต้ม\n","");Check(item.Count==2,"item and food IDs supported");Check(!item.TryId("12390100001","Different English",out _),"reused ID with changed English remains untranslated");
 Check(m.RuntimeSchema==2 && m.Files.Length==3,"runtime schema 2 requires origins file and all merged IDs");
 File.WriteAllText(cache,"corrupt");Reject(()=>TranslationUpdater.LoadCache(config,out _),"corrupt cache rejected for embedded fallback");
 File.WriteAllBytes(cache,before);
 try
 {
  var linked=Path.Combine(temp,"linked");Directory.CreateSymbolicLink(linked,config);Reject(()=>TranslationUpdater.SaveCache(linked,m,table,rules,origins),"linked config rejected");
  var config2=Path.Combine(temp,"config2");Directory.CreateDirectory(config2);Directory.CreateSymbolicLink(Path.Combine(config2,"RO3.TranslationCache"),Path.GetDirectoryName(cache)!);Reject(()=>TranslationUpdater.SaveCache(config2,m,table,rules,origins),"linked cache directory rejected");
  Check(before.SequenceEqual(File.ReadAllBytes(cache)),"linked writes do not alter outside data");
 }
 catch(Exception e) when(e is IOException or UnauthorizedAccessException or PlatformNotSupportedException){Console.WriteLine("SKIP symlink tests; creating symlinks is unavailable");}
 Console.WriteLine($"{passed} updater checks passed. Fake network only; no game or executable downloads.");
}finally{Directory.Delete(temp,true);}
