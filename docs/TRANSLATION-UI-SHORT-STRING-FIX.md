# แก้คำจีนสั้น `取下` ที่ยังแสดงในเกม

## อาการและสาเหตุ

คำแปล `取下` → `ถอด` มีอยู่แล้วในตารางภาษา แต่บาง UI ส่งคำสั้นนี้ตรงเข้า text setter โดยไม่มี localization ID จึงไม่เข้าเส้นทางแปลที่อาศัย ID และตาราง English-base ส่วน fallback ของข้อความทั่วไปตั้งใจไม่แปลป้ายสั้น เพื่อป้องกันการแทนชื่อหรือคำที่บังเอิญตรงกัน

ใน `Client/BepInEx/LogOutput.log` พบว่า plugin `RO3 Thai Localization + Translation Updates 0.4.1` โหลดตาราง 33,513 IDs และติดตั้ง hook ของตนได้ ส่วน XUnity มี hook บางตัวใช้กับ Unity/runtime นี้ไม่ได้ เช่น `UnityEngine.TextMesh.set_text` และ `GameObject.SetActive` ปัญหาของ `取下` มาจากข้อความสั้นที่ไม่มี ID ไม่ใช่เพราะไม่มีคำแปล และไม่เกี่ยวกับ error ของ PhysX/MetaSDK ที่อยู่ใน log เดียวกัน

## วิธีแก้

runtime สร้างรายการ fallback จากต้นฉบับภาษาจีน/ไต้หวันที่ผูกกับ ID ซึ่งมีคำแปลไทยอนุมัติแล้ว โดยแปลเฉพาะเมื่อข้อความทั้งสตริงตรงกับต้นฉบับ และทุก ID ที่ใช้ต้นฉบับเดียวกันให้คำแปลไทยเหมือนกัน หากคำแปลขัดกัน runtime จะข้ามข้อความนั้น ไม่แปล CJK ที่ไม่อยู่ในรายการ และไม่ใช้การค้นหา/แปลบางส่วน

จึงรองรับป้าย `取下` → `ถอด` ได้โดยไม่เปิดการแปลจีนทั่วระบบ การเปลี่ยนอยู่ใน `src/SkillRuntime.Engine/SkillDictionary.cs` และติดตั้งผ่าน DLL ของ `RO3.ThaiLocalization.Skills` กับ `SkillRuntime.Engine` รุ่น minimal runtime ยังจำกัด fallback ของ text setter ให้ค้นเฉพาะข้อความที่อนุมัติแบบตรงทั้งประโยค เพื่อไม่ให้แชตไปสแกน numeric regex rules ทั้งชุด

## การตรวจสอบ

- Build `src/SkillRuntime/SkillRuntime.csproj` สำเร็จสำหรับ .NET Framework 4.7.2
- ติดตั้ง DLL ที่ build แล้วใน `Client/BepInEx/plugins` หลังปิดเกม
- ต้องเปิดเกมใหม่และตรวจหน้าจอที่เคยแสดง `取下` เพื่อยืนยันผลจริง

การตรวจจาก log และการ build ไม่ยืนยันผลบนหน้าจอเกม รุ่นนี้ยังเป็น Alpha
