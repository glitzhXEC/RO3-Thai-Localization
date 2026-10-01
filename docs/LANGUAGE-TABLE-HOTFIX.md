# v0.4.1 — Language Table Hotfix Alpha

แก้เส้นทางอ่านภาษาเพิ่มเติมเพื่อป้องกันข้อความกลับเป็นจีน โดยยังใช้ BepInEx เหมือนเดิม รุ่นนี้ยังไม่ผ่านการทดสอบกับเกมจริง

## สิ่งที่ตรวจพบ

- v0.4.0 ดัก `LanguageMain.Obf_gO` และรีเซ็ต `Obf_Pk` แต่ไม่มี hook สำหรับการอ่านตาราง Lua ภาษาโดยตรง
- ตรวจข้อมูล feed แล้ว: English base ครบ 33,513 IDs ไม่มีข้อความภาษาจีนปนในต้นฉบับ English หรือ target ยกเว้นสัญลักษณ์ `皿` ในอีโมติคอน `(╯▔皿▔)╯` ของ ID 18019 ซึ่งต้องคงไว้
- Log ที่ผู้ใช้ส่งก่อนหน้านี้ระบุ v0.3.1 และการโหลด `Localization_zh_CN` แต่ยังไม่มี LogOutput.log ล่าสุดที่ยืนยัน ID/เส้นทางของอาการบน v0.4.0 จึงยังไม่ถือว่าพิสูจน์สาเหตุของทุกอาการในเครื่องผู้ใช้แล้ว

## วิธีแก้

- เพิ่ม hook ของ Lua manager ที่โหลดโมดูลภาษา (`Obf_MD`), เริ่ม Lua (`Obf_iD`), คืน environment (`Obf_jD`) และ game-thread tick (`Obf_o.Obf_ie`)
- แก้เฉพาะ `Localization_en`, `Localization_zh_CN`, `Localization_zh_TW` รวมถึงตารางที่อยู่ใน global/package.loaded และ wrapper ที่มีความลึกจำกัด
- จีนที่ตรงกับต้นฉบับเดิมของ ID ใช้คำไทยที่อนุมัติแล้ว หรือ English หากยังไม่มีคำไทย
- จีนที่ไม่ตรงกับต้นฉบับเดิม แต่เป็น ID ที่มี English และยังมี placeholder/แท็ก/ตัวเลข/บรรทัดตรงกัน จะคืน English ที่เชื่อถือได้ก่อน ไม่เดาความหมายจีนเป็นไทย
- รักษา English ใหม่ที่ไม่ตรงกับ snapshot, ID ที่ไม่รู้จัก, ข้อความที่รูปแบบเปลี่ยน และคำไทยที่ไม่ได้เป็น target ของแพตช์ไว้ พร้อมรายงาน IDs ที่ต้องตรวจต้นฉบับ
- รองรับ ID keys แบบ string, long และ double โดยไม่เพิ่ม ID ที่ไม่มีใน Lua table ไม่เขียน Recovery/game data ลงดิสก์ และไม่เรียก Lua script/require เพิ่ม
- การเขียนตาราง Lua ทำบน game thread เท่านั้น การดาวน์โหลด feed ไม่เขียน Lua จาก worker thread และไม่สแกนหรือแปลข้อความผู้เล่น/แชตเป็นรายคำ

## คำแปลเพิ่มเติม

ชุด 056 เพิ่ม 232 IDs ของเมนูการ์ด ฉายา รายละเอียดไอเทม และปฏิทินอีเวนต์ แปลจาก English ไม่ใช้จีนเป็นต้นฉบับ ไม่ลบคำแปลไทยที่ทำไว้ก่อนหน้า

## อัปเดตแพตช์

รุ่นนี้เปลี่ยน runtime DLL จึงต้องอัปเดตครั้งเดียว: การดาวน์โหลด TSV เพียงอย่างเดียวไม่เพิ่ม hook ใหม่ให้ v0.4.0

**วิธีแนะนำ:** ปิดเกมและตัวเปิดเกม แล้วใช้ `RO3-Thai-Patch-Installer.exe` ของ Release v0.4.1 เลือก Client ที่มี ro3.exe ตามเดิมและติดตั้งแพตช์

**อัปเดตเฉพาะ plugin:** ปิดเกมก่อน แตก `RO3-Thai-English-Base-Update.zip` ของ Release v0.4.1 ลงใน Client เดิมที่ติดตั้ง BepInEx และแพตช์นี้ไว้แล้ว โดยรวมโฟลเดอร์ BepInEx และแทนที่เฉพาะไฟล์ที่อยู่ใน ZIP ห้ามลบ BepInEx หรือม็อดอื่นเพื่ออัปเดต เก็บสำเนาไฟล์เดิมก่อนหากต้องการย้อนกลับ

หลังจากนี้คำแปลที่เพิ่มยังอัปเดตผ่าน data feed บน main โดยไม่ต้องออก EXE ใหม่ทุกครั้ง ไม่มี BAT ไม่มีการค้นหาเกมอัตโนมัติ และเมนูถอน BepInEx ทั้งหมดยังทำงานแบบสำรองก่อนตามเดิม

## การทดสอบและข้อจำกัด

- ตรวจ placeholder/แท็ก/ตัวเลข/บรรทัด, English base ทุก ID, runtime dictionaries/rules, offline/update cache และสร้าง plugin net472
- จำลองตาราง Lua ทั้ง 3 ภาษาและรูปแบบ key, เปิด/รีโหลดซ้ำ, reset cache, data swap, Chinese variant → English, unknown ID/new English/unrelated Thai/chat preservation
- Callback harness ใช้โค้ด plugin เดียวกับรุ่นจริง แต่จำลอง Harmony/Unity APIs: ตรวจ registration, callback, main-thread guard และ diagnostic logs ไม่ใช่การทดสอบ IL patches หรือเกมจริง
- ยังไม่ยืนยันว่าจีนหายทุกจุด: ID ที่ไม่อยู่ใน export, รูปแบบเปลี่ยน, Lua API เปลี่ยน หรือข้อความจาก prefab/เส้นทางอื่นต้องใช้ log/ภาพจากเกมจริงเพื่อตรวจต่อ
- ยังไม่ได้แปลไทยครบทุกระบบ ชื่อเฉพาะ/คำเทคนิคและข้อความกำกวมบางส่วนคง English

## ช่วยตรวจหลังติดตั้ง

เปิดเมนูที่เคยกลับเป็นจีนหลายครั้งแล้วส่ง `BepInEx/LogOutput.log` พร้อมภาพเมนูที่ยังเป็นจีนถ้ามี

ควรพบ `Loading ... 0.4.1`, `Language table Localization_zh_CN ...`, `Language table Localization_zh_TW ...` เมื่อเกมโหลดโมดูลนั้น และ `Language IDs requiring source review`/`Unmatched Chinese lookup ... ID=` หากมี ID ที่ยังต้องตรวจ ไม่มีการบันทึกข้อความผู้เล่นเป็นตัวอย่าง
