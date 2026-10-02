# Class, quest and UI translation progress — batches 104–107

Verified current English game export only. No legacy Thai imported. Three existing agents translated disjoint immutable queues; the parent translated stat labels and remaining readable UI candidates, reviewed all source conditions and performed whole-feed QA.

| Batch | Added IDs | Scope |
|---|---:|---|
| 104 | 433 | Class/build/talent descriptions, buff/profession/crafting effects, cart/headwear lore and combat labels |
| 105 | 570 | Settings/social/guild/shop/auction/pass/service menus, emotive actions and scene labels |
| 106 | 640 | Quest/achievement requirements, activity/season/story/defense messages and book/lore text |
| 107 | 524 | Combat/stat labels and remaining readable below100000 UI candidates, with explicit canonical stat abbreviations |

**2,167 new authored IDs**: 2,146 Thai-text entries and 21 canonical English stat-label adjustments. Overall authored total is **21,676**. The complete 33,513-ID runtime English-base table contains **21,663** targets differing from English and **11,850** retained English targets. This is not completion of every game string.

## Scope, names and unresolved source

Every one of the 3,171 assigned records was reviewed. 2,167 were authored; 1,004 true names, formats/protected-label-only rows or source ambiguities retain English. Full per-ID inventories and reasons are in `class-ui-translation-coverage.json`.

- Includes class/build descriptions, talent/buff/profession effects, crafting/gathering modifiers, quest/achievement goals, season/activity guidance, story/lore and guild announcements, settings/pass/social/shop headings and physical/magic/stat labels.
- Every placeholder, style/tag order, number, percent/math symbol, literal wire control and protected bracket label is preserved. Actual skill/item/monster/map/NPC/class/currency/rank/event names stay English, with per-ID spellings unchanged. Ordinary descriptive headings and UI actions are localized rather than classified indiscriminately as proper names.
- Ten previously authored labels were reviewed: Dismantle is consistently ย่อย, Passive is ติดตัว, and a revive-activation button that had borrowed the guild-activity word คึกคัก is restored to its original English Active. The two new skill-type Active labels retain English: the current DLL cannot represent different exact-English targets by context without a runtime change. No DLL/EXE change is made.
- Pure stat abbreviations are normalized to P.ATK/M.ATK/P.DEF/M.DEF/P.DMG/M.DMG/Single Target only where the supplied label establishes the meaning. Unchanged STR/AGI/HP/EXP/FPS/UID/number/sprite templates do not receive invented Thai filler.
- Combat labels preserve outgoing vs incoming, damage vs damage reduction, healing done vs healing received, element vs race vs enemy size, physical vs magic, source percent markers and Basis Points. Generic ATK/DEF remain generic; no damage type or chance is guessed. The unclear R suffix in PDMG.R remains R after normalizing P.DMG.
- Unclear Locate/Placement/Misc/Call Mio!! remain English terms inside translated surrounding prose, not silently repaired to imagined gameplay roles/actions. Pure Stunt and Get ${1} Off Summons stay held without guessing a skill type, price/discount/unit or number.
- Protected highlighted Realm Map/Fatigue text remains English inside brackets; the surrounding prose is localized without losing the original 20/1 layer counts or newline boundaries. Developer/test labels are strings to translate, never commands to execute.

## UI review inventory

55 additional below100000 UI IDs are authored. This inventory now has 6,771 authored IDs and 289 retained English candidates. High-ID UI text in the other batches is recorded separately, not added to the below100000 totals. Retained names, technical formats and ambiguous source are not missing runtime IDs.

## QA and delivery

Whole-authoring corruption/placeholder/decoded-number checks passed, with zero conflicting rendered numeric/style Thai targets. All 33,513 generated runtime English/Thai fields are checked byte-for-byte against the canonical merged table. **201,401 runtime checks**, including 90 mocked language-bridge checks, **27 compiled-plugin hook checks**, and **57 updater checks** passed. The feed has **3,369 numeric rules**; 10 unsafe adjacent numeric capture patterns remain ID-only.

Public publication is confirmed only after all 2,167 new ID/target pairs, all three hashes, canonical runtime-table fidelity and the configured main HTTPS updater/cache/unchanged-version behavior pass. These are data/runtime harness checks, not real-game tests.

Data-only main update: no new Release, DLL, EXE, exporter or installer change. Runtime v0.4.1 users restart online to receive the main feed; offline/invalid responses keep verified cache or bundled fallback. Actual Windows/game UI, clipping and in-context dialogue behavior remain untested.
