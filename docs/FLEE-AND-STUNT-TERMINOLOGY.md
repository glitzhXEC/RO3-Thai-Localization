# FLEE and Stunt terminology correction

ปรับคำศัพท์ตาม glossary ล่าสุดทั่วทุก batch:

- `Flee` / `Dodge` → `FLEE`: **16 IDs**
- `Stunt` → `ออปชั่นพิเศษ`: **13 IDs**

คงมาตรฐานเดิมสำหรับ `P.ATK`, `M.ATK`, `P.DEF`, `M.DEF`, `ASPD`, `MSPD` และ `CRIT`

Validator ปฏิเสธ `Flee`, `Dodge`, `Crit`, รูป ATK/DEF แบบเก่า และคำ `Stunt` ที่หลงเหลือใน target พร้อม regression tests ป้องกันการย้อนกลับ
