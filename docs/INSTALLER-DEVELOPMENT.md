# ตัวติดตั้งรุ่นพัฒนา

## สิ่งที่ทำแล้ว

- ซอร์ส WinForms (.NET 8) ชื่อ `RO3-Thai-Patch-Installer`
- ช่องตำแหน่งเกม ปุ่มเลือกโฟลเดอร์ ปุ่มเลือก `ro3.exe` และปุ่มติดตั้ง
- ตรวจเฉพาะ `ro3.exe` ที่อยู่ตรงโฟลเดอร์ที่เลือก ไม่ค้นหา drive, registry, launcher หรือโฟลเดอร์ลูก
- ปุ่มติดตั้งเปิดได้ต่อเมื่อโฟลเดอร์ถูกต้องและมี embedded payload ที่พร้อมติดตั้ง
- แสดง progress การคัดลอกและผลตรวจ hash
- ตรวจ manifest/hash ป้องกัน traversal, duplicate entries, symlink และการเขียนทับ `ro3.exe`
- สำรองไฟล์เดิม พร้อม rollback หากคัดลอกไม่ครบ บันทึก ownership เมื่อสำเร็จ
- รุ่นพัฒนานี้หยุดหากพบ BepInEx เดิม เพื่อไม่ทับม็อดของผู้ใช้

## ผลทดสอบในเครื่องพัฒนา Linux

`dotnet build src/Installer/Installer.csproj -c Release` สำเร็จ ไม่มี warning/error

`dotnet run --project tests/Installer.Core.Tests` ผ่าน 18 checks ด้วยไฟล์จำลอง ได้แก่โฟลเดอร์ถูก/ผิด เลือก exe ไม่ค้นหาโฟลเดอร์ลูก hash ไม่ตรง payload ไม่พร้อม traversal rollback การรักษาไฟล์เดิม และป้องกันม็อดเดิม

นี่ไม่ใช่การทดสอบ UI บน Windows หรือการทดสอบเกม และยังไม่มี runtime payload ที่ผ่านการอนุมัติให้ติดตั้งจริง

## สิ่งที่ยังต้องทำก่อนแจก EXE

1. แปลและตรวจคำอธิบายที่เหลือ รวมรายการ semantic-review
2. ทำ runtime ใหม่ที่ไม่บรรจุคำแปลเก่าหรือ online translation fallback ต้องใช้ game-managed dependencies เพื่อ compile/test plugin
3. ออกแบบ ownership และ merge/update สำหรับ BepInEx เดิม โดยไม่ลบม็อดเดิม
4. ระบบ update/repair/uninstall ยังไม่ได้ทำในซอร์สรุ่นนี้
5. ทดสอบ UI/install/uninstall/rollback บน Windows กับเกมจริง
6. สร้าง `payload.zip` พร้อม manifest ของ runtime และคำแปลที่ตรวจแล้ว จากนั้นจึง publish single-file Windows EXE

การ build ปัจจุบันตรวจ compile เท่านั้น ห้ามถือเป็นตัวติดตั้งพร้อมใช้ เมื่อไม่มี payload ปุ่มติดตั้งจะไม่เปิดทำงาน
