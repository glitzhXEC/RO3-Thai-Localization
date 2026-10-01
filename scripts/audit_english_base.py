"""Audit the complete English-first runtime feed without guessing language from an emoticon."""
import csv,json,re
from pathlib import Path
from live_language_source import read_languages,unwire
root=Path(__file__).resolve().parents[1]
en,origins,meta=read_languages(root)
with (root/'translations/RO3.LocalizationMerged.tsv').open(encoding='utf-8',newline='') as f:
    rows={r[0]:r for r in csv.reader(f,delimiter='\t',quoting=csv.QUOTE_NONE)}
assert set(rows)==set(en),'Missing/extra English-base IDs'
chinese=re.compile('[\u3400-\u4dbf\u4e00-\u9fff\uf900-\ufaff]')
bad_en=[];bad_targets=[];kept_emoticons=[];english_ids=0
for key,row in rows.items():
    assert len(row)==3 and unwire(row[1])==en[key],key
    english, target=unwire(row[1]),unwire(row[2])
    if chinese.search(english.replace('(╯▔皿▔)╯','')):bad_en.append(key)
    if chinese.search(target.replace('(╯▔皿▔)╯','')):bad_targets.append(key)
    if '(╯▔皿▔)╯' in english:kept_emoticons.append(key)
    if english==target:english_ids+=1
report={'source_ids':len(en),'runtime_ids':len(rows),'runtime_english_ids':english_ids,'runtime_thai_difference_ids':len(rows)-english_ids,'chinese_prose_in_english_ids':bad_en,'chinese_prose_in_target_ids':bad_targets,'preserved_source_emoticon_ids':kept_emoticons,'all_source_ids_have_english_base':True,'complete_thai_translation':False,'game_tested':False}
(root/'docs/english-base-audit.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
assert not bad_en and not bad_targets,'Chinese prose found in data feed'
print(json.dumps(report,ensure_ascii=False))
