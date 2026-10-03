# Canonical combat-stat labels

ป้ายและข้อความเป้าหมายใช้รูปมาตรฐานต่อไปนี้ทุกจุดนอก protected brackets:

- `PATK` / `Physical ATK` → `P.ATK`
- `MATK` / `Magic ATK` → `M.ATK`
- `PDEF` / `Physical DEF` → `P.DEF`
- `MDEF` / `Magic DEF` → `M.DEF`
- `PDMG` / `Physical Damage` → `P.DMG`
- `MDMG` / `Magic Damage` → `M.DMG`
- `PPEN` / `Physical Penetration` → `P.PEN`
- `MPEN` / `Magic Penetration` → `M.PEN`
- `Flee` / `Dodge` → `FLEE`

Validator ตรวจทุก batch และปฏิเสธรูปเดิมที่หลงเหลือนอกข้อความในวงเล็บซึ่ง runtime schema 2 ปกป้องไว้ ผลตรวจล่าสุดไม่พบ legacy stat target นอก protected brackets
