"""Build a complete, installable-for-testing skills payload, never mark it game-tested."""
import subprocess,sys,os,json,hashlib,zipfile,shutil
from pathlib import Path
root=Path(__file__).resolve().parents[1]
dotnet=os.environ.get('DOTNET_COMMAND','dotnet')
def run(args):subprocess.run(args,cwd=root,check=True)
# Use the checked-in main TSV as the canonical feed input; do not overwrite direct edits with batches.
run([sys.executable,'scripts/prepare_compatibility_runtime.py'])
run([sys.executable,'scripts/prepare_live_translations.py','--seed-config','runtime-build/payload/BepInEx/config'])
# Prevent immutable XUnity mappings from winning over an updated plugin dictionary.
(root/'runtime-build/payload/BepInEx/Translation/th/Text/RO3_Skills_Canonical.txt').write_text('',encoding='utf-8')
stage=root/'runtime-build/payload'
shutil.copyfile(root/'packaging/AutoTranslatorConfig.ini',stage/'BepInEx/config/AutoTranslatorConfig.ini')
run([dotnet,'build','src/SkillRuntime/SkillRuntime.csproj','-c','Release'])
run([dotnet,'run','--project','tests/SkillRuntime.Tests','-c','Release','--',str(root)])
run([dotnet,'run','--project','tests/TranslationUpdater.Tests','-c','Release','--',str(root)])
plugin=stage/'BepInEx/plugins';plugin.mkdir(parents=True,exist_ok=True)
for source in [root/'src/SkillRuntime/bin/Release/net472/RO3.ThaiLocalization.Skills.dll',root/'src/SkillRuntime.Engine/bin/Release/net472/SkillRuntime.Engine.dll']:
 shutil.copyfile(source,plugin/source.name)
# Do not reintroduce attributes/API calls that the stripped game runtime cannot resolve.
engine=(plugin/'SkillRuntime.Engine.dll').read_bytes()
assert all(token not in engine for token in (b'System.Runtime.Serialization',b'DataContractAttribute',b'DataContractJsonSerializer',b'set_ReadWriteTimeout')), 'Stripped-runtime incompatibility in plugin metadata'
files=[]
for file in sorted(stage.rglob('*')):
 if not file.is_file():continue
 relative=file.relative_to(stage).as_posix()
 assert not relative.lower().endswith('.bat') and '/Translation/ja/' not in relative
 assert 'RO3.LocalizationTablePatcher' not in relative and 'LocalizationAliases' not in relative
 files.append({'Path':relative,'Sha256':hashlib.sha256(file.read_bytes()).hexdigest()})
required=['BepInEx/config/RO3.TranslationCache/cache.json','winhttp.dll','doorstop_config.ini','arialuni_sdf_u2022','BepInEx/core/BepInEx.dll','BepInEx/core/BepInEx.Preloader.dll','BepInEx/core/0Harmony.dll','BepInEx/plugins/RO3.ThaiLocalization.Skills.dll','BepInEx/plugins/SkillRuntime.Engine.dll','BepInEx/config/RO3.SkillTranslations.tsv','BepInEx/config/RO3.SkillRules.tsv','BepInEx/config/AutoTranslatorConfig.ini']
assert set(required).issubset({f['Path'] for f in files})
manifest={'Version':'0.3.1-ui-hotfix-alpha.1','ReadyForInstallation':True,'TargetProfile':'ro3-mono-x64','Files':files}
installer=root/'src/Installer'
(installer/'payload-manifest.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
with zipfile.ZipFile(installer/'payload.zip','w',zipfile.ZIP_DEFLATED) as z:
 for file in files:z.write(stage/file['Path'],file['Path'])
(root/'docs/partial-install-build.json').write_text(json.dumps({'version':manifest['Version'],'payload_files':len(files),'payload_zip_sha256':hashlib.sha256((installer/'payload.zip').read_bytes()).hexdigest(),'runtime_source':'pinned compatibility runtime; fresh new plugin/dictionaries','installable_for_testing':True,'existing_BepInEx_supported':False,'windows_ui_tested':False,'game_tested':False},indent=2)+'\n')
print('Auto-update payload packaged:',len(files),'files. Game compatibility/display still require in-game testing.')
