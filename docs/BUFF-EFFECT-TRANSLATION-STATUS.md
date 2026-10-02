# Buff and effect label translation — batches 112–115

Fresh translation from verified current English only. No legacy Thai. No DLL/EXE/Release changes.

| Batch | New authored IDs |
|---|---:|
| 112 | 293 |
| 113 | 289 |
| 114 | 320 |
| 115 | 50 |

**952 new authored IDs**, 952 containing Thai and 0 canonical-only stat labels. Overall **24,974 authored IDs**. All **33,513 runtime IDs** have an English base; **24,961** targets differ from English and **8,552** remain English.

## Why this further review

The previous pass retained catalog-role/shape classified names without individually translating every label. This pass individually re-reviewed **all 1,515 remaining 102202 buff/effect records**. A catalog field may contain a descriptive state/modifier/developer effect label, not only a proper name. Such ordinary labels are now localized, keeping actual named skills, items and monsters English within surrounding Thai. 563 assigned true names/protected-only formats/internal keys/ambiguities remain English with per-ID reasons in the coverage JSON.

Scope includes damage increase/reduction, damage taken, physical/magic/element stat modifiers, shields, immunity/control states, timing/trigger/counter/detection labels, combat conditions and skill-specific effect qualifiers. Pure real skill names such as Blessing, Recovery and Increase AGI do not inherit a Thai translation from an unrelated UI homonym. Protected bracket content remains English; no filler prefixes are added to force a pure proper-name string to pass Thai checks. Underscore/resource identifiers remain unchanged.

Every placeholder, style/tag order, number, math/percent marker and wire control is preserved. Generic ATK/DEF/DMG remain generic. Explicit physical/magic stats use P.ATK/M.ATK/P.DEF/M.DEF/P.DMG/M.DMG. Unclear Life/Attack Life/Element Counter terms are retained literally or deferred rather than silently guessed as HP, lifesteal, lifespan or elemental-counterattack mechanics. Exact-English consistency retains the previously approved fresh Life Steal Orbit wording (วงโคจรดูดพลังชีวิต); this generic translation of life is not a substitution of an unspecified Life stat with HP. Thus not every occurrence of the ordinary English word Life is claimed to remain literal English.

Cumulative ledger `remaining-translation-review.tsv` still accounts for all 11,837 IDs un-authored at round108 start. Current dispositions: **3,298 A (authored), 3,493 R (individually reviewed and retained English), 5,046 C (catalog role/shape classification only)**. The historical round108 report retains its original snapshot counts; this report is the later update. All assigned current-round names were individually examined, but unrelated C catalogs still have not received individual semantic review.

**Not every game string is Thai.** Proper names, formats and source ambiguities remain intentionally English. Catalogs outside this scope and in-game naming/context can still be revisited.

## QA and delivery

Authoring/corruption/decoded-number/control checks passed. Zero conflicting rendered numeric/style targets. **205,070 runtime checks**, including 90 mocked language-bridge checks, **27 compiled hook checks**, and **57 updater checks** passed. **3,491 numeric rules**, with 10 unsafe adjacent-capture patterns ID-only. Runtime source/target bytes are compared to all 33,513 canonical merged rows.

Public publication is confirmed only after main HTTPS manifest/all three hashes, all 952 new ID/target pairs, retained English rows, merged/authored-table fidelity and the actual configured updater/cache/unchanged-version tests pass. These are data/runtime harness checks, not game/Windows UI tests. Real wording in context, clipping and layout remain untested.

Runtime v0.4.1 users restart online to receive the existing main feed; no new installer or Release. Offline/invalid responses retain verified cache or bundled fallback.
