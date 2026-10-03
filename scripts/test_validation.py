"""Check validator success and deliberate corruption, restoring the batch afterward."""
import json,subprocess,sys
from pathlib import Path
root=Path(__file__).resolve().parents[1]
batch=root/'translations/batch-001.th.json'
original=batch.read_bytes()
def run():
    return subprocess.run([sys.executable,str(root/'scripts/validate_translations.py')],capture_output=True,text=True)
try:
    assert run().returncode==0, 'Baseline failed'
    data=json.loads(original)
    data['10110300000']=data['10110300000'].replace('${3}','${99}')
    batch.write_text(json.dumps(data,ensure_ascii=False))
    assert run().returncode!=0, 'Corrupted placeholder not rejected'
    data=json.loads(original)
    data['10220300542']=data['10220300542'].replace('*','')
    batch.write_text(json.dumps(data,ensure_ascii=False))
    assert run().returncode!=0, 'Missing multiplication not rejected'
    data=json.loads(original)
    data['10220300542']=data['10220300542'].replace('[ Frozen ]','[ Ice ]')
    batch.write_text(json.dumps(data,ensure_ascii=False))
    assert run().returncode!=0, 'Changed bracket name not rejected'
    data=json.loads(original)
    data['10110300000']+='\ufffd'
    batch.write_text(json.dumps(data,ensure_ascii=False))
    assert run().returncode!=0, 'Broken Unicode not rejected'
    data=json.loads(original)
    data['10110300000']+='ZXQ0000QXZ'
    batch.write_text(json.dumps(data,ensure_ascii=False))
    assert run().returncode!=0, 'Draft marker not rejected'
finally:
    batch.write_bytes(original)
# Newly supported live-source controls must not be dropped or normalized away.
for key,control in [('44010','\\r'),('53001','\\t')]:
    matches=[p for p in (root/'translations').glob('batch-*.th.json') if key in json.loads(p.read_text(encoding='utf-8'))]
    assert len(matches)==1, 'Live-source regression case missing/duplicated'
    target=matches[0];saved=target.read_bytes()
    try:
        data=json.loads(saved)
        assert control in data[key]
        data[key]=data[key].replace(control,'',1)
        target.write_text(json.dumps(data,ensure_ascii=False),encoding='utf-8')
        assert run().returncode!=0, 'Dropped live-source control was not rejected'
    finally:
        target.write_bytes(saved)
# Malformed nested source brackets are allowed only when the raw segment remains byte-faithful.
key='10110301061'
matches=[p for p in (root/'translations').glob('batch-*.th.json') if key in json.loads(p.read_text(encoding='utf-8'))]
assert len(matches)==1, 'Malformed-bracket regression case missing/duplicated'
target=matches[0];saved=target.read_bytes()
try:
    data=json.loads(saved)
    data[key]=data[key].replace('Poisoned','Frozen',1)
    target.write_text(json.dumps(data,ensure_ascii=False),encoding='utf-8')
    assert run().returncode!=0, 'Changed malformed bracket segment not rejected'
finally:
    target.write_bytes(saved)
# Canonical stat spellings reject full-form and uppercase legacy variants.
for key,canonical,legacy in [('10110301042','P.ATK','Physical ATK'),('10110301042','FLEE','Flee')]:
    matches=[p for p in (root/'translations').glob('batch-*.th.json') if key in json.loads(p.read_text(encoding='utf-8'))]
    assert len(matches)==1, 'Canonical-stat regression case missing/duplicated'
    target=matches[0];saved=target.read_bytes()
    try:
        data=json.loads(saved)
        data[key]=data[key].replace(canonical,legacy,1)
        target.write_text(json.dumps(data,ensure_ascii=False),encoding='utf-8')
        assert run().returncode!=0, 'Legacy stat spelling not rejected'
    finally:
        target.write_bytes(saved)
# Stunt must use the approved Thai glossary term.
key='12074'
matches=[p for p in (root/'translations').glob('batch-*.th.json') if key in json.loads(p.read_text(encoding='utf-8'))]
assert len(matches)==1, 'Stunt glossary regression case missing/duplicated'
target=matches[0];saved=target.read_bytes()
try:
    data=json.loads(saved)
    data[key]=data[key].replace('ออปชั่นพิเศษ','Stunt',1)
    target.write_text(json.dumps(data,ensure_ascii=False),encoding='utf-8')
    assert run().returncode!=0, 'Legacy Stunt/Stunts term not rejected'
finally:
    target.write_bytes(saved)
# No approved Thai target may retain singular or plural Stunt.
import re
for path in (root/'translations').glob('batch-*.th.json'):
    for item_id,text in json.loads(path.read_text(encoding='utf-8')).items():
        assert not re.search(r'\bStunts?\b',text), 'Legacy Stunt target remains: '+item_id
assert run().returncode==0, 'Restored baseline failed'
print('PASS: baseline, corrupt placeholder, missing multiplication, changed bracket name, broken Unicode, draft marker, dropped live CR/tab controls, malformed bracket preservation, canonical stats, Stunt glossary, restored baseline')
