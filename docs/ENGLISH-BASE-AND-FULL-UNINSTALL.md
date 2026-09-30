# English base + full BepInEx removal — Alpha

## v0.4.0-english-base-alpha.1

ใช้ข้อมูล Localization_en / zh_CN / zh_TW ที่ผู้ใช้ export จากเกมจริง ไม่รันสคริปต์ exporter และไม่ใช้คำแปลไทยเดิมจาก Localization_th

## แก้ UI อังกฤษกลับเป็นจีน

- ฐาน English ครอบคลุม 33,513 IDs รวมชื่อ เมนู ปุ่ม และข้อความที่ยังไม่แปล
- คง `translations/RO3.LocalizationOverrides.tsv` เป็นซอร์สคำแปลใหม่ 3,380 IDs; เพิ่ม `RO3.LocalizationUntranslated.tsv` 30,133 IDs และสร้าง `RO3.LocalizationMerged.tsv` ก่อนแพ็ก
- runtime มี 3,368 IDs ที่ข้อความต่างจาก English และ 30,145 IDs ที่คง English (รวม 7 overrides ที่ค่าเดิมเป็น English อยู่แล้วและ 5 IDs ที่พักตรวจความหมาย)
- จับคู่จีนตัวย่อ/ตัวเต็มกับต้นฉบับของ **ID เดียวกัน** เท่านั้น แล้วคืน English หรือคำแปลไทยที่อนุมัติ ไม่แปลข้อความจีน/ชื่อผู้เล่นแบบทั่วระบบ
- เติมแคช LanguageMain ตอนเริ่มต้นและหลังรีเซ็ตภาษาบน thread ที่เรียกเกม; ไม่เขียนแคชเกมจาก thread ดาวน์โหลด
- ไม่ทับข้อความ English/จีนใหม่ที่ไม่ตรงต้นฉบับของ ID นั้น หากยังเกิดจีนจะ log ID แบบจำกัดจำนวนโดยไม่เดาคำแปล
- ข้อความเดียวกันที่อยู่ทั้ง ID ไทยและ ID English จะไม่ถูกแทนทั่ว UI; ID hook ยังเลือกเป้าหมายแต่ละ ID ได้
- คงแท็ก sprite, style markers, placeholders, อัญประกาศ, backslash, tab และ newline/CRLF ผ่าน wire codec

ตัวอย่างข้อมูลจริง: ID 1002 Confirm / 确定, 1003 Cancel / 取消, 1008 Backpack / 背包 ต้องกลับเป็น English เมื่อยังไม่มีคำแปลไทย

ไฟล์ต้นฉบับจัดเก็บเป็น delta English 31 รายการเหนือ snapshot เดิม และคู่ข้อความจีนแบ่งส่วน พร้อมตรวจ hash English ที่ประกอบแล้วให้ตรงข้อมูล export ทุกแถว ไม่มีการแก้ไฟล์เกมหรือ LuaRecovery บนดิสก์

## การอัปเดตคำแปล

runtime schema 2 ใช้ชุดข้อมูล 3 ไฟล์ (merged TSV, rules, origins) พร้อม SHA-256 และ version directory เดียวกัน รุ่นก่อน schema 1 จะไม่รับ schema ใหม่และยังใช้ฐานเดิม จึงต้องเปลี่ยน EXE/DLL+ข้อมูลครั้งเดียว หลังจากนั้นแก้คำแปล main แล้ว workflow สร้าง feed ให้เหมือนเดิม ไม่ต้องออก Release เพื่อแก้คำแปลอย่างเดียว

แก้ `RO3.LocalizationOverrides.tsv` เพื่อเพิ่มคำแปล; ตาราง untranslated/merged เป็นไฟล์ generated อย่าแก้โดยตรง แก้ source batches จะ regenerate overrides จาก batches จึงอย่าแก้สองทางขัดกัน

ดาวน์โหลดเฉพาะข้อมูล ไม่ดาวน์โหลด DLL/EXE อัตโนมัติ แคช validated/offline ยังใช้ได้เมื่อเน็ตไม่ได้ เอา UserAgent setter ที่ runtime เกมไม่รองรับออกแล้ว ไม่คืน serialization attributes หรือ API เดิมที่ทำให้ SoftMask พัง

