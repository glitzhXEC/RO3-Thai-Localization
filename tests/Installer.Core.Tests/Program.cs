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
 var manifest=new PayloadManifest("1.0.0-alpha.1",true,[entry]);
 Reject(()=>InstallEngine.Install(client,payload,new PayloadManifest("dev",false,[entry])),"unready payload rejected");
 Check(!File.Exists(Path.Combine(client,"winhttp.dll")),"unready payload writes nothing");
 Reject(()=>InstallEngine.Install(client,payload,new PayloadManifest("bad",true,[entry with { Sha256="bad" }])),"hash mismatch rejected");
 Check(!File.Exists(Path.Combine(client,"winhttp.dll")),"bad hash writes nothing");
 // Unowned files are never overwritten and installation creates no persistent backup.
 File.WriteAllText(Path.Combine(client,"winhttp.dll"),"original");Directory.CreateDirectory(Path.Combine(client,"doorstop_config.ini"));File.WriteAllText(Path.Combine(payload,"doorstop_config.ini"),"dummy config");
 var collision=new PayloadFile("doorstop_config.ini",InstallEngine.Sha(Path.Combine(payload,"doorstop_config.ini")));
 Reject(()=>InstallEngine.Install(client,payload,new PayloadManifest("1.0.0-alpha.1",true,[entry,collision])),"unowned destination collision rejected");
 Check(File.ReadAllText(Path.Combine(client,"winhttp.dll"))=="original","collision leaves existing file unchanged");
 Check(!File.Exists(Path.Combine(client,".ro3-thai-localization.json")),"failed install has no success marker");
 File.Delete(Path.Combine(client,"winhttp.dll"));
 Directory.Delete(Path.Combine(client,"doorstop_config.ini"));
 InstallEngine.Install(client,payload,manifest);
 Check(File.ReadAllText(Path.Combine(client,"winhttp.dll"))=="dummy payload","valid copy");
 Check(File.Exists(Path.Combine(client,".ro3-thai-localization.json")),"success ownership record");
 Check(InstallEngine.ReadInstallation(client)?.Version==manifest.Version,"installed version is detected from ownership record");
 Check(InstallEngine.CompareVersions("1.0.0-alpha.1","1.1.0-installer-alpha.1")<0,"older installed version detected");
 Check(InstallEngine.CompareVersions("1.1.0-installer-alpha.1","1.1.0-installer-alpha.2")<0,"newer installer prerelease detected");
 File.WriteAllText(Path.Combine(payload,"winhttp.dll"),"updated payload");
 var updateEntry=new PayloadFile("winhttp.dll",InstallEngine.Sha(Path.Combine(payload,"winhttp.dll")));
 var updateManifest=new PayloadManifest("1.1.0-installer-alpha.1",true,[updateEntry]);
 InstallEngine.Install(client,payload,updateManifest);
 Check(File.ReadAllText(Path.Combine(client,"winhttp.dll"))=="updated payload","managed installation updates in place");
 Check(InstallEngine.ReadInstallation(client)?.Version==updateManifest.Version,"ownership version advances after update");
 Check(!Directory.EnumerateDirectories(client).Any(d=>Path.GetFileName(d).StartsWith(".ro3-thai-backup-",StringComparison.Ordinal)),"update leaves no backup directory");
 string modded=Path.Combine(temporary,"modded");Directory.CreateDirectory(Path.Combine(modded,"BepInEx"));File.WriteAllText(Path.Combine(modded,"ro3.exe"),"dummy");
 Reject(()=>InstallEngine.Install(modded,payload,manifest),"existing mods protected");
 string linked=Path.Combine(temporary,"linked");
 try { Directory.CreateSymbolicLink(linked,client);Reject(()=>InstallEngine.Install(linked,payload,updateManifest),"symlink target rejected"); }
 catch (Exception e) when (e is IOException or UnauthorizedAccessException or PlatformNotSupportedException) { Console.WriteLine("SKIP symlink test; creating symlinks is unavailable"); }

 InstallEngine.Uninstall(client);
 Check(!File.Exists(Path.Combine(client,"winhttp.dll")),"uninstall removes owned files without a backup");
 Check(!File.Exists(Path.Combine(client,".ro3-thai-localization.json")),"uninstall removes ownership marker");
 string protect=Path.Combine(temporary,"protect");Directory.CreateDirectory(protect);File.WriteAllText(Path.Combine(protect,"ro3.exe"),"dummy");
 InstallEngine.Install(protect,payload,updateManifest);File.WriteAllText(Path.Combine(protect,"winhttp.dll"),"user-modified DLL");
 Reject(()=>InstallEngine.Uninstall(protect),"changed DLL not deleted by uninstall");
 Check(File.ReadAllText(Path.Combine(protect,"winhttp.dll"))=="user-modified DLL","changed DLL preserved");
 string hotfix=Path.Combine(temporary,"plugin hotfix");Directory.CreateDirectory(hotfix);File.WriteAllText(Path.Combine(hotfix,"ro3.exe"),"dummy");
 string pluginPath="BepInEx/plugins/RO3.ThaiLocalization.Skills.dll";string pluginSource=InstallEngine.ResolveSafe(payload,pluginPath);Directory.CreateDirectory(Path.GetDirectoryName(pluginSource)!);File.WriteAllText(pluginSource,"official plugin");
 var pluginEntry=new PayloadFile(pluginPath,InstallEngine.Sha(pluginSource));
 InstallEngine.Install(hotfix,payload,new PayloadManifest("1.0.0-alpha.1",true,[pluginEntry]));File.WriteAllText(Path.Combine(hotfix,pluginPath),"official plugin hotfix");
 InstallEngine.Uninstall(hotfix);Check(!File.Exists(Path.Combine(hotfix,pluginPath)),"owned plugin hotfix can be removed by explicit uninstall");
 string legacy=Path.Combine(temporary,"legacy install");Directory.CreateDirectory(legacy);File.WriteAllText(Path.Combine(legacy,"ro3.exe"),"game");
 File.WriteAllText(Path.Combine(legacy,"winhttp.dll"),"patch loader");string oldBackup=Path.Combine(legacy,".ro3-thai-backup-legacy");Directory.CreateDirectory(oldBackup);File.WriteAllText(Path.Combine(oldBackup,"0"),"original proxy");
 var legacyEntry=new PayloadFile("winhttp.dll",InstallEngine.Sha(Path.Combine(legacy,"winhttp.dll")));
 File.WriteAllText(Path.Combine(legacy,".ro3-thai-localization.json"),System.Text.Json.JsonSerializer.Serialize(new InstallationRecord("1.0.0-alpha.1",[legacyEntry],oldBackup)));
 Check(InstallEngine.ReadInstallation(legacy)?.Version=="1.0.0-alpha.1","legacy installed version remains readable");
 InstallEngine.Install(legacy,payload,updateManifest);
 Check(File.ReadAllText(Path.Combine(legacy,"winhttp.dll"))=="updated payload","legacy install updates without replacing its original-file record");
 InstallEngine.Uninstall(legacy);Check(File.ReadAllText(Path.Combine(legacy,"winhttp.dll"))=="original proxy","legacy uninstall restores original file");
 Check(!Directory.Exists(oldBackup),"legacy backup is removed after restoring original");
 string mono=Path.Combine(temporary,"mono");Directory.CreateDirectory(Path.Combine(mono,"ro3_Data/Managed"));
 byte[] pe=new byte[256];pe[0]=0x4d;pe[1]=0x5a;BitConverter.GetBytes(128).CopyTo(pe,0x3c);pe[128]=0x50;pe[129]=0x45;pe[132]=0x64;pe[133]=0x86;File.WriteAllBytes(Path.Combine(mono,"ro3.exe"),pe);File.WriteAllText(Path.Combine(mono,"ro3_Data/Managed/UnityEngine.CoreModule.dll"),"dummy");
 InstallEngine.ValidateMonoX64(mono);Check(true,"selected x64 Mono profile");
 pe[132]=0x4c;pe[133]=1;File.WriteAllBytes(Path.Combine(mono,"ro3.exe"),pe);Reject(()=>InstallEngine.ValidateMonoX64(mono),"x86 rejected");
 pe[132]=0x64;pe[133]=0x86;File.WriteAllBytes(Path.Combine(mono,"ro3.exe"),pe);File.Delete(Path.Combine(mono,"ro3_Data/Managed/UnityEngine.CoreModule.dll"));Reject(()=>InstallEngine.ValidateMonoX64(mono),"non-Mono client rejected without searching");
 Reject(()=>InstallEngine.ValidatePayload(payload,new PayloadManifest("skills",true,[entry],"ro3-mono-x64")),"incomplete skill runtime rejected");

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
  InstallEngine.Uninstall(whole);
  Check(!full.Files.Any(f=>File.Exists(InstallEngine.ResolveSafe(whole,f.Path))),"only full owned payload removed");
  Check(File.ReadAllText(userMod)=="user-added mod; not executed","later user mod retained");
  Check(!Directory.EnumerateDirectories(whole).Any(d=>Path.GetFileName(d).StartsWith(".ro3-thai-uninstall-snapshot-",StringComparison.Ordinal)),"uninstall creates no persistent snapshot");
  Check(InstallEngine.Sha(Path.Combine(whole,"ro3.exe"))==gameBefore,"game executable unchanged after uninstall");
 }
 Console.WriteLine($"{passed} tests passed; dummy files only, no game or Windows UI executed.");
}
finally { Directory.Delete(temporary,true); }
