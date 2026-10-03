# RO3 Thai Patch — Installer และ Data-only Update

Runtime ปัจจุบันคือ `v0.4.4-minimal-runtime-alpha.1` และใช้ translation feed แบบ data-only ผู้ใช้จึงอัปเดตคำแปลได้โดยไม่ดาวน์โหลด `RO3-Thai-Patch-Installer.exe` ใหม่

## วิธีใช้

1. ปิด RO3 และ RO3AsiaLauncher
2. เปิด `RO3-Thai-Patch-Installer.exe`
3. เลือกโฟลเดอร์ `Client` ที่มี `ro3.exe` หรือเลือกไฟล์ `ro3.exe` โดยตรง
4. หาก runtime เก่า ให้กด **อัปเดตตัวแพตช์**
5. หากข้อมูลคำแปลเก่า ให้กด **อัปเดตคำแปลจาก GitHub**
6. plugin จะตรวจ translation feed อัตโนมัติเมื่อเปิดเกมขณะออนไลน์ด้วย

## เวอร์ชันที่แยกจากกัน

- **Runtime version** อยู่ใน Installer และ ownership marker เปลี่ยนเมื่อ DLL/ตัวติดตั้งหรือ schema ที่รองรับเปลี่ยน
- **Translation data version** อยู่ใน `translations/live/manifest.json` เปลี่ยนเมื่อข้อมูลคำแปลที่ผ่าน validation เปลี่ยน

translation data version ปัจจุบัน:

```text
19f0ada39734b794168cbd61b1e42265e6bdcccce4263e2b5ecbd4dc91a4c448
```

manifest ปัจจุบันมี 33,513 translation IDs และ 3,496 runtime rules บน runtime schema 2

## Runtime ที่ติดตั้ง

payload มีเฉพาะ BepInEx/Harmony, ปลั๊กอินของโครงการ, dictionaries และ updater ไม่ติดตั้ง XUnity AutoTranslator, XUnity ResourceRedirector, XUnity config หรือ font bundle ที่ไม่ใช้

ข้อความ UI ที่อนุมัติใช้ exact match ส่วนคำอธิบายสกิลที่มีตัวเลข/ตัวแปรใช้ prefix-indexed rules เพื่อลดการสแกนข้อความที่ไม่เกี่ยวข้อง

## Ownership และการถอน

Installer อ่าน `.ro3-thai-localization.json` ใน Client ที่ผู้ใช้เลือกและแตะเฉพาะไฟล์ที่แพตช์เป็นเจ้าของ หากพบ BepInEx ที่ไม่มี ownership marker จะหยุดโดยไม่เขียนทับ การถอนจะลบเฉพาะไฟล์ของแพตช์และคืนไฟล์เดิมตาม ownership record เมื่อจำเป็น

## ผลทดสอบล่าสุด

ชุดทดสอบอัตโนมัติผ่านทั้งหมด:

- validation และ menu checks
- LanguageHooks compiled-plugin checks: 27/27
- language bridge/fallback checks: 91/91
- SkillRuntime checks: 205,174/205,174
- updater checks: 58/58
- Installer core checks: 43/43
- build ไม่มี error

`SHA256SUMS.txt` และ hash ใน manifest ใช้ตรวจความสมบูรณ์ของไฟล์ที่ดาวน์โหลด
