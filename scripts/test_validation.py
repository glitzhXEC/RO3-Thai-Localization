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
finally:
    batch.write_bytes(original)
assert run().returncode==0, 'Restored baseline failed'
print('PASS: baseline, corrupt placeholder, missing multiplication, changed bracket name, restored baseline')
