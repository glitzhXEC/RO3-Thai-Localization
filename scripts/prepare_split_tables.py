"""Merge fresh Thai overrides with English base from the current game export, by ID."""
import csv,json,re
from pathlib import Path
from live_language_source import read_languages,wire,unescape
root=Path(__file__).resolve().parents[1];en,origins,meta=read_languages(root)
translated={}
with (root/'translations/RO3.LocalizationOverrides.tsv').open(encoding='utf-8',newline='') as f:
 for row in csv.reader(f,delimiter='\t',quoting=csv.QUOTE_NONE):
  if len(row)!=3 or row[0] in translated or row[0] not in en or unescape(row[1])!=en[row[0]]:raise ValueError('Invalid/duplicate/changed English override')
  translated[row[0]]=unescape(row[2])
blocked={r['ID'] for r in json.loads((root/'translations/semantic-review.json').read_text())}
output={key:(translated[key] if key in translated and key not in blocked else text) for key,text in en.items()}
merged=[(key,wire(text),wire(output[key])) for key,text in en.items()]
untranslated=[(key,wire(text),wire(text)) for key,text in en.items() if key not in translated]
for name,rows in [('RO3.LocalizationUntranslated.tsv',untranslated),('RO3.LocalizationMerged.tsv',merged)]:
 with (root/'translations'/name).open('w',encoding='utf-8',newline='') as f:
  for row in rows:
   assert re.fullmatch(r'\d{4,11}',row[0]) and not any(c in ''.join(row) for c in '\t\r\n')
   f.write('\t'.join(row)+'\n')
config=root/'runtime-build/payload/BepInEx/config';config.mkdir(parents=True,exist_ok=True)
with (config/'RO3.LanguageOrigins.tsv').open('w',encoding='utf-8',newline='') as f:
 for key in en:f.write(key+'\t'+wire(origins[key][0])+'\t'+wire(origins[key][1])+'\n')
assert len(translated)+len(untranslated)==len(merged)==len(en)
summary={'source_ids':len(en),'translated_source_ids':len(translated),'untranslated_source_ids':len(untranslated),'runtime_thai_ids':sum(output[k]!=en[k] for k in en),'runtime_english_ids':sum(output[k]==en[k] for k in en),'english_due_to_semantic_review':sorted(blocked),'legacy_thai_used':False,'live_english_sha256':meta['english_canonical_sha256']}
(root/'docs/split-localization-tables.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8');print(json.dumps(summary))
