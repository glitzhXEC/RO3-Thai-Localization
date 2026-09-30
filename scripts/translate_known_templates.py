"""Translate fully matched, explicitly authored English templates only.
No machine-translation fallback, no old Thai data, no substring replacement.
Unmatched sentences remain pending for new translation and semantic review.
"""
import csv,json,re
from pathlib import Path
root=Path(__file__).resolve().parents[1]
from source_snapshot import read_source
source=read_source(root)
existing={}
for p in (root/'translations').glob('batch-*.th.json'):
 if p.name!='batch-002.th.json':existing.update(json.loads(p.read_text()))
priority={r['ID'] for r in source if r['ID'].startswith(('101103','102203','123901','108001','100501'))}
tok=r'(?:\^\{\d+\})?(?:[$@]\{\d+\}|\d+)(?:%)?(?:\^\{\d+\})?'
stat={
 'Strength':'STR','Dexterity':'DEX','Intelligence':'INT','Agility':'AGI','Vitality':'VIT','Luck':'LUK','STR':'STR','DEX':'DEX','INT':'INT','AGI':'AGI','VIT':'VIT','LUK':'LUK',
 'Physical ATK':'P.ATK','Physical Attack':'P.ATK','PATK':'P.ATK','Magic ATK':'M.ATK','Magic Attack':'M.ATK','MATK':'M.ATK','Physical DEF':'P.DEF','Magic DEF':'M.DEF',
 'Physical and Magic DEF':'P.DEF และ M.DEF','Physical ATK and Magic ATK':'P.ATK และ M.ATK','Physical Attack and Magic Attack':'P.ATK และ M.ATK','PATK and MATK':'P.ATK และ M.ATK',
 'Physical DEF and Magic DEF':'P.DEF และ M.DEF','Physical/Magic DEF':'P.DEF/M.DEF','Physical/Magic ATK':'P.ATK/M.ATK',
 'Movement Speed':'MSPD','Move Speed':'MSPD','MSPD':'MSPD','Attack Speed':'ASPD','ATK Speed':'ASPD','Cast Speed':'Cast Speed','Attack':'ATK','ATK':'ATK','DEF':'DEF',
 'Max Life':'Max HP','Max HP':'Max HP','Life':'HP','HP':'HP','Max MP':'Max MP','MP Recovery Speed':'ความเร็วในการฟื้นฟู MP','MP Regen':'MP Regen',
 'Crit Chance':'CRIT','Critical Rate':'CRIT','CRIT Rate':'CRIT','Crit':'CRIT','CRIT':'CRIT','Crit DMG':'CRIT DMG','CRIT DMG':'CRIT DMG','FLEE':'Flee','Flee':'Flee','HIT':'Hit',
 'Physical Damage':'P.DMG','Physical damage':'P.DMG','Physical DMG':'P.DMG','Magic DMG':'M.DMG','Magic damage':'M.DMG',
 'Damage':'ความเสียหาย','damage':'ความเสียหาย','damage dealt':'ความเสียหายที่สร้าง','damage taken':'ความเสียหายที่ได้รับ','Damage taken':'ความเสียหายที่ได้รับ','Damage Taken':'ความเสียหายที่ได้รับ',
 'Damage Reduction':'การลดความเสียหาย','damage reduction':'การลดความเสียหาย','Damage Increase':'การเพิ่มความเสียหาย','Healing Received':'การฟื้นฟูที่ได้รับ','healing received':'การฟื้นฟูที่ได้รับ','Healing Volume':'ปริมาณการฟื้นฟู','Healing':'การฟื้นฟู',
 'Holy damage':'ความเสียหายธาตุ Holy','Ghost damage dealt':'ความเสียหายธาตุ Ghost ที่สร้าง','Poison damage':'ความเสียหายธาตุ Poison','Fire damage':'ความเสียหายธาตุ Fire','Water damage taken':'ความเสียหายธาตุ Water ที่ได้รับ','magic damage dealt':'ดาเมจเวทที่สร้าง','melee damage dealt':'ความเสียหายระยะประชิดที่สร้าง','Normal Attack damage':'ความเสียหายจากการโจมตีปกติ','Basic Attack damage':'ความเสียหายจากการโจมตีปกติ',
 'Cooldown Reduction':'การลด Cooldown','Ranged Damage':'ความเสียหายระยะไกล','Ranged Damage Taken':'ความเสียหายระยะไกลที่ได้รับ','MP Cost':'MP ที่ใช้','Stun Resistance':'ความต้านทาน Stun'
}
stat_rx='(?:'+'|'.join(re.escape(x) for x in sorted(stat,key=len,reverse=True))+')'
rules=[]
def add(pattern,fn):rules.append((re.compile(pattern),fn))
add(r'\s*[Pp]ermanently unlocks? the (.+) appearance\.?',lambda m:'ปลดล็อกรูปลักษณ์ '+m[1]+' อย่างถาวร')
add(r'Activate the (.+) matching (Theme )?(Headwear|Facewear|Mouthwear|Backwear|Hairstyle|Character Effects) Appearance \(Permanent\)\.',lambda m:'ปลดล็อกรูปลักษณ์'+{'Headwear':'เครื่องประดับศีรษะ','Facewear':'เครื่องประดับใบหน้า','Mouthwear':'เครื่องประดับปาก','Backwear':'เครื่องประดับหลัง','Hairstyle':'ทรงผม','Character Effects':'เอฟเฟกต์ตัวละคร'}[m[3]]+'ที่เข้าชุดกับ'+('ธีม ' if m[2] else ' ')+m[1]+' อย่างถาวร')
add(r'Activate the (.+) (Costume Appearance|Appearance|Hairstyle|Character Effects) \(Permanent\)\.',lambda m:'ปลดล็อก'+{'Costume Appearance':'รูปลักษณ์ชุด ','Appearance':'รูปลักษณ์ ','Hairstyle':'ทรงผม ','Character Effects':'เอฟเฟกต์ตัวละคร '}[m[2]]+m[1]+' อย่างถาวร')
add(r'Mount · (.+) can be unlocked upon use\.',lambda m:'ใช้เพื่อปลดล็อกสัตว์ขี่ '+m[1])
add(r'Use to Activate the (.+) Name Card (Theme|Location)\.',lambda m:'ใช้เพื่อเปิดใช้งาน'+{'Theme':'ธีมนามบัตร ','Location':'สถานที่บนหน้าบัตรชื่อ '}[m[2]]+m[1])
add(r'Use to Activate the (.+) title for ('+tok+r') hours\.',lambda m:'ใช้เพื่อเปิดใช้งานฉายา '+m[1]+' เป็นเวลา '+m[2]+' ชั่วโมง')
add(r'Use to Activate the (.+) (title|Bubble|Portrait Frame|Portrait) \(Permanent\)\.',lambda m:'ใช้เพื่อปลดล็อก'+{'title':'ฉายา ','Bubble':'กรอบข้อความแชต ','Portrait Frame':'กรอบรูปโปรไฟล์ ','Portrait':'รูปโปรไฟล์ '}[m[2]]+m[1]+' อย่างถาวร')
add(r'Use to unlock Chapter (\d+) of a book',lambda m:'ใช้เพื่อปลดล็อกบทที่ '+m[1]+' ของหนังสือ')
add(r'Transform(?: into|:) (Poring|Drops|Poporing|Bombring|Angeling|Deviling|Lava Poring|Red|Blue)',lambda m:'แปลงร่างเป็น '+m[1])
add(r'(?:Changes armor property to|Change Armor to|Convert Armor to) (Earth|Water|Wind|Poison|Fire|Ghost|Holy)(?: Property)?\.?',lambda m:'เปลี่ยนธาตุของชุดเกราะเป็น '+m[1])
add(r'Immune to 【(.+)】\.?',lambda m:'มีภูมิคุ้มกันต่อ [ '+m[1]+' ]')
# Full stat-only expressions: every component must be in the whitelist.
def expression(en):
    en=en.rstrip('.')
    parts=re.split(r', | and ',en)
    out=[]
    for part in parts:
        m=re.fullmatch('('+stat_rx+') ([+-])('+tok+')',part)
        if not m:return None
        out.append(stat[m[1]]+' '+m[2]+m[3])
    return ', '.join(out) if out else None
