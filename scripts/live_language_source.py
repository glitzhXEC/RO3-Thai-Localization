"""Read only decoded localization data supplied by the user; never execute the exporter."""
import json,hashlib
from source_snapshot import read_source

def read_languages(root):
 folder=root/'translations/source';d=json.loads((folder/'live-English-delta.json').read_text(encoding='utf-8'))
 en={r['ID']:r['English'].replace('\\n','\n').replace('\\r','\r').replace('\\t','\t') for r in read_source(root)}
 en.update(d['rows'])
 assert len(en)==d['english_rows']
 assert hashlib.sha256(json.dumps(en,ensure_ascii=False,sort_keys=True,separators=(',',':')).encode()).hexdigest()==d['english_canonical_sha256'],'English snapshot integrity mismatch'
 origins={}
 for part in sorted(folder.glob('language-origins-part-*.json')):
  for row in json.loads(part.read_text(encoding='utf-8')):
   assert len(row)==3 and row[0] in en and row[0] not in origins and all(isinstance(v,str) for v in row)
   origins[row[0]]=row[1:]
 assert set(origins)==set(en),'Incomplete language origin mapping'
 return en,origins,d

def wire(text):
 return text.replace('\\','\\\\').replace('\t','\\t').replace('\r','\\r').replace('\n','\\n')

def unescape(text):
 # Historical translated table used explicit escaped controls, not JSON quoting.
 return text.replace('\\n','\n').replace('\\r','\r').replace('\\t','\t')

def unwire(text):
 out=[];i=0;esc={'n':'\n','r':'\r','t':'\t','\\':'\\'}
 while i<len(text):
  if text[i]=='\\' and i+1<len(text) and text[i+1] in esc:
   out.append(esc[text[i+1]]);i+=2
  else:out.append(text[i]);i+=1
 return ''.join(out)
