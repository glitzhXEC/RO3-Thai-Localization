using RO3.Installer;
string temporary = Path.Combine(Path.GetTempPath(), "ro3-install-tests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(temporary);
int passed = 0;
void Check(bool value, string name) { if (!value) throw new Exception(name); passed++; Console.WriteLine("PASS " + name); }
void Reject(Action action, string name) { try { action(); } catch { passed++; Console.WriteLine("PASS " + name); return; } throw new Exception("Expected rejection: " + name); }
try
{
 string client = Path.Combine(temporary, "Client ไทย & spaces"); Directory.CreateDirectory(client); File.WriteAllText(Path.Combine(client,"ro3.exe"),"dummy; never execute");
 Check(GameSelection.ValidateDirectory(client)==client, "explicit folder");
 Check(GameSelection.FromExe(Path.Combine(client,"ro3.exe"))==client,"explicit exe");
 Reject(()=>GameSelection.ValidateDirectory(temporary),"parent not searched");
 Reject(()=>GameSelection.FromExe(Path.Combine(client,"other.exe")),"wrong exe rejected");
 Reject(()=>InstallEngine.ResolveSafe(client,"../escape"),"traversal rejected");
 Reject(()=>InstallEngine.ResolveSafe(client,"ro3.exe"),"game exe cannot be overwritten");
 string payload=Path.Combine(temporary,"payload");Directory.CreateDirectory(payload);File.WriteAllText(Path.Combine(payload,"winhttp.dll"),"dummy payload");
 var entry=new PayloadFile("winhttp.dll",InstallEngine.Sha(Path.Combine(payload,"winhttp.dll")));
 var manifest=new PayloadManifest("test-only",true,[entry]);
 Reject(()=>InstallEngine.Install(client,payload,new PayloadManifest("dev",false,[entry])),"unready payload rejected");
 Check(!File.Exists(Path.Combine(client,"winhttp.dll")),"unready payload writes nothing");
 Reject(()=>InstallEngine.Install(client,payload,new PayloadManifest("bad",true,[entry with { Sha256="bad" }])),"hash mismatch rejected");
 Check(!File.Exists(Path.Combine(client,"winhttp.dll")),"bad hash writes nothing");
 // A later destination collision must restore an earlier overwritten original.
 File.WriteAllText(Path.Combine(client,"winhttp.dll"),"original");Directory.CreateDirectory(Path.Combine(client,"doorstop_config.ini"));File.WriteAllText(Path.Combine(payload,"doorstop_config.ini"),"dummy config");
 var collision=new PayloadFile("doorstop_config.ini",InstallEngine.Sha(Path.Combine(payload,"doorstop_config.ini")));
 Reject(()=>InstallEngine.Install(client,payload,new PayloadManifest("test",true,[entry,collision])),"late failure rolls back");
 Check(File.ReadAllText(Path.Combine(client,"winhttp.dll"))=="original","original file restored");
 Check(!File.Exists(Path.Combine(client,".ro3-thai-localization.json")),"failure has no success marker");
 Directory.Delete(Path.Combine(client,"doorstop_config.ini"));
 InstallEngine.Install(client,payload,manifest);
 Check(File.ReadAllText(Path.Combine(client,"winhttp.dll"))=="dummy payload","valid copy");
 Check(File.Exists(Path.Combine(client,".ro3-thai-localization.json")),"success ownership record");
 Reject(()=>InstallEngine.Install(client,payload,manifest),"duplicate install blocked in development");
 string modded=Path.Combine(temporary,"modded");Directory.CreateDirectory(Path.Combine(modded,"BepInEx"));File.WriteAllText(Path.Combine(modded,"ro3.exe"),"dummy");
 Reject(()=>InstallEngine.Install(modded,payload,manifest),"existing mods protected");
 string linked=Path.Combine(temporary,"linked");Directory.CreateSymbolicLink(linked,client);
 Reject(()=>InstallEngine.Install(linked,payload,manifest),"symlink target rejected");
 Console.WriteLine($"{passed} tests passed; dummy files only, no game or Windows UI executed.");
}
finally { Directory.Delete(temporary,true); }