add(r'Increases (?:your )?('+stat_rx+r') by ('+tok+r')\.?',lambda m:'เพิ่ม '+stat[m[1]]+' ขึ้น '+m[2])
add(r'Reduces ('+stat_rx+r') by ('+tok+r')\.?',lambda m:'ลด '+stat[m[1]]+' ลง '+m[2])
add(r'('+stat_rx+r') (increases|increased|decreases|decreased|reduced|is reduced) by ('+tok+r')\.?',lambda m:('เพิ่ม ' if m[2] in ('increases','increased') else 'ลด ')+stat[m[1]]+(' ขึ้น ' if m[2] in ('increases','increased') else ' ลง ')+m[3])
add(r'Increases (?:your )?('+stat_rx+r') by ('+tok+r') for ('+tok+r') (seconds|sec\.)\.?',lambda m:'เพิ่ม '+stat[m[1]]+' ขึ้น '+m[2]+' เป็นเวลา '+m[3]+' วินาที')
add(r'Reduces ('+stat_rx+r') by ('+tok+r') for ('+tok+r') (seconds|sec\.)\.?',lambda m:'ลด '+stat[m[1]]+' ลง '+m[2]+' เป็นเวลา '+m[3]+' วินาที')
add(r'Gain ('+tok+r') Holy damage\.',lambda m:'ได้รับโบนัส '+m[1]+' ให้แก่ความเสียหายธาตุ Holy')
add(r'Gain (?:a |a Shield |a shield )?(?:shield |Shield )?equal to ('+tok+r') (?:of )?(?:your Life|of max HP|Max Life|Max HP)\.',lambda m:'ได้รับโล่เท่ากับ '+m[1]+(' ของ HP' if 'your Life' in m[0] else ' ของ Max HP'))
add(r'Shield equal to ('+tok+r') (?:of )?(Max Life|Max HP|of Max Life)\.?',lambda m:'โล่เท่ากับ '+m[1]+' ของ Max HP')
add(r'Stacks up to ('+tok+r') times\.',lambda m:'สะสมได้สูงสุด '+m[1]+' stacks')
add(r'Restores ('+tok+r') Life per second\.',lambda m:'ฟื้นฟู HP '+m[1]+' แต้มทุกวินาที')
add(r'Damage against (.+) \+('+tok+r')\.',lambda m:'ความเสียหายต่อ '+(m[1][4:]+' ทั้งหมด' if m[1].startswith('all ') else m[1])+' +'+m[2])
add(r'Increases 【(.+)】 damage by ('+tok+r')\.',lambda m:'เพิ่มความเสียหายของ [ '+m[1]+' ] ขึ้น '+m[2])
add(r'【(.+)】 damage increased by ('+tok+r')\.',lambda m:'[ '+m[1]+' ] สร้างความเสียหายเพิ่มขึ้น '+m[2])
add(r'Reduces skill cooldowns by ('+tok+r')\.',lambda m:'ลด Cooldown ของสกิลลง '+m[1])
add(r'Reduces skill cooldowns by ('+tok+r') for ('+tok+r') sec\.',lambda m:'ลด Cooldown ของสกิลลง '+m[1]+' เป็นเวลา '+m[2]+' วินาที')

