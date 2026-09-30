# Windows x64 UI Preview — NOT an installable Thai patch

> **รุ่นนี้ทดสอบหน้าจอเท่านั้น ยังติดตั้งแพตช์ภาษาไทยหรือ BepInEx ไม่ได้**
> ไม่มี runtime/payload พร้อมใช้ใน EXE ปุ่มติดตั้งจึงยังไม่เปิดทำงาน

## Downloads

- `RO3-Thai-Patch-Installer.exe`: Windows x64 single-file, self-contained .NET UI preview
- `RO3-Thai-Patch-Windows-x64-Preview.zip`: EXE + คำแนะนำ
- `RO3-Thai-Localization-Source-Preview.zip`: ซอร์สและคำแปลใหม่ 691 รายการ
- `SHA256SUMS.txt`: hashes สำหรับตรวจไฟล์ดาวน์โหลด

## ทดสอบอะไรได้

เลือกโฟลเดอร์ `Client` ที่มี `ro3.exe` หรือเลือกไฟล์โดยตรง และตรวจข้อความเมื่อเลือกโฟลเดอร์ถูก/ผิด ไม่มีการค้นหาเกมในเครื่อง ไม่มี BAT หรือ Translation-Editor และรุ่น preview ไม่ติดตั้งไฟล์ลงเกม

## สถานะการแปล

691 รายการใหม่จาก English เท่านั้น ไม่ใช้คำแปลเก่า ตรวจ placeholder/style marker/ตัวเลข/สูตรแล้ว คิวที่พบยังเหลือ 2,726 รายการ และยังมี semantic review ที่ต้องทำ

## ข้อจำกัด

- ยังไม่มี BepInEx/runtime payload ใหม่ที่พร้อมใช้งาน
- Update/repair/uninstall ยังไม่พร้อม
- การทดสอบแกนติดตั้ง 18 checks ใช้ไฟล์จำลอง ไม่ใช่เกมจริง
- GitHub Actions build บน Windows ไม่เท่ากับทดสอบ GUI หรือเกม
- EXE ยังไม่มี code signing อย่าปิดระบบป้องกันเครื่องเพียงเพื่อทดลอง
- ไม่มีการแก้หรือ bypass anti-cheat

นี่เป็น prerelease ของ UI ไม่ใช่ Release ของแพตช์พร้อมใช้งาน
