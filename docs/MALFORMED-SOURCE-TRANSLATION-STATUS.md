# Malformed source translation — batch 119

แปลข้อความที่เหลืออีก **2 IDs** โดยไม่เดาหรือซ่อมข้อมูลต้นฉบับ

- `Auto Guard Increases Attack Life` → คง `Attack Life` เป็น English เพราะไม่ทราบว่าหมายถึง HP, อายุเอฟเฟกต์ หรือการดูดพลังชีวิต
- คำอธิบาย Dynamite/Poisoned → แปลข้อความรอบกลไก แต่คง raw malformed bracket `【^{3}^{4}【Poisoned】` ตรงต้นฉบับทุกตัว

Validator รองรับต้นฉบับ nested bracket ที่เสียเฉพาะเมื่อ raw malformed segment ยังตรงเดิม และมี regression test ที่ยืนยันว่าการเปลี่ยน `Poisoned` เป็นชื่ออื่นต้องถูกปฏิเสธ ไม่ได้ผ่อนกฎ bracket สำหรับข้อความปกติ
