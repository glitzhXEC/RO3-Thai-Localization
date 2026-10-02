# ตัวติดตั้งรุ่นพัฒนา

## สิ่งที่ทำแล้ว

- ซอร์ส WinForms (.NET 8) ชื่อ `RO3-Thai-Patch-Installer`
- ช่องตำแหน่งเกม ปุ่มเลือกโฟลเดอร์ ปุ่มเลือก `ro3.exe` และปุ่มติดตั้ง
- ตรวจเฉพาะ `ro3.exe` ที่อยู่ตรงโฟลเดอร์ที่เลือก ไม่ค้นหา drive, registry, launcher หรือโฟลเดอร์ลูก
- ปุ่มติดตั้งเปิดได้ต่อเมื่อโฟลเดอร์ถูกต้องและมี embedded payload ที่พร้อมติดตั้ง
- แสดง progress การคัดลอกและผลตรวจ hash
- ตรวจ manifest/hash ป้องกัน traversal, duplicate entries, symlink และการเขียนทับ `ro3.exe`
- สร้าง ownership marker พร้อม version; เมื่อผู้ใช้เลือก Client จะตรวจว่ารุ่นที่ติดตั้งเก่ากว่าหรือไม่
- แยก runtime version ใน Installer/ownership marker ออกจาก translation data version ใน feed manifest/cache; เช็ค latest feed ได้โดยดาวน์โหลด manifest อย่างเดียว
- ปุ่มอัปเดตคำแปลใช้ runtime updater ที่ตรวจ SHA-256, schema, จำนวน IDs/rules และบันทึก cache แบบ atomic; ไม่ดาวน์โหลด client, EXE หรือ DLL
- plugin ยังตรวจ translation feed ตอนเปิดเกมอัตโนมัติ การเพิ่มคำแปลใน feed จึงไม่ต้องออก Installer Release ใหม่
- รองรับอัปเดตเฉพาะไฟล์ที่ marker เดิมเป็นเจ้าของ เก็บ config/cache ที่เปลี่ยนได้ และคงไฟล์ที่ไม่ใช่ของแพตช์
- ถอนเฉพาะไฟล์ที่ marker เป็นเจ้าของและลบ marker โดยไม่เก็บ snapshot หรือ backup ถาวร
- เมื่อพบ BepInEx ที่ไม่มี marker หรือปลายทางที่ไม่ใช่ของแพตช์ โปรแกรมหยุดโดยไม่เขียนทับ
- rollback ระหว่างติดตั้งใช้งาน temp ชั่วคราว ไม่ทิ้งสำเนาไว้ใน Client หลังจบ

## ผลทดสอบในเครื่องพัฒนา Linux

`dotnet build src/Installer/Installer.csproj -c Release` สำเร็จ ไม่มี warning/error

`dotnet run --project tests/Installer.Core.Tests` ผ่าน 18 checks ด้วยไฟล์จำลอง ได้แก่โฟลเดอร์ถูก/ผิด เลือก exe ไม่ค้นหาโฟลเดอร์ลูก hash ไม่ตรง payload ไม่พร้อม traversal rollback การรักษาไฟล์เดิม และป้องกันม็อดเดิม

นี่ไม่ใช่การทดสอบ UI บน Windows หรือการทดสอบเกมจริง

## สิ่งที่ยังต้องทำก่อนแจก EXE

1. แปลและตรวจคำอธิบายที่เหลือ รวมรายการ semantic-review
2. ทำ runtime ใหม่ที่ไม่บรรจุคำแปลเก่าหรือ online translation fallback ต้องใช้ game-managed dependencies เพื่อ compile/test plugin
3. ตรวจ UI/update/uninstall/rollback บน Windows กับไฟล์จำลองและเกมจริง
4. ตรวจว่าไฟล์ user/mod ที่ไม่ได้อยู่ใน ownership marker ไม่ถูกเขียนทับหรือลบ
5. สร้าง `payload.zip` พร้อม manifest ของ runtime และคำแปลที่ตรวจแล้ว จากนั้นจึง publish single-file Windows EXE

การ build compile และ core tests ไม่ยืนยันว่าข้อความแสดงผลถูกต้องในเกมจริง
