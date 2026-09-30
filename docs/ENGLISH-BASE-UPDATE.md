# อัปเดตผู้ใช้ BepInEx เดิม — v0.4.0-english-base-alpha.1

ZIP นี้ไม่ใช่ตัวติดตั้ง BepInEx ใหม่ สำหรับผู้ใช้ patch Alpha เดิม

1. ปิดเกมและ Launcher
2. สำรองสอง DLL เดิม และไฟล์ config ที่ ZIP นี้มีไว้ **นอก BepInEx/plugins** อย่าให้ loader โหลด DLL สำรองซ้ำ
3. คัดลอกโฟลเดอร์ BepInEx จาก ZIP ไป Client แทนที่เฉพาะไฟล์เหล่านี้:
   - plugins/RO3.ThaiLocalization.Skills.dll
   - plugins/SkillRuntime.Engine.dll
   - config/RO3.SkillTranslations.tsv
   - config/RO3.SkillRules.tsv
   - config/RO3.LanguageOrigins.tsv
   - config/RO3.TranslationCache/cache.json
4. ไม่ทับ AutoTranslatorConfig.ini หรือ config/ม็อดอื่น คง Arial จาก hotfix ก่อน ถ้ายังใช้ Tahoma ให้เปลี่ยน OverrideFont/FallbackFont เป็น Arial ตามคู่มือเดิม
5. เปิดเกม ทดสอบเปิด–ปิดหน้าเดิมหลายครั้งแล้วดู log: English-base dictionary ต้องมี 33,513 IDs; หากยังจีนจะมี ID diagnostics เพื่อไล่เพิ่ม

การคัดลอก DLL/TSV เองทำให้ hash ownership ของ installer เดิมไม่ตรง ถ้าจะถอนแบบเฉพาะแพตช์ต้องคืนไฟล์สำรองเดิมก่อน แต่ EXE รุ่นนี้มีปุ่มถอน BepInEx ทั้งหมดพร้อมสำรองทุกม็อด โดยไม่ต้องอาศัย hash ของ DLL เดิม อ่านคำเตือนให้ครบ

ยังเป็น Alpha ต้องทดสอบเกมจริง ไม่ปรับ locale เกมหรือแปลข้อความจีนทั่ว UI ไม่เปลี่ยน DLL ระบบเกม/ro3.exe/ro3_Data
