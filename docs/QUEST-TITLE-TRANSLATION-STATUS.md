# Quest title translation — batch 116

แปลชื่อเควสต์ที่ไม่ใช่ชื่อแผนที่/สถานที่เพิ่ม **28 IDs** จากคอลัมน์ English ปัจจุบันเท่านั้น ไม่ใช้คำแปลไทยเก่า

## ขอบเขต

- ชื่อเควสต์ 25 รายการในตระกูล `131501`
- ชื่อกิจกรรมเควสต์ที่ซ้ำกัน 3 รายการใน `131500`, `131502` และ `131504`
- คงชื่อคลาส เมือง และคำเกมที่ต้องค้นหาได้ เช่น `Novice`, `Swordsman` และ `Holy See` ไว้ในชื่อไทย
- รักษา placeholder และ style marker ทุกตัว เช่น `@{1}`, `^{1}` และ `^{2}`

## กฎชื่อแผนที่และมอนสเตอร์

ชื่อแผนที่ ดันเจี้ยน เมือง โซน สถานที่ มอนสเตอร์ บอส NPC และสัตว์เลี้ยงต้องคง English ตรงต้นฉบับ ห้ามแปลหรือทับศัพท์ ถ้าทั้งแถวเป็นชื่อ ให้คงทั้งแถวพร้อม marker เดิม

หลังตรวจครบทั้งตาราง ได้คืน 13 IDs เป็น English ได้แก่ `Temple of Melody`, `Above the Sea of Clouds`, `Courtyard of Chaos`, `Boneyard`, `Ant Hell`, `Garden of Time`, `Oak Inn` (2 IDs), `Phantom Realm`, `Academy Sanctuary`, `Southern Payon`, `Deep Forest` และ `Training Ground`

## ยังไม่แปล

- ข้อความ `Utility` ที่ไม่ระบุว่าเป็นไอเทม ทรัพยากร หรือการช่วยเหลือ
- `Rift Stone*` ซึ่งเป็นชื่อไอเทมพร้อมเครื่องหมายต้นฉบับ
- `@{1}` ซึ่งเป็น placeholder ล้วน
- ป้ายประเภทเควสต์ในวงเล็บ เช่น `[Main Quest]` เนื่องจาก validator กำหนดให้ protected bracket คงเดิม
