"""Validate freshly written batches against the English-only snapshot."""
import csv,json,re,collections,sys
from pathlib import Path
root=Path(__file__).resolve().parents[1]
from source_snapshot import read_source
source={r['ID']:r['English'] for r in read_source(root)}
protected=re.compile(r'[$@^]\{\d+\}|(?<![$@^])\{\d+\}|%(?:\d+\$)?[sdif]|<[^>]+>|\\[nrt]')
math=re.compile(r'[%*+]')
numbers=re.compile(r'(?<![A-Za-z])\d+(?:\.\d+)?')
brackets=re.compile(r'【([^】]+)】|\[([^\]]+)\]')
errors=[];warnings=[];translations={}
for path in sorted((root/'translations').glob('batch-*.th.json')):
    for key,thai in json.loads(path.read_text()).items():
        if key in translations:errors.append({'ID':key,'check':'duplicate_translation'})
        translations[key]=thai
        if key not in source:errors.append({'ID':key,'check':'unknown_source'});continue
        en=source[key]
        a,b=protected.findall(en),protected.findall(thai)
        if a!=b:errors.append({'ID':key,'check':'protected_token_order','english':a,'thai':b})
        if collections.Counter(math.findall(en))!=collections.Counter(math.findall(thai)):
            errors.append({'ID':key,'check':'math_symbol_counts'})
        a,b=collections.Counter(numbers.findall(en)),collections.Counter(numbers.findall(thai))
        if a!=b:errors.append({'ID':key,'check':'numeric_values','english':dict(a),'thai':dict(b)})
        if en.count('\n')!=thai.count('\n'):errors.append({'ID':key,'check':'physical_newlines'})
        for escape in ('\\n','\\r','\\t'):
            if en.count(escape)!=thai.count(escape):errors.append({'ID':key,'check':'escaped_control_counts','token':escape})
        names=[(a or b).strip() for a,b in brackets.findall(en)]
        thai_names=[(a or b).strip() for a,b in brackets.findall(thai)]
        if names!=thai_names:errors.append({'ID':key,'check':'bracket_name_order_or_count'})
        for name in names:
            if '[ '+name+' ]' not in thai:errors.append({'ID':key,'check':'bracket_name','name':name})
        if re.search(r'\bST\b',thai):errors.append({'ID':key,'check':'ST_not_expanded'})
        if re.search(r'สแต็ก|สแตค',thai):errors.append({'ID':key,'check':'stacks_transliterated'})
        if re.search(r'\b(PATK|MATK|PDEF|MDEF|MDMG|PDMG)\b',thai):errors.append({'ID':key,'check':'noncanonical_stat'})
        if not re.search(r'[\u0e00-\u0e7f]',thai):warnings.append({'ID':key,'check':'no_thai'})
report={'translated_rows':len(translations),'errors':errors,'warnings':warnings,'in_game_tested':False,'semantic_review':'Draft translations; further review required.'}
(root/'docs/qa-translations.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(report,ensure_ascii=False,indent=2))
if errors:sys.exit(1)
with (root/'translations/retranslated.tsv').open('w',encoding='utf-8',newline='') as f:
    w=csv.writer(f,delimiter='\t');w.writerow(['ID','English','Thai_Translation'])
    for key,thai in translations.items():w.writerow([key,source[key],thai])
# Only audited newly translated rows enter the runtime table. No old fallback.
with (root/'translations/RO3.LocalizationOverrides.tsv').open('w',encoding='utf-8',newline='') as f:
    for key,thai in translations.items():
        en=source[key]
        assert not any(c in en+thai for c in '\t\r\n'), 'Runtime values require explicit escaping'
        f.write(key+'\t'+en+'\t'+thai+'\n')
