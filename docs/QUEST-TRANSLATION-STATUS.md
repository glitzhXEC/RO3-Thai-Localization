# Quest translation progress — batches 092–095

The verified current English game export is the only translation source. No old Thai dialogue is copied. Three existing translation agents received immutable, disjoint queues and wrote drafts only; the parent performed integration, terminology/source-condition review and complete runtime/data QA.

| Batch | Added IDs | Scope |
|---|---:|---|
| 092 | 434 | Quest descriptions: beginner trials, collection/delivery/combat tasks, investigations, rescue/travel stories, job advance and progression help (131502) |
| 093 | 446 | Quest objectives and stage tracking: combat, gear/cards/pets, map/activity progress, storyline actions and progression requirements (131500) |
| 094 | 350 | NPC conversations, dialogue choices, job-advance advice, Eden Group/airship/Professor storylines and delivery/rescue dialogue (103700) |
| 095 | 252 | Encounter goals and activity/reward requirements: boss/arena actions, daily tasks, season records, party/guild conditions and progression counters (110810/111901) |

1,482 new authored IDs, bringing the overall authored count to 13,066. All 33,513 source IDs remain present in the English-base runtime table: 13,054 differ from English and 20,459 retain English. The below-100000 UI inventory is separate and unchanged. This is not a complete translation of every quest or game text.

## Source fidelity

NPC, monster, item, map, class and activity names are retained where they are names. Per-ID English differences such as Rhina/Rhine/Rhyne, Rein, Lain/Ryan/Levin, Poya/Baoya/Boya/Marika, Ki/Ji/Ji'ang, Granny Gwen/Grandma Gwen, Ilmata/Ilmatar, Vocals/Rockers, and Nail of World Law/Nail of Cosmic Law are not silently unified.

All placeholders, order, styling boundaries, numbers, math symbols, percentages, literal controls and protected bracket content remain faithful to English. Bracketed prose such as [poor girl], [the device next to me] and the free-Flying-Mount warning remains English because the current runtime treats bracket contents as protected names. Developer notices are translated as game strings, not followed as instructions.

- Quest item/kill counts retain source values; unlabeled obtain/deliver/kill fields do not gain guessed item/count units.
- Repeated `${1}` in quality/count/level requirements (11190100148–11190100154) is retained rather than corrected to `${2}`.
- The unitless time/progress fields remain unitless. Literal XXX, Monster A, and the trailing `*` after Alchemy Materials remain as written.
- The unusual Orc Warrior@{1} adjacency is retained. Source-inconsistent item labels Snake Galls/Snake Gallbladders and event/map names are not guessed into one value.
- Positive actions and unmet-condition notices remain distinct: participation vs clear/win, at least vs more than, team vs individual kills, within-time vs until-time, and not-yet-met relationship conditions.
- Identical English uses the approved fresh Thai wording. 169 targets were canonicalized for this consistency. Thai spellings for item, season and stats were aligned without changing source meaning.

## Deferred assigned entries

Eight assigned entries retain their original English:

| ID | Reason |
|---|---|
| 13150200065 | “Return to camp for Utility” does not establish the intended help/resource/item |
| 13150200213 | Tower Trial: pure named label |
| 13150000158 | The Hunter Who Retrieves Mounts: pure named NPC/title label |
| 13150000187 | Same ambiguous return-to-camp Utility clause |
| 13150000218 | “Go provide Utility” does not identify what to supply |
| 13150000350 | Tower Trial: pure named label |
| 11081000007 | “Faction @{1} Players” does not establish faction identifier vs player count |
| 11190100050 | Rift Stone: item name only |

These are not missing source IDs. They remain in the runtime table with English fallback. More English context is needed for the ambiguous clauses; proper-name-only entries are intentionally retained.

## QA and delivery

Authoring/corruption/decoded-number checks passed. The runtime table is verified byte-for-byte against the canonical merged source across all 33,513 IDs. A preflight found zero conflicts in rendered English/Thai numeric/style variants. Local test results: 191,417 runtime checks including 90 mocked language-bridge checks, 27 compiled-plugin hook checks, and 57 updater checks. The complete feed has 2,856 numeric rules. Public verification checks all 1,482 new targets, all three file hashes, raw runtime-table fidelity, and actual HTTPS update/cache/unchanged-version behavior.

This is a data-only main-feed update, not a new Release. No DLL/EXE, game script or installer behavior changes. Runtime v0.4.1 users restart with internet access to receive it; offline/invalid downloads retain the verified cache or bundled fallback. No real-game or Windows UI testing has been performed; clipping, dialogue tone in context and scene behavior still need in-game review.
