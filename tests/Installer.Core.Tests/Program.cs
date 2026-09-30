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

 string originalBackup = InstallEngine.Uninstall(client);
 Check(File.ReadAllText(Path.Combine(client,"winhttp.dll"))=="original","uninstall restores original proxy");
 Check(!File.Exists(Path.Combine(client,".ro3-thai-localization.json")),"uninstall removes ownership marker");
 Check(Directory.Exists(originalBackup),"uninstall snapshot retained");
 string protect=Path.Combine(temporary,"protect");Directory.CreateDirectory(protect);File.WriteAllText(Path.Combine(protect,"ro3.exe"),"dummy");
 InstallEngine.Install(protect,payload,manifest);File.WriteAllText(Path.Combine(protect,"winhttp.dll"),"user-modified DLL");
 Reject(()=>InstallEngine.Uninstall(protect),"changed DLL not deleted by uninstall");
 Check(File.ReadAllText(Path.Combine(protect,"winhttp.dll"))=="user-modified DLL","changed DLL preserved");
 string mono=Path.Combine(temporary,"mono");Directory.CreateDirectory(Path.Combine(mono,"ro3_Data/Managed"));
 byte[] pe=new byte[256];pe[0]=0x4d;pe[1]=0x5a;BitConverter.GetBytes(128).CopyTo(pe,0x3c);pe[128]=0x50;pe[129]=0x45;pe[132]=0x64;pe[133]=0x86;File.WriteAllBytes(Path.Combine(mono,"ro3.exe"),pe);File.WriteAllText(Path.Combine(mono,"ro3_Data/Managed/UnityEngine.CoreModule.dll"),"dummy");
 InstallEngine.ValidateMonoX64(mono);Check(true,"selected x64 Mono profile");
 pe[132]=0x4c;pe[133]=1;File.WriteAllBytes(Path.Combine(mono,"ro3.exe"),pe);Reject(()=>InstallEngine.ValidateMonoX64(mono),"x86 rejected");
 pe[132]=0x64;pe[133]=0x86;File.WriteAllBytes(Path.Combine(mono,"ro3.exe"),pe);File.Delete(Path.Combine(mono,"ro3_Data/Managed/UnityEngine.CoreModule.dll"));Reject(()=>InstallEngine.ValidateMonoX64(mono),"non-Mono client rejected without searching");
 Reject(()=>InstallEngine.ValidatePayload(payload,new PayloadManifest("skills",true,[entry],"ro3-mono-x64")),"incomplete skill runtime rejected");


 string all=Path.Combine(temporary,"remove all ไทย");Directory.CreateDirectory(Path.Combine(all,"BepInEx/plugins"));Directory.CreateDirectory(Path.Combine(all,"BepInEx/core"));Directory.CreateDirectory(Path.Combine(all,"ro3_Data"));
 File.WriteAllText(Path.Combine(all,"ro3.exe"),"game unchanged");File.WriteAllText(Path.Combine(all,"ro3_Data/game.dat"),"game data unchanged");
 File.WriteAllText(Path.Combine(all,"BepInEx/plugins/OtherMod.dll"),"third party mod snapshot");File.WriteAllText(Path.Combine(all,"BepInEx/core/BepInEx.dll"),"dummy core");File.WriteAllText(Path.Combine(all,"BepInEx/LogOutput.log"),"user log");
 File.WriteAllText(Path.Combine(all,"winhttp.dll"),"loader");File.WriteAllText(Path.Combine(all,"doorstop_config.ini"),"target_assembly=BepInEx/core/BepInEx.Preloader.dll");File.WriteAllText(Path.Combine(all,"version.dll"),"unrelated root DLL");File.WriteAllText(Path.Combine(all,".ro3-thai-localization.json"),"damaged ownership marker");
 Reject(()=>InstallEngine.RemoveBepInExAll(all,new ThrowOnBepFolder()),"full removal rolls back after folder move failure");
 Check(Directory.Exists(Path.Combine(all,"BepInEx"))&&File.ReadAllText(Path.Combine(all,"winhttp.dll"))=="loader","full rollback restores folder and proxy");
 var removed=InstallEngine.RemoveBepInExAll(all);
 Check(!Directory.Exists(Path.Combine(all,"BepInEx"))&&!File.Exists(Path.Combine(all,"winhttp.dll")),"all active BepInEx and confirmed loader removed");
 Check(!File.Exists(Path.Combine(all,".ro3-thai-localization.json")),"damaged old marker moved safely");
 Check(File.ReadAllText(Path.Combine(removed.BackupDirectory,"BepInEx/plugins/OtherMod.dll"))=="third party mod snapshot","full removal backs up third-party mods");
 Check(File.ReadAllText(Path.Combine(removed.BackupDirectory,"BepInEx/LogOutput.log"))=="user log","full removal backs up logs");
 Check(File.ReadAllText(Path.Combine(all,"ro3.exe"))=="game unchanged"&&File.ReadAllText(Path.Combine(all,"ro3_Data/game.dat"))=="game data unchanged","full removal never modifies game files");
 Check(File.ReadAllText(Path.Combine(all,"version.dll"))=="unrelated root DLL","unrelated root DLL retained");
 string unknown=Path.Combine(temporary,"unknown proxy");Directory.CreateDirectory(Path.Combine(unknown,"BepInEx/plugins"));File.WriteAllText(Path.Combine(unknown,"ro3.exe"),"game");File.WriteAllText(Path.Combine(unknown,"winhttp.dll"),"unknown proxy");
 var retained=InstallEngine.RemoveBepInExAll(unknown);Check(File.ReadAllText(Path.Combine(unknown,"winhttp.dll"))=="unknown proxy"&&retained.PreservedRootPaths.Contains("winhttp.dll"),"unproven proxy retained and reported");
 string unsafeRoot=Path.Combine(temporary,"unsafe tree");Directory.CreateDirectory(Path.Combine(unsafeRoot,"BepInEx"));File.WriteAllText(Path.Combine(unsafeRoot,"ro3.exe"),"game");Directory.CreateSymbolicLink(Path.Combine(unsafeRoot,"BepInEx/outside"),all);
 Reject(()=>InstallEngine.RemoveBepInExAll(unsafeRoot),"linked mod tree rejected before removal");Check(Directory.Exists(Path.Combine(unsafeRoot,"BepInEx"))&&File.ReadAllText(Path.Combine(all,"ro3.exe"))=="game unchanged","outside target untouched by removal");
 Reject(()=>InstallEngine.RemoveBepInExAll(temporary),"full uninstall does not search parent folder for Client");
 if (args.Length>0)
 {
  string repo=Path.GetFullPath(args[0]);
  var full=System.Text.Json.JsonSerializer.Deserialize<PayloadManifest>(File.ReadAllText(Path.Combine(repo,"src/Installer/payload-manifest.json")))!;
  string stage=Path.Combine(repo,"runtime-build/payload");
  InstallEngine.ValidatePayload(stage,full);Check(true,"full real payload hash validation");
  string whole=Path.Combine(temporary,"whole payload Thai client");Directory.CreateDirectory(Path.Combine(whole,"ro3_Data/Managed"));
  pe[132]=0x64;pe[133]=0x86;File.WriteAllBytes(Path.Combine(whole,"ro3.exe"),pe);File.WriteAllText(Path.Combine(whole,"ro3_Data/Managed/UnityEngine.CoreModule.dll"),"dummy");
  string gameBefore=InstallEngine.Sha(Path.Combine(whole,"ro3.exe"));
  InstallEngine.Install(whole,stage,full);
  Check(full.Files.All(f=>InstallEngine.Sha(InstallEngine.ResolveSafe(whole,f.Path))==f.Sha256),"all real payload files installed correctly");
  Check(InstallEngine.Sha(Path.Combine(whole,"ro3.exe"))==gameBefore,"game executable unchanged");
  string userMod=Path.Combine(whole,"BepInEx/plugins/UserExtra.dll");File.WriteAllText(userMod,"user-added mod; not executed");
  File.AppendAllText(Path.Combine(whole,"BepInEx/config/AutoTranslatorConfig.ini"),"\n# user setting\n");
  string cacheFile=Path.Combine(whole,"BepInEx/config/RO3.TranslationCache/cache.json");
  File.WriteAllText(cacheFile,"updated translation cache; data only");
  string saved=InstallEngine.Uninstall(whole);
  Check(!full.Files.Any(f=>File.Exists(InstallEngine.ResolveSafe(whole,f.Path))),"only full owned payload removed");
  Check(File.ReadAllText(userMod)=="user-added mod; not executed","later user mod retained");
  Check(Directory.Exists(saved),"mutable settings retained in uninstall snapshot");
  Check(File.ReadAllText(Path.Combine(saved,Array.FindIndex(full.Files,f=>f.Path=="BepInEx/config/RO3.TranslationCache/cache.json").ToString()))=="updated translation cache; data only","updated owned cache preserved in uninstall snapshot");
  Check(InstallEngine.Sha(Path.Combine(whole,"ro3.exe"))==gameBefore,"game executable unchanged after uninstall");
  var fullClean=InstallEngine.RemoveBepInExAll(whole);Check(!Directory.Exists(Path.Combine(whole,"BepInEx")),"full menu removes leftovers preserved by scoped uninstall");
  Check(File.ReadAllText(Path.Combine(fullClean.BackupDirectory,"BepInEx/plugins/UserExtra.dll"))=="user-added mod; not executed","leftover mod moved to full backup");
  InstallEngine.Install(whole,stage,full);Check(File.Exists(Path.Combine(whole,".ro3-thai-localization.json")),"clean reinstall succeeds after full removal");
  InstallEngine.Uninstall(whole);
 }
 Console.WriteLine($"{passed} tests passed; dummy files only, no game or Windows UI executed.");
}
finally { Directory.Delete(temporary,true); }

sealed class ThrowOnBepFolder : IProgress<InstallProgress>
{
 public void Report(InstallProgress p){if(p.Path=="BepInEx")throw new IOException("Simulated failure after BepInEx move");}
}
