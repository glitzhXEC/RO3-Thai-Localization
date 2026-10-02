# System and gameplay translation progress — batches 100–103

Verified current English game export only. No legacy Thai file or web machine-translated draft was imported. Three existing agents translated disjoint immutable queues; parent translated sources/subjects, score/event/condition labels and additional gameplay announcements/dialogue, then performed whole-feed QA.

| Batch | Added IDs | Scope |
|---|---:|---|
| 100 | 1,179 | All remaining 106300 system/account/server/service error and status notices |
| 101 | 775 | 100105 combat/stat explanations, 111900 requirements, 105400 tutorial steps, 101401 mail/system announcements |
| 102 | 933 | 101000 quest/inventory/filter labels, 101600 market/settings screens and 109601 guide/UI headings |
| 103 | 967 | 107901 sources/shop headings, 101400 mail subjects, 120500 score/actions, 123400 event phases, 105700 requirements, all remaining readable 103200 gameplay/dialogue messages |

**3,854 new authored IDs**, bringing the total to **19,509**. All **33,513** source IDs remain in the runtime English-base table: **19,497** differ from English and **14,016** retain English. The below-100000 menu-review inventory is unchanged: these are higher-ID UI/system/gameplay families, not new below-100000 records.

## Coverage and source fidelity

Every one of the 4,134 assigned records was reviewed. 3,854 received Thai and 280 retained English as actual proper-name-only, protected-label/format-only, or ambiguous/malformed text. The per-ID inventory and reasons are in `system-translation-coverage.json`. This is not completion of every game string.

- Covers account/server failures, teleport and party/guild restrictions, market/stall/auction outcomes, player-progression requirements, tutorial actions, trade/refund/reissue mail, UI headings, combat/stat explanations, battle scoring, Bonfire/Campfire activity phases and additional Bobo/airship/Ant Hell/Professor dialogue and Prontera defense announcements.
- Every placeholder, token order, style boundary, number, percent/math sign, literal wire control and protected bracket label is retained. Physical/Magic stats use P.ATK/M.ATK/P.DEF/M.DEF/P.DMG/M.DMG where English explicitly identifies them. Generic ATK/DMG is not assigned an invented damage type.
- Per-ID names remain as written: Eddga, Wule/Wuller, Louis/Louie, Rhine, Sprazzi, Bonfire/Campfire, map/item/class/rank names. Protected labels remain English; exact whole-English matches use approved newly authored wording, not old Thai.
- Five pure stat labels are normalized to Single Target/P.ATK/M.ATK/P.DEF/M.DEF without inventing Thai filler. They are canonical-label changes, not Thai prose translations.
- Developer/test notices and activity scripts are translated as strings, not executed. Original debug fragments, unlabeled time/counts, typo-like SD and inconsistent source variants are not repaired to guessed values/actions.
- Missing requirement, not-yet-completed, denied, canceled and failed states remain distinct from completion/success. Personal vs guild rewards, gifts vs refunds, own vs enemy guilds, damage vs damage taken, kill vs death, attack/capture/defense, and pronoun ownership remain faithful to English.
- Proper monster/item/activity names, pure template/punctuation rows and genuine ambiguities stay in the runtime table with English fallback, not deleted or replaced with guessed Thai.

## QA and delivery

Whole-authoring corruption/placeholder/decoded-number checks passed, with zero rendered numeric/style target conflicts. All 33,513 generated runtime English/Thai fields are checked byte-for-byte against the canonical merged table. **198,677 runtime checks** (including 90 mocked language-bridge checks), **27 compiled-plugin hook checks**, and **57 updater checks** passed. The feed has **3,175 numeric rules**; 10 unsafe adjacent numeric capture patterns remain ID-only.

Public publication is confirmed only after all 3,854 new ID/target pairs, all three file hashes, runtime-table fidelity and the configured main HTTPS updater/cache/unchanged-version behavior pass. Local checks are not actual game tests.

This is a data-only main update: no Release, DLL, EXE, exporter script or installer change. Runtime v0.4.1 users restart online to receive the main feed; offline/invalid responses keep verified cache or bundled fallback. Real-game/Windows UI testing, contextual dialogue tone and clipping checks have not been performed.
