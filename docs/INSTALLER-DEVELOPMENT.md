# ตัวติดตั้งและ Runtime

## สถานะปัจจุบัน

- โปรแกรม WinForms .NET 8: `RO3-Thai-Patch-Installer.exe`
- Runtime รุ่นเป้าหมาย: `v0.4.4-minimal-runtime-alpha.1`
- รองรับ Windows x64 และ RO3 Unity Mono x64
- ใช้ BepInEx/Harmony และปลั๊กอินของโครงการ
- ไม่ใช้ XUnity AutoTranslator, ResourceRedirector, BAT หรือ online machine translation
- แยกรุ่น runtime ออกจากรุ่นข้อมูลคำแปล
- อัปเดต translation feed จาก GitHub ได้โดยไม่ดาวน์โหลด Installer ใหม่

## ความปลอดภัยของ Installer

- ตรวจเฉพาะ Client/`ro3.exe` ที่ผู้ใช้เลือก ไม่สแกนทั้งเครื่อง
- ตรวจ manifest, schema และ SHA-256 ก่อนเขียนไฟล์
- ป้องกัน traversal, duplicate entry, symlink และการเขียนทับ `ro3.exe`
- หยุดเมื่อพบ BepInEx ที่ไม่มี ownership marker
- อัปเดตและถอนเฉพาะไฟล์ที่ marker ระบุว่าแพตช์เป็นเจ้าของ
- รักษาไฟล์ม็อด/config ที่ไม่ใช่ของแพตช์
- เขียน cache แบบ atomic และ rollback ด้วยไฟล์ชั่วคราวเมื่อเกิดข้อผิดพลาด

## ผลทดสอบล่าสุด

ชุดทดสอบอัตโนมัติผ่านทั้งหมด:

- build สำเร็จ ไม่มี error
- validation และ menu checks ผ่าน
- LanguageHooks compiled-plugin checks: 27/27
- language bridge/fallback checks: 91/91
- SkillRuntime checks: 205,138/205,138
- updater checks: 57/57
- Installer core checks: 43/43

## ระบบอัปเดตคำแปล

`translations/live/manifest.json` ใช้ runtime schema 2 และ version แบบ SHA-256 ของข้อมูลที่เผยแพร่ ตัว Installer สามารถอัปเดต cache ล่วงหน้า ส่วน plugin ตรวจ feed เมื่อเปิดเกมขณะออนไลน์

การเพิ่มหรือแก้คำแปลที่ยังอยู่ใน schema เดิมเป็น data-only update จึงไม่ต้องออก Installer/Client Release ใหม่
