# RO3 Thai Localization — Auto-update Alpha

รุ่น `v0.3.1-ui-hotfix-alpha.1` สำหรับ Windows x64 / Unity Mono — **ยังไม่ทดสอบ GUI หรือเกมจริง ไม่ใช่ Stable**

## Hotfix จาก log เกมจริง

แก้ dependency/attributes ของตัวอัปเดตที่ทำให้การสแกน types ของ SoftMask ล้มเหลว และเลิกเรียก ReadWriteTimeout ที่ runtime ไม่มี คืนฟอนต์ Arial ให้เหมือน repo เดิม ไม่เปลี่ยนคำแปลเก่าเข้ามาแทน [ผลตรวจ ID และหลักฐาน](UI-COMPATIBILITY-HOTFIX.md)

ผู้ใช้ v0.3.0-auto-update-alpha.1 สามารถใช้ `RO3-Thai-Plugin-UI-Hotfix.zip` เพื่อเปลี่ยนสอง DLL เท่านั้น โดยไม่ต้องถอน BepInEx อ่าน [คู่มือและผลต่อ ownership](PLUGIN-HOTFIX.md) ก่อนทำ รุ่นนี้ยังต้องทดสอบ UI ในเกมจริงอีกครั้ง

## ดาวน์โหลดและติดตั้ง

ดาวน์โหลด `RO3-Thai-Patch-Installer.exe` จาก Release นี้ หรือ ZIP ที่มี EXE พร้อมคู่มือ แล้วตรวจ SHA256SUMS.txt
ปิดเกมและ Launcher เปิด EXE เลือกโฟลเดอร์ `Client` ที่มี `ro3.exe` หรือเลือกไฟล์ `ro3.exe` โดยตรง จากนั้นกดติดตั้ง ไม่มี BAT ไม่มี Translation-Editor ไม่มีการสแกนเครื่องหรือหาเกมอัตโนมัติ ยังใช้ BepInEx เหมือนเดิม

**ถ้าใช้ Alpha เดิม:** ปิดเกม เปิด EXE แล้วเลือก Client โปรแกรมแสดง runtime version และ translation data version แยกกัน runtime เก่าสามารถกด **อัปเดตตัวแพตช์**; translation data เก่ากด **อัปเดตคำแปลจาก GitHub** ได้โดยไม่โหลด Installer/Client ใหม่ หรือกด **ถอนแพตช์นี้** โปรแกรมไม่ค้นหา Client อัตโนมัติ และคงไฟล์/ม็อดที่ไม่มี ownership marker ของแพตช์ไว้

## คำแปลอัปเดตอย่างไร

- มีฐานออฟไลน์ 3,375 IDs / 1,848 rules (สกิล 2,414 IDs รวมไอเทมและอาหาร)
- ตอนเริ่มเกม ใช้แคชที่ตรวจแล้วทันที จากนั้นตรวจข้อมูลใหม่จาก GitHub `main` หนึ่งครั้งใน background
- แหล่งต้นทางคือ `translations/RO3.LocalizationOverrides.tsv` บน main; GitHub Actions สร้าง TSV + rules เป็นชุด versioned ใน `translations/live` ก่อนเผยแพร่
- แก้คำแปลใน main แล้วรอ workflow Translation data feed สำเร็จ plugin จะเช็ค manifest ที่ GitHub ตอนเปิดเกมและรับข้อมูลรุ่นใหม่อัตโนมัติ ไม่ต้องดาวน์โหลด Client/Installer หรือสร้าง Release ทุกครั้ง; Installer รุ่นใหม่มีปุ่มเช็ค/อัปเดต data ล่วงหน้าได้
- เปลี่ยนผ่าน `batch-*.th.json` จะใช้ workflow Refresh translation tables ที่ตรวจและสร้างทั้งตารางและ feed; อย่าแก้ batch และ TSV ต่างเวอร์ชันพร้อมกัน เพราะ batch workflow สร้าง TSV ใหม่จาก batches
- ข้อความที่แสดงเป็นไทยไปแล้วอาจต้องเปิดหน้าสกิลใหม่หรือเริ่มเกมใหม่ ไม่ได้ย้อนแปลทุกหน้าจอที่เปิดอยู่
- เน็ตล่ม แฮชผิด ไฟล์ไม่ครบ schema ใหม่ หรือเขียนแคชไม่ได้: ใช้ข้อมูลเดิมต่อ หากแคชเสีย ใช้ฐานที่ติดตั้งมา
- ดาวน์โหลดเฉพาะข้อมูลคำแปล ไม่ดาวน์โหลดหรือรัน DLL/EXE/ZIP ไม่ส่งข้อมูลเกมหรือผู้เล่น

ระบบตรวจ HTTPS repo ที่กำหนดตายตัว, SHA-256, ขนาด, schema, จำนวนแถว, IDs, placeholders, สูตร และชื่อสถานะ; ใช้ชุดข้อมูล content-versioned เพื่อไม่ปะปนไฟล์คนละเวอร์ชัน SHA-256 ตรวจความสอดคล้อง/ความสมบูรณ์ **ไม่ใช่ลายเซ็นผู้เผยแพร่** ความเชื่อถืออยู่ที่ repo นี้และ HTTPS

เวอร์ชัน runtime ของ Installer/ownership marker แยกจากเวอร์ชันคำแปลใน manifest/cache (`RO3.TranslationCache/cache.json`). การอัปเดต translations แตะเฉพาะ data cache ไม่แก้ DLL หรือ TSV ฐานที่ installer เป็นเจ้าของ และไม่ต้องดาวน์โหลด installer ใหม่เมื่อเพิ่มคำแปล การอัปเดต runtime คงแคช/config ที่ผู้ใช้แก้ไว้; การถอนลบเฉพาะไฟล์ใน ownership marker และไม่สร้าง snapshot/backup คงเหลือ

หลังเปิดเกมครั้งแรก ปิดการตรวจออนไลน์ได้ที่ `BepInEx/config/com.ro3.thailocalization.skills.cfg` ตั้ง `[Translations] AutoUpdateOnStartup = false` แล้วเริ่มเกมใหม่

## ขอบเขตและข้อจำกัด

ตารางซอร์สมี 3,380 IDs; runtime เว้น 5 IDs ที่ต้องตรวจความหมายเพิ่มเติม และ 37 ต้นฉบับที่ค้างตรวจยังไม่รวม ไม่ได้แปลครบทุกข้อความของเกม ทั้งหมดแปลใหม่จาก English ไม่ใช้คำแปลไทยเดิม รองรับเฉพาะตระกูล ID ที่กำหนด และไม่ฝืน ID ที่ English เปลี่ยนไป

CI ทดสอบ exact ID/English, styled numeric rules, online/offline ด้วย fake network, ไฟล์เสีย/แฮชผิด/พาธอันตราย, แคช atomic และ install/uninstall ด้วยไฟล์จำลอง พร้อม build Windows EXE แต่ไม่ยืนยันการแสดงผลหรือความเข้ากันได้ในเกมจริง

ต้องออก Release ใหม่เมื่อแก้ plugin, installer, BepInEx หรือ schema/ฟีเจอร์ ไม่จำเป็นเมื่อเปลี่ยนคำแปลใน schema ที่รองรับเดิม
