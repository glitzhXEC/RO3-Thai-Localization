# Quest translation progress — batches 096–099

The verified current English game export is the only source. No old Thai translation was used. Three existing agents received disjoint immutable queues and wrote drafts only; the parent translated descriptive titles, reviewed source conditions, integrated exact-English wording and performed whole-feed QA.

| Batch | Added IDs | Scope |
|---|---:|---|
| 096 | 599 | Quest objectives: remaining 131500 plus remaining 131506 stage trackers and Relax in 131502 |
| 097 | 682 | NPC dialogue, choices, services and quest storyline: first remaining 103700 half |
| 098 | 680 | NPC dialogue, choices, services and quest storyline: second remaining 103700 half |
| 099 | 628 | Descriptive quest titles, stage headings, job progression, party/guild activity labels (131501) |

**2,589 new authored IDs**, bringing the total to **15,655**. The complete 33,513-ID runtime table contains 15,643 targets that differ from English and 17,870 retained English targets. The below-100000 menu inventory is unchanged. This is not completion of every game string.

## Covered and retained text

All 2,632 assigned records were reviewed: 2,589 translated and 43 retained English. The queue covered all remaining readable NPC dialogue in the 103700 family and all remaining quest trackers in 131500/131506, plus descriptive 131501 quest titles. Pure format-only NPC rows were not assigned. Detailed remaining IDs and reasons are recorded in `quest-translation-coverage.json`.

- NPC conversations and dialogue choices include shops/services, novice/job advance, Eden Group, the Professor, airship incidents, Ant Hell/Culvert investigations, cargo recovery and later story scenes.
- Descriptive quest titles are translated; actual NPC, monster, map, item, class and activity names remain English. Same-English text uses approved freshly authored wording, never legacy Thai.
- Two ambiguous Utility clauses remain English (13150000187/13150000218); English does not identify a resource/item/support action. Tower Trial and Rift Stone* are retained names, not missing IDs.
- Protected bracket contents remain English even when the content is highlighted prose. All placeholders, style boundaries, numbers, math symbols, literal controls, quoted text and original name spellings are retained. Source variant boss/NPC names are not silently unified.
- No unit is invented for map-clear placeholders. Developer labels and debug notices are translated as game text, not executed. The trailing asterisks in objectives remain literal.

## QA and delivery

Whole-authoring corruption/placeholder/decoded-number checks passed. Rendered numeric/style variants have zero conflicting Thai targets. All 33,513 generated English/Thai runtime rows are checked byte-for-byte against the canonical merged table. Local results: **194,191 runtime checks** including 90 mocked language-bridge checks, **27 compiled-plugin hook checks**, and **57 updater checks**. The feed contains **2,943 numeric rules**. Seven unsafe adjacent numeric patterns remain ID-only.

Public verification checks all 2,589 new ID/target pairs, all three file hashes, the runtime table, verified cache and unchanged-version behavior through actual HTTPS. Publication is only confirmed after these checks; these are data/runtime harness tests, not real-game tests.

This is a data-only main update: no new Release, DLL, EXE, script or installer behavior. Runtime v0.4.1 receives the main feed when restarting online; offline/invalid downloads keep verified cache or bundled fallback. Windows UI/game layout, dialogue tone in context and scene behavior have not been tested in the actual game.
