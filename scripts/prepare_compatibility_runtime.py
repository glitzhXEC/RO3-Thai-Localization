"""Extract ONLY runtime DLLs/config/font from a pinned, hashed compatibility bundle.
Never read legacy translations, aliases, BAT files, or the old custom plugin.
"""
import hashlib,json,urllib.request,zipfile,shutil
from pathlib import Path,PurePosixPath
root=Path(__file__).resolve().parents[1]
work=root/'runtime-build';cache=work/'downloads';cache.mkdir(parents=True,exist_ok=True)
name='RO3_Asia_Thai_Patch_2026.09.30.17.zip'
url='https://github.com/glitzhXEC/RO3_Asia_Thai_Patch/releases/download/v2026.09.30.17/'+name
expected='994a11a08985676c806d5679c0ab95bee75a4f6d5ad82d7dc45a6ff82b872f01'
archive=cache/name
if not archive.exists():urllib.request.urlretrieve(url,archive)
assert hashlib.sha256(archive.read_bytes()).hexdigest()==expected,'Compatibility runtime hash mismatch'
stage=work/'payload'
if stage.exists():shutil.rmtree(stage)
stage.mkdir(parents=True)
selected=[]
with zipfile.ZipFile(archive) as z:
 for entry in z.infolist():
  if entry.is_dir() or '/payload/' not in entry.filename:continue
  relative=entry.filename.split('/payload/',1)[1]
  allowed=(relative.startswith('BepInEx/core/') and relative.endswith('.dll')) or ((relative.startswith('BepInEx/plugins/XUnity.AutoTranslator/') or relative.startswith('BepInEx/plugins/XUnity.ResourceRedirector/')) and relative.endswith('.dll')) or relative in ['winhttp.dll','.doorstop_version','doorstop_config.ini','arialuni_sdf_u2022','BepInEx/config/BepInEx.cfg','BepInEx/config/gravydevsupreme.xunity.resourceredirector.cfg']
  if not allowed:continue
  assert '..' not in PurePosixPath(relative).parts and not relative.startswith('/')
  target=stage/relative;target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(z.read(entry))
  selected.append({'path':relative,'sha256':hashlib.sha256(target.read_bytes()).hexdigest()})
assert hashlib.sha256((stage/'arialuni_sdf_u2022').read_bytes()).hexdigest()=='de18a759d475e01f90cffee16bdc861bf3aa09bc501f3c622e5822622b5e1606'
assert not (stage/'BepInEx/plugins/RO3.LocalizationTablePatcher.dll').exists()
assert not list(stage.rglob('*.tsv')) and not list(stage.rglob('*.bat'))
licenses=stage/'BepInEx/config/RO3.ThaiSkills.Licenses';licenses.mkdir(parents=True,exist_ok=True)
with zipfile.ZipFile(archive) as z:
 for filename in ['BepInEx-LICENSE.txt','XUnity-AutoTranslator-LICENSE.txt']:
  matching=[n for n in z.namelist() if n.endswith('/licenses/'+filename)]
  assert len(matching)==1,'Missing component license';(licenses/filename).write_bytes(z.read(matching[0]))
(licenses/'NOTICE.txt').write_text('BepInEx 5.4.23.5 / XUnity 5.6.2 compatibility runtime for RO3.\nPinned compatibility bundle: '+url+'\nSHA256: '+expected+'\nCompatibility modification source: https://github.com/glitzhXEC/RO3_Asia_Thai_Patch/tree/main/tools/legacy-runtime-patching\nXUnity: https://github.com/bbepis/XUnity.AutoTranslator\nFont bundle source: https://github.com/bbepis/XUnity.AutoTranslator/releases/tag/v5.4.5\nNo legacy translation dictionaries or old custom localization plugin are included.\n',encoding='utf-8')
(root/'docs/runtime-provenance.json').write_text(json.dumps({'bundle_url':url,'bundle_sha256':expected,'components':selected,'legacy_translation_files_read':False,'legacy_plugin_included':False,'compatibility_runtime_reused':True,'game_tested':False},indent=2)+'\n',encoding='utf-8')
print('Runtime components extracted:',len(selected),'No old dictionaries/plugin/BAT.')
