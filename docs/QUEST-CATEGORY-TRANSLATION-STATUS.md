# Quest category labels — runtime compatibility hold

ป้ายประเภทเควสต์ใน protected brackets เช่น `[Main Quest]` และ `[Side Quest]` ยังคง English เพราะ runtime schema 2 ใน `RO3-Thai-Patch-Installer.exe` ตรวจ bracket content เป็นชื่อที่ต้องตรงต้นฉบับ การแปลป้ายเหล่านี้ทำให้ data-only feed ถูกปฏิเสธด้วย `Status names changed`

การแปลรอบ batch 120 จึงถูกถอดออกจาก runtime feed จนกว่าจะมี Installer/runtime รุ่นใหม่ที่รองรับ localized bracket labels โดยชัดเจน ชื่อและข้อความนอก protected brackets ยังแปลตามปกติ
