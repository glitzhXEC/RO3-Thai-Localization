"""Publish data-only, content-versioned dictionaries built from the English-only TSV.
Run after validated changes to the main Overrides TSV. No executable files or runtime ZIP is published here.
"""
import argparse, hashlib, json, subprocess, sys
from pathlib import Path
root=Path(__file__).resolve().parents[1]
p=argparse.ArgumentParser();p.add_argument('--seed-config');args=p.parse_args()
subprocess.run([sys.executable,'scripts/prepare_skill_runtime.py','--all','--from-overrides'],cwd=root,check=True)
# Generator also asserts no conflicting exact English translations.
config=root/'runtime-build/payload/BepInEx/config'
files=[('RO3.LocalizationOverrides.tsv',(config/'RO3.SkillTranslations.tsv').read_bytes()),('RO3.LocalizationRules.tsv',(config/'RO3.SkillRules.tsv').read_bytes())]
assert all(len(b)<=8*1024*1024 for _,b in files)
metadata=[{'Name':name,'Sha256':hashlib.sha256(b).hexdigest(),'Bytes':len(b)} for name,b in files]
version=hashlib.sha256((metadata[0]['Sha256']+'|'+metadata[1]['Sha256']).encode()).hexdigest()
summary=json.loads((root/'docs/partial-skill-payload.json').read_text())
manifest={'Schema':1,'RuntimeSchema':1,'Version':version,'SourceSha256':hashlib.sha256((root/'translations/RO3.LocalizationOverrides.tsv').read_bytes()).hexdigest(),'TranslationIds':summary['translation_ids'],'RuntimeRules':summary['numeric_runtime_rules'],'Files':metadata}
live=root/'translations/live';folder=live/'versions'/version;folder.mkdir(parents=True,exist_ok=True)
for name,contents in files:
 target=folder/name
 if target.exists():assert target.read_bytes()==contents,'Content-addressed version must never be changed'
 else:target.write_bytes(contents)
(live/'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
(live/'README.md').write_text('''# Translation data feed — schema 1\n\nGenerated from approved English-only batches and RO3.LocalizationOverrides.tsv. Never hand-edit a version directory.\nmanifest.json points to two SHA-256 checked, content-versioned TSV files. No DLL/EXE/ZIP or arbitrary URLs are supported by the client updater.\nThe full source table remains in translations/RO3.LocalizationOverrides.tsv. Known semantic-review IDs are excluded from the runtime feed.\nPublish a new feed by running scripts/prepare_live_translations.py and runtime tests, then committing the manifest and immutable version files together.\nThe updater checks only on startup, caches a validated bundle atomically, and keeps the previous dictionary when offline or validation fails.\nSHA-256 checks consistency/integrity, not publisher identity; trust is the configured HTTPS GitHub repository.\n''',encoding='utf-8')
if args.seed_config:
 destination=(root/args.seed_config/'RO3.TranslationCache/cache.json');destination.parent.mkdir(parents=True,exist_ok=True)
 destination.write_text(json.dumps({'Manifest':manifest,'Table':files[0][1].decode('utf-8'),'Rules':files[1][1].decode('utf-8')},ensure_ascii=False,separators=(',',':')),encoding='utf-8')
(root/'docs/translation-update-feed.json').write_text(json.dumps({'schema':1,'data_version':version,'source_overrides_sha256':manifest['SourceSha256'],'translation_ids':manifest['TranslationIds'],'runtime_rules':manifest['RuntimeRules'],'data_only':True,'game_tested':False},indent=2)+'\n',encoding='utf-8')
print('Translation feed prepared:',version,manifest['TranslationIds'],'IDs; data only.')
