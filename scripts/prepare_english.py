"""Extract only English and IDs. Never open legacy Thai dictionaries."""
import csv, json, hashlib, subprocess
from pathlib import Path
import sys
root = Path(__file__).resolve().parents[1]
source = Path(sys.argv[1])
raw = source / '_TranslationWorkspace/LanguageKV_full_en.tsv'
rows=[]; anomalies=[]
with raw.open(encoding='utf-8-sig', newline='') as f:
    for line, row in enumerate(csv.reader(f, delimiter='\t'),1):
        if len(row)!=2:
            anomalies.append({'line':line,'id':row[0] if row else None,'columns':len(row)})
            continue
        rows.append({'ID':row[0],'English':row[1]})
assert len({r['ID'] for r in rows})==len(rows), 'Duplicate source IDs'
(root/'translations/source').mkdir(parents=True,exist_ok=True)
for old in (root/'translations/source').glob('english-part-*.tsv'): old.unlink()
(root/'translations/source/english.tsv').unlink(missing_ok=True)
for offset in range(0,len(rows),5000):
    part=root/'translations/source'/f'english-part-{offset//5000+1:02d}.tsv'
    with part.open('w',encoding='utf-8',newline='') as f:
        w=csv.DictWriter(f,fieldnames=['ID','English'],delimiter='\t');w.writeheader();w.writerows(rows[offset:offset+5000])
# These are candidates from observed ID families, not a proven exhaustive taxonomy.
prefixes={'101103':'skill-description','102203':'skill-effect','123901':'item-description','108001':'skill-enhancement','100501':'food-potion-description'}
queue=[{**r,'Category':prefixes[r['ID'][:6]]} for r in rows if r['ID'][:6] in prefixes]
with (root/'translations/source/priority-queue.tsv').open('w',encoding='utf-8',newline='') as f:
    w=csv.DictWriter(f,fieldnames=['ID','English','Category'],delimiter='\t');w.writeheader();w.writerows(queue)
manifest={'source_repository':'https://github.com/glitzhXEC/RO3_Asia_Thai_Patch','source_commit':subprocess.check_output(['git','-C',str(source),'rev-parse','HEAD'],text=True).strip(),'source_file_sha256':hashlib.sha256(raw.read_bytes()).hexdigest(),'valid_source_rows':len(rows),'candidate_priority_rows':len(queue),'anomalies_excluded':anomalies,'legacy_thai_used':False,'scope_note':'ID-family candidates require further classification; not exhaustive.'}
(root/'translations/source/provenance.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n')
print(json.dumps({'source_rows':len(rows),'priority_candidates':len(queue),'anomalies':anomalies}))
