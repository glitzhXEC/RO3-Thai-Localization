"""Additional menu QA: count numbers after decoding escaped control boundaries."""
import collections,json,re
from pathlib import Path
from live_language_source import read_authoring_source
root=Path(__file__).resolve().parents[1]
source=read_authoring_source(root)
def numbers(s):
    # Preserve protected IDs/placeholders/tags separately in the main validator.
    # Decode control delimiters before numeric matching so \\n2 is counted as 2.
    s=s.replace('\\n','\n').replace('\\r','\r').replace('\\t','\t')
    return collections.Counter(re.findall(r'(?<![A-Za-z])\d+(?:\.\d+)?',s))
checked=0
for path in sorted((root/'translations').glob('batch-*.th.json')):
    if int(path.name.split('-')[1].split('.')[0])<46:continue
    for key,thai in json.loads(path.read_text()).items():
        assert numbers(source[key])==numbers(thai),(key,numbers(source[key]),numbers(thai))
        checked+=1
assert numbers('Items\\n2-player party')=={'2':1}
assert numbers('Items\\n2-player party')!=numbers('ไอเทม\\n3 คนในปาร์ตี้')
print(f'PASS: {checked} audited ID pairs (menus and descriptions); decoded-control numeric check and changed-digit rejection')