## ติดตั้ง / ย้ายจาก Alpha เดิม

1. ปิดเกมและ Launcher
2. เปิด EXE ใหม่ เลือก Client ที่มี ro3.exe ด้วยตัวเอง ไม่มีการสแกนหาเกม
3. ถ้ามีแพตช์/BepInEx เดิม ใช้ปุ่มถอนที่ต้องการ อ่านขอบเขตก่อนยืนยัน
4. เลือก Client อีกครั้งแล้วติดตั้งรุ่นใหม่

มี ZIP อัปเดตสำหรับผู้ใช้ที่มี BepInEx เดิม: ต้องเปลี่ยน **ทั้งสอง DLL และข้อมูล config ที่รวมฐาน English แล้ว** ไม่ใช่เฉพาะ DLL รุ่นก่อน แคช schema 1 จะถูกปฏิเสธและใช้ฐาน schema 2 ที่ติดตั้งมาแทน เก็บสำรองก่อนคัดลอกและอ่าน README ใน ZIP

## เมนูถอนสองแบบ

### ถอนแพตช์นี้

เหมือนเดิม: ถอนเฉพาะไฟล์ที่ installer เป็นเจ้าของ เก็บม็อด/ไฟล์ที่เพิ่มเอง และรักษา hash/rollback protection

### ถอน BepInEx ทั้งหมด

แยกปุ่มและมีคำเตือน Yes/No โดยค่าเริ่มต้นเป็น No ว่ารวมม็อดของผู้ใช้ด้วย ปิดเกมและ Launcher ก่อนกด

- ย้าย BepInEx ทั้งโฟลเดอร์ (ทุก plugin, config, cache, log) ไป `.ro3-bepinex-removed-…` ใน Client
- ย้าย winhttp.dll / doorstop_config.ini / .doorstop_version เฉพาะเมื่อยืนยัน provenance จาก config ที่ใช้ BepInEx.Preloader หรือ ownership ของ installer
- ย้ายฟอนต์ root ที่เป็นของแพตช์หรือ hash ตรง font ที่แพ็ก และย้าย marker ของแพตช์
- ไม่มีการลบข้อมูลถาวร สำรองเป็นการ rename ใน volume เดียวกัน มี removal.json บอกสิ่งที่ย้าย และ rollback หากขั้นตอนกลางล้มเหลว
- proxy/font root ที่ยืนยันไม่ได้จะคงไว้และแจ้งในผลลัพธ์ ไม่ลบ arbitrary root DLL เช่น dxgi.dll/version.dll ของม็อดอื่นโดยเดา
- ไม่แตะ ro3.exe, ro3_Data, ไฟล์เกมหรือการแก้ไฟล์เกมที่ม็อดอื่นเคยทำไว้ ไม่ลบ backup/snapshot เดิม ไม่ค้นหาที่อื่นนอก Client ที่ผู้ใช้เลือก
- ปฏิเสธ symlink/junction/reparse ใน tree ก่อนย้าย ป้องกันการไปแตะไฟล์นอก Client

หลังถอนทั้งหมด หากไม่มี proxy อื่นที่ไม่ทราบ provenance จะติดตั้งใหม่ใน Client เดิมได้โดยไม่ต้องลบ BepInEx leftovers เอง สำรองเก็บไว้เพื่อกู้ไฟล์ได้ ไม่ใช่ไฟล์ที่ loader โหลดใช้งาน

## สถานะการทดสอบ

มี tests จากข้อมูลเกมจริงทุก ID, Chinese/English pairing, global-text scope, mock cache reset/reopen, schema/hash/cache/offline, install/scoped uninstall/full removal/rollback/reinstall และ Windows build แต่ยังต้องให้ผู้ใช้ทดสอบหน้าจอและ log บน runtime เกมจริงอีกครั้ง ไม่ใช่ Stable หรือการรับประกันว่า UI ทุกเส้นทางผ่าน LanguageMain
