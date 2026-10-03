# Remaining prose translation — batch 118

แปลข้อความอธิบายยาวและข้อความแจ้งเตือนที่ยังเป็น English เพิ่ม **8 IDs** จาก English ปัจจุบันเท่านั้น

## ขอบเขต

- จดหมายแจ้ง Trade ผิดปกติและกฎปลดระงับ Zeny
- กฎการได้รับ Guild Prestige
- คำอธิบายสกิล Cart Revolution/Money Throw และ Bobo
- ข้อความระบบ/HP ที่ต้นฉบับใช้คำกำกวม `Misc`, `Description` และ `Reply`

## การรักษาต้นฉบับ

- คงชื่อเฉพาะทั้งหมด เช่น `Bobo`, `Cart Revolution`, `Money Throw`, `Guild Territory War`, `Stronghold War`, `Escort Battle`, `Regional Capital War` และ `Royal City War`
- คงคำกำกวม `Defeats`, `Utility`, `Reply`, `Misc` และ `high-multiplier Description` เป็น English ภายในประโยคไทยแทนการเดาความหมาย
- คง placeholder, style marker, ตัวเลข, สูตร, เครื่องหมาย และ escaped newline ครบทุกตัว
- คง syntax ต้นฉบับที่เสีย `${3-second` โดยไม่ซ่อมหรือเติมค่าที่หาย

รายการที่ยังไม่แปลต่อเป็นชื่อเฉพาะ รูปแบบเทคนิค internal key หรือข้อความที่โครงสร้างวงเล็บ/สูตรเสียจนไม่สามารถสร้างผลลัพธ์ที่ validator ยอมรับได้โดยไม่แก้ต้นฉบับ
