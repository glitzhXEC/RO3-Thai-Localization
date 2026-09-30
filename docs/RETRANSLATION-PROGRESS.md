# สถานะงานแปลใหม่

แปลใหม่รวม 691 รหัสจาก English เท่านั้น ไม่ใช้คำแปลเก่า แบ่งไฟล์ batch-001 ถึง batch-008

- คิวคำอธิบายที่ยืนยันตระกูลได้: 3417 รายการ
- คงเหลือในคิว: 2726 รายการ
- คิวครอบคลุมสกิล เอฟเฟกต์ เสริมสกิล ไอเทม อาหารและยา แต่ยังไม่ใช่หลักฐานว่าครอบคลุมคำอธิบายทุกตระกูล
- ผ่านการตรวจลำดับ placeholder/style marker, ตัวเลข, เครื่องหมายสูตร, ชื่อในวงเล็บ และ escaped newline
- มี 44 แจ้งเตือนคำแปลที่เป็นค่าสถานะอังกฤษล้วน 43 รายการ เช่น STR/ASPD และชื่อเฉพาะอังกฤษ 1 รายการ (Tree Spirit/Respawn) โดยตั้งใจ ไม่ใช่ข้อความอธิบายภาษาอังกฤษที่ยังไม่ได้แปล
- ข้อความกำกวมแยกไว้ใน translations/semantic-review.json ต้องตรวจในเกมก่อนอนุมัติ
- ไม่รวม BAT, Translation-Editor, คำแปลเก่า, plugin binaries เก่า หรือ auto-translation fallback
- src/Installer compile ผ่าน; แกนตัวติดตั้งผ่าน 18 checks ด้วยไฟล์จำลอง ไม่ใช่ Windows/game testing
- ยังไม่มี runtime/payload พร้อมใช้ ยังไม่ได้ push GitHub หรือ Release
- ไม่ใช้เอเจนต์ย่อย ตามที่ผู้ใช้ระบุ

## ลำดับงานต่อ

1. ใช้ translations/source/pending-queue.tsv แปลชุดถัดไปจาก English เท่านั้น โดยไม่เปิดคำแปลเดิม
2. เพิ่ม batch-009.th.json และตรวจด้วย scripts/validate_translations.py
3. ทำ semantic review ทุกชุด ตรวจหมวดเพิ่มเติม และแก้ข้อความกำกวม
4. ทำ runtime ใหม่และทดสอบกับ game-managed dependencies
5. ทำ update/repair/uninstall และทดสอบ Windows/เกมจริง ก่อน push และ publish

## ตรวจซ้ำ

```sh
python scripts/validate_translations.py
python scripts/test_validation.py
dotnet run --project tests/Installer.Core.Tests
dotnet build src/Installer/Installer.csproj -c Release
```

provenance.json เก็บ commit/hash ของต้นฉบับ 33,512 แถวที่มีสองคอลัมน์ แถว 53001 รูปแบบผิดปกติถูกแยกออกโดยยังไม่ได้เดาความหมาย
