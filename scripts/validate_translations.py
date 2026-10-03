"""Validate freshly written batches against the English-only snapshot."""
import csv,json,re,collections,sys
from pathlib import Path
root=Path(__file__).resolve().parents[1]
from live_language_source import read_authoring_source
source=read_authoring_source(root)
protected=re.compile(r'[$@^]\{\d+\}|(?<![$@^])\{\d+\}|%(?:\d+\$)?[sdif]|<[^>]+>|\\[nrt]')
math=re.compile(r'[%*+]')
numbers=re.compile(r'(?<![A-Za-z])\d+(?:\.\d+)?')
brackets=re.compile(r'【([^】]+)】|\[([^\]]+)\]')
localized_bracket_labels={'13150300002': 'ดันเจี้ยน', '13150300003': 'แข่งม้า', '13150300004': 'ภารกิจว่าจ้าง', '13150300005': 'เควสต์หลัก', '13150300006': 'เปลี่ยนอาชีพ', '13150300007': 'พิชิต', '13150300008': 'เควสต์รอง', '13150300009': 'ซ่อนเร้น', '13150300010': 'คู่มือ', '13150300011': 'อีเวนต์', '13150300012': 'ฝ่าย', '10110301094': ['${1}%*P.ATK', 'Shadow Strike', 'Shadow Strike'], '10110301083': ['Basic Attack', 'Cross Impact', '${1}% P.ATK'], '10110301084': ['Basic Attack', 'Cross Impact', '${2}% P.ATK'], '10110301085': ['Basic Attack', 'Cross Impact', '${3}% P.ATK'], '10110301086': ['Basic Attack', 'Cross Impact', '${4}% P.ATK'], '10110301087': ['Basic Attack', 'Cross Impact', '${5}% P.ATK'], '10110301088': ['Basic Attack', 'Cross Impact', '${6}% P.ATK'], '10110301089': ['Basic Attack', 'Cross Impact', '${7}% P.ATK'], '10110301090': ['Basic Attack', 'Cross Impact', '${8}% P.ATK'], '10110301091': ['Basic Attack', 'Cross Impact', '${9}% P.ATK'], '10110301092': ['Basic Attack', 'Cross Impact', '${10}% P.ATK']}
errors=[];warnings=[];translations={}
for path in sorted((root/'translations').glob('batch-*.th.json')):
    for key,thai in json.loads(path.read_text(encoding='utf-8')).items():
        if key in translations:errors.append({'ID':key,'check':'duplicate_translation'})
        translations[key]=thai
        if key not in source:errors.append({'ID':key,'check':'unknown_source'});continue
        if '\ufffd' in thai:errors.append({'ID':key,'check':'replacement_character'})
        if re.search(r'ZX[QR]\d{4}[QR]XZ',thai):errors.append({'ID':key,'check':'draft_marker_residue'})
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
        malformed_bracket_segments=re.findall(r'【[^】]*【[^】]*】',en)
        if malformed_bracket_segments:
            for segment in malformed_bracket_segments:
                if segment not in thai:errors.append({'ID':key,'check':'malformed_bracket_segment','segment':segment})
        else:
            names=[(a or b).strip() for a,b in brackets.findall(en)]
            thai_names=[(a or b).strip() for a,b in brackets.findall(thai)]
            if key in localized_bracket_labels:
                expected=localized_bracket_labels[key] if isinstance(localized_bracket_labels[key],list) else [localized_bracket_labels[key]]
                if thai_names!=expected:errors.append({'ID':key,'check':'localized_bracket_label','expected':expected,'thai':thai_names})
                for label in expected:
                    if '[ '+label+' ]' not in thai:errors.append({'ID':key,'check':'localized_bracket_spacing','label':label})
            else:
                if names!=thai_names:errors.append({'ID':key,'check':'bracket_name_order_or_count'})
                for name in names:
                    if '[ '+name+' ]' not in thai:errors.append({'ID':key,'check':'bracket_name','name':name})
        if re.search(r'\bST\b',thai):errors.append({'ID':key,'check':'ST_not_expanded'})
        if re.search(r'สแต็ก|สแตค',thai):errors.append({'ID':key,'check':'stacks_transliterated'})
        noncanonical_stat_patterns=(
            r'\b(PATK|MATK|PDEF|MDEF|MDMG|PDMG)\b',
            r'\b(Physical ATK|Magic ATK|Physical DEF|Magic DEF|Attack Speed|Movement Speed|Critical Rate|FLEE|Dodge|Crit)\b',
        )
        if any(re.search(pattern,thai) for pattern in noncanonical_stat_patterns):errors.append({'ID':key,'check':'noncanonical_stat'})
        if not re.search(r'[\u0e00-\u0e7f]',thai):warnings.append({'ID':key,'check':'no_thai'})
report={'translated_rows':len(translations),'errors':errors,'warnings':warnings,'in_game_tested':False,'semantic_review':'English-only fresh translations; eligible priority drafts reviewed/rewritten. Withheld source rows remain excluded. Independent linguistic and in-game QA pending.'}
(root/'docs/qa-translations.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
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
