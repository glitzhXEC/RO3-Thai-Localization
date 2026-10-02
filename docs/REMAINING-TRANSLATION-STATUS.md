# Remaining translation review — batches 108–111

Verified current English export only; no old Thai imported. Data-only update, no DLL/EXE/new Release.

| Batch | New authored IDs |
|---|---:|
| 108 | 675 |
| 109 | 721 |
| 110 | 730 |
| 111 | 220 |

**2,346 new authored IDs**, including 2,346 Thai-text targets and 0 canonical English stat labels. Overall **24,022 authored IDs**. All **33,513 runtime IDs** retain the correct English base: **24,009** targets differ from English and **9,504** remain English.

## Scope and completion boundary

At round start, 11,837 IDs were not authored. Three existing agents and the parent reviewed 5,276 disjoint candidate records. 2,346 were authored; 2,930 genuine names, technical/protected-only formats or malformed/ambiguous English sources remain English with per-ID reasons. Another 6,561 skill/item/buff/NPC/monster/internal-name catalog records were classified by catalog role and structural shape and intentionally remain English; **this is not individual semantic review of every catalog name**. Sentence-shaped outliers were checked and moved into translation queues where meaningful prose was present.

The TSV review ledger accounts for every round-start un-authored ID exactly once. Disposition A = authored; R = individually reviewed and retained English; C = catalog role/shape classified and retained English. Reason codes resolve through `remaining-translation-review-reasons.json`; original English remains in the verified source and runtime English-base table. `remaining-translation-coverage.json` records totals and source concerns.

Readable menus, escort/supply alerts, survey/account notifications, ritual/music exploration guidance, battlefield labels, quests/tutorials and short-ID aliases are localized. Ordinary descriptive headings are not automatically treated as names. Actual skill/item/NPC/map/class/currency/rank/event names and protected bracket contents remain English under the translation convention. Source numbers, placeholders, style/tag order, percentages, arithmetic and wire controls are preserved.

**Not a claim that every game text is Thai or that every possible localizable catalog label is resolved.** Ambiguous/incomplete source and names/formats remain; catalog role classification can be revisited with game UI context. English fallback is intentional, not a missing runtime ID. Active and Log context conflicts/regressions remain English where required by the unchanged runtime. Existing semantic-review holds are unchanged.

## QA and distribution

Authoring and decoded-number/control validation passed. Zero conflicting rendered numeric/style Thai targets. **204,104 runtime checks**, including 90 mocked language-bridge checks, **27 compiled hook checks**, and **57 updater checks** passed. 3,484 numeric rules; the 10 unsafe adjacent-capture patterns remain ID-only. All runtime source/target bytes are compared against the canonical merged table.

Publication is confirmed only after public main manifest/all three file hashes, all 2,346 new ID/target pairs, retained English rows, byte-faithful 33,513-ID merged table and the actual configured HTTPS updater/cache/unchanged-version checks pass. These tests are data/runtime harness tests; no real game or Windows UI was executed. Wording in context, clipping and layout still require game verification.

Runtime v0.4.1 users restart online to receive the existing main feed. No new installer or Release is required; offline/invalid responses keep verified cache or bundled fallback.
