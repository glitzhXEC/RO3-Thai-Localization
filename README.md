# RO3 Thai Localization

แพตช์ภาษาไทยสำหรับ **RO3 Asia PC** แปลจากต้นฉบับ English เท่านั้น ใช้ BepInEx/Harmony และตัวติดตั้ง Windows แบบไฟล์เดียว โดยไม่ใช้ BAT, Translation-Editor หรือบริการแปลอัตโนมัติ

## ดาวน์โหลดและอัปเดต

- Runtime/Installer ปัจจุบัน: [`v0.4.4-minimal-runtime-alpha.1`](https://github.com/glitzhXEC/RO3-Thai-Localization/releases/tag/v0.4.4-minimal-runtime-alpha.1)
- โปรแกรม: `RO3-Thai-Patch-Installer.exe`
- คู่มือ: [Installer และระบบอัปเดต](docs/INSTALLER-UPDATE-RELEASE.md)

ตัว Installer แยก **รุ่น runtime** ออกจาก **รุ่นข้อมูลคำแปล** ผู้ใช้จึงอัปเดตคำแปลจาก GitHub ได้โดยไม่ต้องดาวน์โหลด EXE ใหม่ และ plugin จะตรวจ feed อัตโนมัติเมื่อเปิดเกมขณะออนไลน์

## สถานะล่าสุด

| รายการ | สถานะ |
| --- | --- |
| ฐานข้อความ English | 33,513 IDs |
| คำแปลไทยใน runtime | 25,021 IDs |
| รายการคง English | 8,492 IDs |
| กฎข้อความแบบไดนามิก | 3,496 rules |
| Runtime schema | 2 |
| Data-only update | พร้อมใช้งาน |
| ชุดทดสอบอัตโนมัติ | ผ่านทั้งหมด |

รุ่นข้อมูลคำแปลล่าสุด:

```text
19f0ada39734b794168cbd61b1e42265e6bdcccce4263e2b5ecbd4dc91a4c448
```

ดูข้อมูลที่เผยแพร่จริงได้ที่ [`translations/live/manifest.json`](translations/live/manifest.json)

## สิ่งที่อัปเดตล่าสุด

- แปลชื่อและคำอธิบายเควสต์ที่ยังเหลือ รวมถึงข้อความยาวและต้นฉบับรูปแบบผิดปกติที่ตรวจได้อย่างปลอดภัย
- นำคำอธิบายสกิลหลักที่เคยพัก semantic review ไว้เข้าสู่ runtime ครบแล้ว
- คงชื่อแผนที่ ดันเจี้ยน เมือง โซน มอนสเตอร์ บอส NPC และสัตว์เลี้ยงเป็น English เช่น `Southern Payon` และ `Ant Hell`
- ทำป้ายและคำศัพท์ค่าสถานะให้เป็นมาตรฐานทั้งหมด: `P.ATK`, `M.ATK`, `P.DEF`, `M.DEF`, `P.DMG`, `M.DMG`, `P.PEN`, `M.PEN`, `ASPD`, `MSPD`, `CRIT` และ `FLEE`
- ใช้ `ออปชั่นพิเศษ` สำหรับ `Stunt`/`Stunts` ทุกจุด รวมป้ายข้อความสั้น
- รักษาข้อความในวงเล็บที่ runtime schema 2 ใช้เป็น protected name เพื่อให้ใช้ร่วมกับ Installer เดิมได้
- เผยแพร่ translation feed แบบ data-only หลัง validation สำเร็จ

## ขอบเขตคำแปล

แปลข้อความที่อธิบาย:

- เควสต์ สกิล เอฟเฟกต์ บัฟ และดีบัฟ
- ไอเทม อาหาร ยา สูตร ระยะเวลา เงื่อนไข และกลไกการต่อสู้
- ข้อความ UI ที่อนุมัติและผูกกับต้นฉบับได้แน่นอน

คงเป็น English:

- ชื่อสกิล ไอเทม อาวุธ คลาส และชื่อเฉพาะที่ผู้เล่นใช้ค้นหา
- ชื่อแผนที่ ดันเจี้ยน เมือง โซน มอนสเตอร์ บอส NPC และสัตว์เลี้ยง
- ชื่อสถานะหรือกลไกเฉพาะ เช่น `Endure`, `Control Immunity` และ `Adaptive Damage`

## หลักการรักษาข้อมูลเกม

คำแปลต้องรักษาต้นฉบับต่อไปนี้ครบถ้วน:

- placeholder เช่น `${1}`, `@{1}` และ `{1}`
- style marker เช่น `^{1}` และ `^{2}`
- ตัวเลข ตัวดำเนินการ `*`, `+`, `%` และลำดับสูตร
- escaped newline เช่น `\n`
- protected name ภายใน `【...】` หรือ `[...]`

ห้ามนำคำแปลเก่าในคอลัมน์ Thai มาอ้างอิง เว้นแต่ระบุว่าเป็นข้อความที่ตรวจแล้ว

## วิธีใช้ Installer

1. ปิด RO3 และ RO3AsiaLauncher
2. เปิด `RO3-Thai-Patch-Installer.exe`
3. เลือกโฟลเดอร์ `Client` ที่มี `ro3.exe` หรือเลือก `ro3.exe` โดยตรง
4. เลือกติดตั้ง อัปเดตตัวแพตช์ อัปเดตคำแปล ซ่อมแซม หรือถอนการติดตั้ง
5. รอให้โปรแกรมตรวจ schema/hash และทำงานจนเสร็จ

Installer ไม่สแกนหาเกมทั้งเครื่อง ไม่เขียนทับ BepInEx ที่ไม่มี ownership marker และถอนเฉพาะไฟล์ที่แพตช์เป็นเจ้าของ

## ผลทดสอบอัตโนมัติล่าสุด

- validation และ menu checks: ผ่าน
- LanguageHooks compiled-plugin checks: 27/27
- language bridge/fallback checks: 91/91
- SkillRuntime checks: 205,174/205,174
- updater checks: 58/58
- Installer core checks: 43/43
- build: ผ่าน ไม่มี error

## สำหรับนักพัฒนา

ไฟล์หลัก:

- [`translations/RO3.LocalizationOverrides.tsv`](translations/RO3.LocalizationOverrides.tsv) — คำแปลที่อนุมัติ
- [`translations/RO3.LocalizationUntranslated.tsv`](translations/RO3.LocalizationUntranslated.tsv) — รายการที่คง English/ยังไม่แปล
- [`translations/RO3.LocalizationMerged.tsv`](translations/RO3.LocalizationMerged.tsv) — ตารางรวม generated
- [`translations/live/`](translations/live/) — feed ที่ Installer และ runtime ใช้งาน

แนวทางแก้ไข:

1. ใช้ข้อความ English เป็นแหล่งเดียว
2. เพิ่มคำแปลใน batch/source ที่กำหนด ห้ามแก้ไฟล์ generated โดยตรง
3. ตรวจ placeholder, style marker, สูตร, protected name และอภิธานศัพท์
4. รัน validation, runtime, bridge, plugin และ updater tests ให้ผ่านทั้งหมด
5. push ซอร์ส แล้วให้ workflow สร้างและเผยแพร่ data-only feed

## รายงานปัญหา

กรุณาแนบข้อความ English ต้นฉบับ คำแปลที่แสดง ภาพหน้าจอ ขั้นตอนที่ทำให้เกิดปัญหา และ log ของ Installer หากเกี่ยวข้องกับการติดตั้ง

## ข้อจำกัดความรับผิดชอบ

โปรเจกต์นี้เป็นผลงานชุมชนและไม่ใช่แพตช์อย่างเป็นทางการของผู้พัฒนาเกม ชื่อเกม เครื่องหมายการค้า และทรัพย์สินที่เกี่ยวข้องเป็นของเจ้าของลิขสิทธิ์แต่ละราย
