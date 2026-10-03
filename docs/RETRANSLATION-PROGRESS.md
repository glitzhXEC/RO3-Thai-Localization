# สถานะงานแปลใหม่

## สถานะล่าสุด

- ฐานข้อความ English: 33,513 IDs
- คำแปลไทยใน runtime: 25,003 IDs
- รายการคง English: 8,510 IDs
- กฎข้อความแบบไดนามิก: 3,496 rules
- Runtime schema: 2
- translation feed version: `6bb226188cd63e5d7243515496528fd206f1e49b7f06c37e77d0d96af95124ff`

งานล่าสุดครอบคลุมชื่อและคำอธิบายเควสต์ที่ยังเหลือ ข้อความยาว ต้นฉบับรูปแบบผิดปกติที่ตรวจได้ และการปรับคำศัพท์ค่าสถานะให้เป็นมาตรฐาน

## กฎที่ใช้

- ใช้เฉพาะ English เป็นแหล่งข้อความและสัญลักษณ์
- คงชื่อแผนที่ ดันเจี้ยน เมือง โซน มอนสเตอร์ บอส NPC และสัตว์เลี้ยงเป็น English
- ใช้ `P.ATK`, `M.ATK`, `P.DEF`, `M.DEF`, `ASPD`, `MSPD`, `CRIT` และ `FLEE`
- ใช้ `ออปชั่นพิเศษ` สำหรับ `Stunt`/`Stunts` ทุกจุด รวมป้ายข้อความสั้น
- รักษา placeholder, style marker, ตัวเลข, สูตร, escaped newline และ protected name
- ไม่เปลี่ยนข้อความภายในวงเล็บที่ runtime schema 2 ใช้ตรวจเป็น protected name

## ผลทดสอบ

ชุดทดสอบอัตโนมัติผ่านทั้งหมด:

- validation และ menu checks
- LanguageHooks compiled-plugin checks: 27/27
- language bridge/fallback checks: 91/91
- SkillRuntime checks: 205,138/205,138
- updater checks: 57/57
- Installer core checks: 43/43
- build ไม่มี error

หลังซอร์สผ่าน validation แล้ว GitHub Actions จะสร้าง `translations/live/` และเผยแพร่ data-only feed ให้ Installer/runtime อัปเดตโดยไม่ต้องออก EXE ใหม่

## ขั้นตอนทำงานต่อ

1. แปลจากรายการ English ที่ยังคงเหลือ โดยไม่อ้างอิงคำแปลเก่า
2. ตรวจบริบทของชื่อเฉพาะและข้อความกำกวมก่อนอนุมัติ
3. รันชุดทดสอบทั้งหมดก่อน push
4. ตรวจ manifest, hash, จำนวน IDs/rules และผล workflow หลังเผยแพร่
