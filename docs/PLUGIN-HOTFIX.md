# Plugin-only hotfix v0.3.1-ui-hotfix-alpha.1

สำหรับผู้ที่ติดตั้ง v0.3.0-auto-update-alpha.1 อยู่แล้ว ไม่ใช่ตัวติดตั้ง BepInEx ใหม่

1. ปิดเกมและ Launcher
2. สำรอง `Client/BepInEx/plugins/RO3.ThaiLocalization.Skills.dll` และ `SkillRuntime.Engine.dll` ไปเก็บนอก BepInEx/plugins (อย่าทิ้ง DLL สำรองไว้ให้ loader โหลดซ้ำ)
3. แตก ZIP แล้วคัดลอกสอง DLL ใน `BepInEx/plugins` ไปตำแหน่งเดียวกันใน Client เพื่อแทนที่สองไฟล์เดิม ไม่แก้ ro3.exe หรือ DLL ระบบเกม
4. ใน `BepInEx/config/AutoTranslatorConfig.ini` แก้เฉพาะ `OverrideFont=Arial` และ `FallbackFont=Arial` เพื่อกลับไปใช้ค่า repo เดิม ไม่ต้องลบหรือทับ config ทั้งไฟล์
5. เปิดเกมแล้วตรวจ LogOutput.log: ต้องไม่เกิด DataContractAttribute/DataContractJsonSerializer/ReadWriteTimeout errors จาก plugin นี้ ทดสอบหน้าจอ UI และสกิลแล้วส่งภาพ/log ใหม่หากยังผิดปกติ

อัปเดต DLL เองจะทำให้ hash ownership ของ installer เดิมไม่ตรง หากต้องถอนติดตั้งด้วย installer ให้ปิดเกมแล้วคืน DLL สองไฟล์ที่สำรองไว้ก่อนถอน เวอร์ชัน installer ใหม่ไม่ติดตั้งทับม็อดเดิม

ฐานออฟไลน์และแคช schema 1 เดิมยังใช้ได้ คำแปลไม่ถูกแทนด้วยของเก่า แพตช์ยังใช้ BepInEx และยังเป็น Alpha ที่ต้องทดสอบในเกมจริง
