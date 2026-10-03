# Quest category labels — batch 120

แปลป้ายประเภทเควสต์ที่เป็นคำอธิบายทั่วไปเพิ่ม **11 IDs** โดยรักษา style marker และวงเล็บเดิม

- `[Instance]` → `[ ดันเจี้ยน ]`
- `[Horse Racing]` → `[ แข่งม้า ]`
- `[Commission]` → `[ ภารกิจว่าจ้าง ]`
- `[Main Quest]` → `[ เควสต์หลัก ]`
- `[Job Advance]` → `[ เปลี่ยนอาชีพ ]`
- `[Conquest]` → `[ พิชิต ]`
- `[Side Quest]` → `[ เควสต์รอง ]`
- `[Hidden]` → `[ ซ่อนเร้น ]`
- `[Guide]` → `[ คู่มือ ]`
- `[Event]` → `[ อีเวนต์ ]`
- `[Faction]` → `[ ฝ่าย ]`

คง `[Adventure Group]` และ `[War of Emperium]` เป็น English เพราะเป็นชื่อเฉพาะ ไม่ใช่คำอธิบายหมวดทั่วไป

Validator อนุญาตเฉพาะรายการและคำแปลที่กำหนดไว้ข้างต้น ไม่ได้เปิดให้แปลชื่ออื่นใน protected brackets และมี regression test ป้องกันคำแปลเปลี่ยนโดยไม่ตั้งใจ
