"""Append only authored, complete-English skill templates; never regenerate old batches.
No model/API fallback or partial substitutions. Ambiguous and internal rows stay pending.
"""
import json,re,collections
from pathlib import Path
from source_snapshot import read_source
root=Path(__file__).resolve().parents[1]
source=read_source(root);existing={}
for p in sorted((root/'translations').glob('batch-*.th.json')):
 if p.name!='batch-016.th.json':existing.update(json.loads(p.read_text(encoding='utf-8')))
blocked={x['ID'] for x in json.loads((root/'translations/semantic-review.json').read_text(encoding='utf-8'))}
value=r'(?:\^\{\d+\})*(?:[$@]\{\d+\}|\d+(?:\.\d+)?)(?:\^\{\d+\})*'
skill=r'((?:\^\{\d+\})*(?:【[^】\r\n]+】|\[[^\]\r\n]+\])(?:\^\{\d+\})*)'
v='('+value+')'
def name(s):return re.sub(r'【([^】]+)】|\[([^\]]+)\]',lambda m:'[ '+(m[1] or m[2]).strip()+' ]',s)
rules=[]
def add(pattern,fn,label):rules.append((re.compile(pattern),fn,label))
add(skill+r"'s percentage(?:-based)? damage multiplier increases by "+v+r"%, and its fixed damage increases by "+v+r'\.',lambda m:'ตัวคูณดาเมจแบบเปอร์เซ็นต์ของ '+name(m[1])+' เพิ่มขึ้น '+m[2]+'% และดาเมจส่วนค่าคงที่เพิ่มขึ้น '+m[3],'skill-percent-and-flat-damage')
add(skill+r"'s percentage damage multiplier and fixed damage increase by "+v+r'%\.',lambda m:'ตัวคูณดาเมจแบบเปอร์เซ็นต์และดาเมจส่วนค่าคงที่ของ '+name(m[1])+' เพิ่มขึ้น '+m[2]+'%','skill-both-damage-percent')
add(skill+r' gains an Extra '+v+r'% Crit Chance\.',lambda m:name(m[1])+' ได้รับ CRIT เพิ่มอีก '+m[2]+'%','skill-extra-crit')
add(skill+r"'s Cooldown is changed to "+v+r' seconds\.',lambda m:'เปลี่ยน Cooldown ของ '+name(m[1])+' เป็น '+m[2]+' วินาที','skill-cooldown-replacement')
add(skill+r' duration is extended by '+v+r' sec\.',lambda m:name(m[1])+' เพิ่มระยะเวลาคงอยู่ '+m[2]+' วินาที','skill-duration-extension')
add(skill+r"'s duration increases to "+v+r' seconds\.',lambda m:name(m[1])+' คงอยู่นานขึ้นเป็น '+m[2]+' วินาที','skill-duration-increase-to')
add(skill+r' can also Deals DMG to '+v+r' Other enemies within '+v+r' meters of the target\.',lambda m:name(m[1])+' สามารถสร้างดาเมจแก่ศัตรูอื่น '+m[2]+' ตัว ในระยะ '+m[3]+' เมตรรอบเป้าหมายด้วย','skill-extra-nearby-enemies')
add(skill+r' ricochets an Extra '+v+r' times\.',lambda m:name(m[1])+' ชิ่งเพิ่มอีก '+m[2]+' ครั้ง','skill-extra-ricochet')
add(r'When casting '+skill+r', there is a '+v+r'% Chance to cast it one Extra time\.',lambda m:'เมื่อใช้ '+name(m[1])+' มีโอกาส '+m[2]+'% ใช้ซ้ำเพิ่มอีกหนึ่งครั้ง','skill-extra-cast')
add(r'When dealing damage, has a '+v+r'% Chance to inflict '+skill+r' on the enemy for '+v+r' sec\. Internal Cooldown: '+v+r' sec\. Chance stacks with multiple cards\.',lambda m:'เมื่อสร้างดาเมจ มีโอกาส '+m[1]+'% ทำให้ศัตรูติดสถานะ '+name(m[2])+' เป็นเวลา '+m[3]+' วินาที CD ภายใน: '+m[4]+' วินาที โอกาสสะสมได้เมื่อใช้การ์ดหลายใบ','card-damage-status')
add(r'When dealing damage, has a '+v+r'% Chance to inflict '+skill+r' for '+v+r' seconds\. Internal Cooldown: '+v+r' seconds\.',lambda m:'เมื่อสร้างดาเมจ มีโอกาส '+m[1]+'% ทำให้ติดสถานะ '+name(m[2])+' เป็นเวลา '+m[3]+' วินาที CD ภายใน: '+m[4]+' วินาที','damage-status-seconds')
add(r'Restores '+v+r'% Max HP per sec for '+v+r' sec\.',lambda m:'ฟื้นฟู '+m[1]+'% ของ Max HP ทุกวินาที เป็นเวลา '+m[2]+' วินาที','restore-maxhp')
add(r'Recover '+v+r'% Max Life per sec\. for '+v+r' sec\.',lambda m:'ฟื้นฟู '+m[1]+'% ของ Max HP ทุกวินาที เป็นเวลา '+m[2]+' วินาที','recover-maxhp')
add(r'Restores '+v+r'% of your Life\.',lambda m:'ฟื้นฟู HP ของคุณ '+m[1]+'%','restore-current-life-source-wording')
add(r'Inflicts '+skill+r' for '+v+r' (?:sec\.|seconds\.)',lambda m:'ทำให้ติดสถานะ '+name(m[1])+' เป็นเวลา '+m[2]+' วินาที','status-duration')
add(r'Grants '+skill+r' for '+v+r' (?:sec\.|seconds\.)',lambda m:'มอบสถานะ '+name(m[1])+' เป็นเวลา '+m[2]+' วินาที','grant-status-duration')
add(r'Reduces damage taken from monsters by '+v+r'% and increases Physical and Magic DEF by '+v+r'%\.?',lambda m:'ลดดาเมจที่ได้รับจากมอนสเตอร์ลง '+m[1]+'% และเพิ่ม P.DEF กับ M.DEF ขึ้น '+m[2]+'%','monster-def')
add(r'Each stack reduces Movement Speed by '+v+r'%\. At '+v+r' stacks, inflicts Stun for '+v+r' sec\.',lambda m:'แต่ละ stacks ลด MSPD ลง '+m[1]+'% เมื่อครบ '+m[2]+' stacks ทำให้ติด Stun เป็นเวลา '+m[3]+' วินาที','stack-slow-stun')
stats={'Physical ATK':'P.ATK','Physical Attack':'P.ATK','PATK':'P.ATK','Magic ATK':'M.ATK','Magic Attack':'M.ATK','MATK':'M.ATK','Physical DEF':'P.DEF','Magic DEF':'M.DEF','PDEF':'P.DEF','MDEF':'M.DEF','Physical and Magic DEF':'P.DEF และ M.DEF','Physical/Magic ATK':'P.ATK/M.ATK','Physical/Magic DEF':'P.DEF/M.DEF','Movement Speed':'MSPD','Move Speed':'MSPD','MSPD':'MSPD','Attack Speed':'ASPD','ASPD':'ASPD','Max Life':'Max HP','Max HP':'Max HP','Life':'HP','HP':'HP','MP':'MP','Max MP':'Max MP','CRIT':'CRIT','Crit':'CRIT','Critical Rate':'CRIT','Crit Chance':'CRIT','Crit Resistance':'CRIT RES','CRIT DMG':'CRIT DMG','Crit DMG':'CRIT DMG','FLEE':'FLEE','Flee':'FLEE','HIT':'Hit','Hit':'Hit','STR':'STR','DEX':'DEX','INT':'INT','AGI':'AGI','VIT':'VIT','LUK':'LUK','DEF':'DEF','ATK':'ATK','Physical damage':'P.DMG','Physical DMG':'P.DMG','Magic damage':'M.DMG','Magic DMG':'M.DMG','Physical Damage':'P.DMG','Magic Damage':'M.DMG','damage taken':'ดาเมจที่ได้รับ','damage dealt':'ดาเมจที่ทำได้','Damage Reduction':'การลดดาเมจ','Holy Resistance':'ความต้านทาน Holy','Demon Resistance':'ความต้านทาน Demon','Fire damage':'ดาเมจธาตุ Fire','Ghost Damage':'ดาเมจธาตุ Ghost'}
s='('+'|'.join(re.escape(x) for x in sorted(stats,key=len,reverse=True))+')'
add(r'Increases (?:your )?'+s+r' by '+v+r'%\.?',lambda m:'เพิ่ม '+stats[m[1]]+' ขึ้น '+m[2]+'%','stat-increase-percent')
add(r'Reduces (?:your )?'+s+r' by '+v+r'%\.?',lambda m:'ลด '+stats[m[1]]+' ลง '+m[2]+'%','stat-reduce-percent')
add(r'Increases (?:your )?'+s+r' by '+v+r'\.?',lambda m:'เพิ่ม '+stats[m[1]]+' ขึ้น '+m[2],'stat-increase-flat')
add(r'Reduces (?:your )?'+s+r' by '+v+r'\.?',lambda m:'ลด '+stats[m[1]]+' ลง '+m[2],'stat-reduce-flat')
add(r'Increases (?:your )?'+s+r' by '+v+r'% for '+v+r' (?:sec\.|seconds\.)',lambda m:'เพิ่ม '+stats[m[1]]+' ขึ้น '+m[2]+'% เป็นเวลา '+m[3]+' วินาที','stat-increase-duration')
add(r'Reduces (?:your )?'+s+r' by '+v+r'% for '+v+r' (?:sec\.|seconds\.)',lambda m:'ลด '+stats[m[1]]+' ลง '+m[2]+'% เป็นเวลา '+m[3]+' วินาที','stat-reduce-duration')
add(r'While '+skill+r' is active, '+s+r' increases by '+v+r'\.',lambda m:'ขณะ '+name(m[1])+' ทำงาน '+stats[m[2]]+' เพิ่มขึ้น '+m[3],'active-skill-stat')
add(r'While '+skill+r' is active, gain an Extra '+v+r' PDEF and MDEF\.',lambda m:'ขณะ '+name(m[1])+' ทำงาน ได้รับ P.DEF และ M.DEF เพิ่มอีก '+m[2],'active-skill-def')
# Stat-only expressions are accepted only when every complete component matches.
def expression(en):
 pieces=re.split(r', | and |; ',en.rstrip('.'));out=[]
 for p in pieces:
  m=re.fullmatch(s+r' ([+-])('+value+r'%?)',p)
  if not m:return None
  out.append(stats[m[1]]+' '+m[2]+m[3])
 return ', '.join(out) if out else None
english={x['ID']:x['English'] for x in source};by=collections.defaultdict(set)
for k,t in existing.items():by[english[k]].add(t)
out={};audit=[]
for row in source:
 k,en=row['ID'],row['English']
 if not k.startswith(('101103','102203','108001')) or k in existing or k in blocked:continue
 thai=None;rule=None
 if len(by[en])==1:thai=next(iter(by[en]));rule='Exact-whole-English-match-to-new-batches-only'
 if thai is None:
  for rx,fn,label in rules:
   m=rx.fullmatch(en)
   if m:thai=fn(m);rule=label;break
 if thai is None:thai=expression(en);rule='Complete-whitelist-stat-expression'
 if thai is None:continue
 out[k]=thai;audit.append({'ID':k,'English':en,'method':rule})
(root/'translations/batch-016.th.json').write_text(json.dumps(out,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
(root/'docs/batch-016-template-provenance.json').write_text(json.dumps(audit,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('Appended',len(out),'fully matched skill rows. No old batch changed.')
print(dict(collections.Counter(x['method'] for x in audit)))
