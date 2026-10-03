# Stat terminology normalization

ตรวจคำแปลทุก batch และปรับรูปค่าสถานะให้เป็นมาตรฐานเดียวกัน

| รูปเดิม | รูปมาตรฐาน | IDs ที่แก้ |
|---|---|---:|
| `Physical ATK` | `P.ATK` | 11 |
| `FLEE` | `Flee` | 2 |
| `Crit` | `CRIT` | 1 |

ไม่พบ `PATK`, `MATK`, `PDEF`, `MDEF`, `Magic ATK`, `Physical DEF`, `Magic DEF`, `Attack Speed`, `Movement Speed`, `Critical Rate` หรือ `Dodge` ใน target ที่ตรวจแล้ว รูป `M.ATK`, `P.DEF`, `M.DEF`, `ASPD` และ `MSPD` ถูกต้องอยู่แล้ว

Validator ปฏิเสธรูปเก่าและรูปเต็มที่ไม่เป็นมาตรฐาน พร้อม regression tests สำหรับ `Physical ATK` และ `FLEE` เพื่อป้องกันการย้อนกลับ
