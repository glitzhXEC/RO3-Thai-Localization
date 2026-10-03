# FLEE and Stunt terminology correction

ปรับคำศัพท์ตาม glossary ล่าสุดทั่วทุก batch:

- `Flee` / `Dodge` → `FLEE`: **16 IDs**
- `Stunt` → `ออปชั่นพิเศษ`: **13 IDs**

คงมาตรฐาน `P.ATK`, `M.ATK`, `P.DEF`, `M.DEF`, `ASPD`, `MSPD` และ `CRIT` ในข้อความทั่วไป ส่วนข้อความใน protected brackets ต้องคงตรง English เพื่อให้ runtime schema 2 ของ Installer ยอมรับ data-only feed

Validator ปฏิเสธรูปเก่านอก protected brackets และคำ `Stunt` ที่หลงเหลือใน target พร้อม regression tests ป้องกันการย้อนกลับ