# English skill-enhancement sentences with a complete grammatical match.
skill = r'((?:\^\{\d+\})*(?:【[^】]+】|\[[^\]]+\])(?:\^\{\d+\})*)'
def canonical_name(value):
    return re.sub(r'【([^】]+)】|\[([^\]]+)\]',lambda x:'[ '+(x[1] or x[2]).strip()+' ]',value)
extra_stats = {'Physical/Magic ATK':'P.ATK/M.ATK','PDEF/MDEF':'P.DEF/M.DEF','MDEF/PDEF':'M.DEF/P.DEF','MDEF':'M.DEF','PDEF':'P.DEF'}
stat.update(extra_stats)
stat_rx='(?:'+'|'.join(re.escape(x) for x in sorted(stat,key=len,reverse=True))+')'
add(skill+r"'s percentage(?:-based)? damage multiplier increases by ("+tok+r") and (?:its )?fixed damage increases by ("+tok+r")\.",lambda m:'ตัวคูณความเสียหายแบบเปอร์เซ็นต์ของ '+canonical_name(m[1])+' เพิ่มขึ้น '+m[2]+' และความเสียหายจริงเพิ่มขึ้น '+m[3])
add(skill+r"'s percentage damage multiplier increases by ("+tok+r")\.",lambda m:'ตัวคูณความเสียหายแบบเปอร์เซ็นต์ของ '+canonical_name(m[1])+' เพิ่มขึ้น '+m[2])
add(skill+r"'s percentage Healing multiplier increases by ("+tok+r") and its fixed Healing increases by ("+tok+r")\.",lambda m:'ตัวคูณการฟื้นฟูแบบเปอร์เซ็นต์ของ '+canonical_name(m[1])+' เพิ่มขึ้น '+m[2]+' และค่าการฟื้นฟูคงที่เพิ่มขึ้น '+m[3])
add(r'When '+skill+r' reaches Lv\. ('+tok+r'), ('+stat_rx+r') \+('+tok+r')\.',lambda m:'เมื่อ '+canonical_name(m[1])+' ถึง Lv. '+m[2]+' จะได้รับ '+stat[m[3]]+' +'+m[4])
add(r'When '+skill+r' or '+skill+r' reaches Lv\. ('+tok+r'), ('+stat_rx+r') \+('+tok+r')\.',lambda m:'เมื่อ '+canonical_name(m[1])+' หรือ '+canonical_name(m[2])+' ถึง Lv. '+m[3]+' จะได้รับ '+stat[m[4]]+' +'+m[5])
add(skill+r' target count \+('+tok+r')\.?',lambda m:canonical_name(m[1])+' จำนวนเป้าหมาย +'+m[2])
add(skill+r' Variable CT -('+tok+r') seconds\.',lambda m:canonical_name(m[1])+' VCT -'+m[2]+' วินาที')
add(skill+r' and '+skill+r' Variable CT -('+tok+r')\.',lambda m:canonical_name(m[1])+' และ '+canonical_name(m[2])+' VCT -'+m[3])
add(r'While '+skill+r' is active, ('+stat_rx+r') \+('+tok+r')\.',lambda m:'ขณะ '+canonical_name(m[1])+' ทำงาน '+stat[m[2]]+' +'+m[3])
add(r'When a (mace|dagger|katar|one-handed or two-handed staff) is equipped, ('+stat_rx+r') \+('+tok+r')\.',lambda m:'เมื่อสวม'+{'mace':'กระบอง','dagger':'มีด','katar':'กาตาร์','one-handed or two-handed staff':'คทามือเดียวหรือคทาสองมือ'}[m[1]]+' '+stat[m[2]]+' +'+m[3])
add(r'Allies within '+skill+r"'s area gain ("+stat_rx+r') \+('+tok+r')\.',lambda m:'พันธมิตรภายในพื้นที่ของ '+canonical_name(m[1])+' ได้รับ '+stat[m[2]]+' +'+m[3])

by_english={}
for key,thai in existing.items():
 en=next(r['English'] for r in source if r['ID']==key)
 if en in by_english and by_english[en]!=thai:raise ValueError('Conflicting new translation')
 by_english[en]=thai
out={};rule_log=[]
for row in source:
 key,en=row['ID'],row['English']
 if key not in priority or key in existing:continue
 thai=by_english.get(en);rule='exact-English-match-to-new-batch'
 if thai is None:
  for rx,fn in rules:
   m=rx.fullmatch(en)
   if m:
    thai=fn(m);rule=rx.pattern;break
 if thai is None:
  thai=expression(en);rule='full-whitelist-stat-expression'
 if thai is not None:
  out[key]=thai;rule_log.append({'ID':key,'English':en,'rule':rule})
(root/'translations/batch-002.th.json').write_text(json.dumps(out,ensure_ascii=False,indent=2)+'\n')
(root/'docs/batch-002-template-provenance.json').write_text(json.dumps(rule_log,ensure_ascii=False,indent=2)+'\n')
print('New rows:',len(out));print('Categories:',{p:sum(k.startswith(p) for k in out) for p in ('101103','102203','123901','108001','100501')})
