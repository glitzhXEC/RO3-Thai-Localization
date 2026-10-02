# RO3 Thai Skills — Partial Alpha 0.2.0

> เอกสารนี้บันทึก payload รุ่นเก่า 0.2.0 ซึ่งเคยรวม XUnity ไว้; payload minimal รุ่นใหม่ถอด AutoTranslator/ResourceRedirector ออกตาม dependency analysis ใน `docs/INSTALLER-UPDATE-RELEASE.md`.

> **รุ่นนี้ติดตั้งไฟล์แพตช์สกิลบางส่วนได้แล้ว แต่ยังไม่ผ่านการทดสอบเกมจริง**
> ไม่ใช่ UI Preview เดิม และยังไม่ใช่ Stable Release

## Downloads

- `RO3-Thai-Patch-Windows-x64-Skills-Alpha.zip` — EXE + คำแนะนำ (แนะนำ)
- `RO3-Thai-Patch-Installer.exe` — single-file self-contained Windows x64
- `RO3-Thai-Localization-Source-Skills-Alpha.zip` — ซอร์สและคำแปล
- `SHA256SUMS.txt` — ตรวจไฟล์ดาวน์โหลด

## สิ่งที่บรรจุ

- BepInEx/XUnity compatibility runtime และฟอนต์ (สถานะของ release เก่า 0.2.0 เท่านั้น)
- Plugin/engine สกิลที่เขียนใหม่ ไม่มี plugin localization เก่า
- คำแปลใหม่ 416 รหัสของสกิล/เอฟเฟกต์/เสริมสกิล ใช้ English เท่านั้น
- ไม่มีคำแปลเก่า ไม่มีการแปลออนไลน์ ไม่มี BAT หรือ Translation-Editor
- เพิ่มคำแปลสกิล 70 รายการในรอบนี้ คำแปลในซอร์สรวม 761 รายการ

## เงื่อนไขสำคัญ

**รองรับ Client ที่ยังไม่มี BepInEx/winhttp.dll เดิมเท่านั้น และต้องเป็น Windows x64 Unity Mono** หากมีแพตช์หรือม็อดเก่า โปรแกรมจะหยุด ไม่ทับ/ลบโดยเดา

ปิดเกม/Launcher เปิด EXE เลือก Client ที่มี ro3.exe หรือเลือก ro3.exe โดยตรง ตรวจตำแหน่งที่เลือกเท่านั้น ไม่มีการค้นหาเกมอัตโนมัติ จากนั้นติดตั้งพร้อม Progress

เปิด Launcher ตามปกติและใช้ภาษาเกม English ข้อความที่ยังไม่ตรงรายการจะคงเดิม สามารถใช้ปุ่มถอนแพตช์นี้เพื่อลบเฉพาะไฟล์ที่แพตช์เป็นเจ้าของ โดยเก็บ snapshot และไม่ลบม็อดที่เพิ่มภายหลัง

## ผลตรวจและข้อจำกัด

1,636 runtime checks และ 34 installation/recovery checks ผ่านด้วยไฟล์/ค่าจำลอง รวมการติดตั้ง payload ทั้งชุด Compile สำเร็จ แต่ **ยังไม่ยืนยันการเปิดเกม การ hook และ glyph/การแสดงผลไทยในเกมจริง** โดยเฉพาะ UI ที่ใช้ MeshUI

Update/repair/ติดตั้งทับ BepInEx เดิมยังไม่พร้อม สูตร/ข้อความที่กำกวมถูกกันออกจาก payload

EXE ไม่มี code signing ไม่ควรปิดระบบป้องกันเครื่องเพื่อทดลอง ไม่มีการแก้หรือ bypass anti-cheat และต้องพิจารณาข้อกำหนดของเกม

หากคำแปลไม่ขึ้น กรุณาแนบภาพสกิล English ต้นฉบับ และ LogOutput.log โดยปิดบังข้อมูลส่วนตัวก่อน
