# Gameplay translation progress — batches 074–115

The verified current English game export is the only source. Existing fresh terminology is reused where the English text and meaning match; old Thai translation files are not imported.

| Batch | Added IDs | Scope |
|---|---:|---|
| 074 | 60 | Equipment forging/appraisal/refine/enhance, wardrobe/shop, guild funds/supplies/tactics/management, trade, gifts and mounts |
| 075 | 54 | Tavern/relationships, MVP reward rules, AFK/field drops, expedition rewards, Soul Echo/Totem/card guides, Codex and Survivor Battle help |
| 076 | 27 | Remaining readable skill/item descriptions and literal test/designer notices |
| 077 | 166 | Boss/dragon mechanics, siege and territory warnings, map-object status, killstreaks and combat instructions |
| 078 | 54 | Airship encounters, grappling/cannon instructions, boss ground-warning order, Dracula/bride rescue and spawn alerts |

14,878 new authored IDs across batches 074–115, including 952 in the latest 112–115 update; 24,974 authored IDs overall. Of the complete 33,513-ID runtime table, 24,961 IDs differ from English and 8,552 retain English. Overall values exclude no source IDs; semantic-review holds and explicitly retained unchanged targets remain English.

## Source fidelity and unresolved text

- Preserve placeholders, tag order, all numbers, percentages, formula symbols, bracketed names and escaped controls. Generic Physical/Magic combat stats use canonical P.ATK, P.DMG, P.DEF and M.DEF as appropriate. Single Target and stacks conventions remain unchanged.
- Live-English guide IDs 10960000071–10960000073 retain their original CR/newline boundaries. Current quotation text for 10320000172 comes from the live export rather than the older authoring snapshot.
- Literal A/x values, `*specified*`/`*value description*` draft fields, the source multiplication sign after blade count (10110300975), and `% sec` in 10220300699 are not replaced with guessed gameplay values or repaired formulas. Designer/test notices are translated as data, not executed as instructions.
- Named skills, monsters, items, objects and protected bracket content remain English. Some warning prefixes remain English because the current runtime protects bracket content as names.
- Remaining malformed or ambiguous source examples retain English: 10110300508 (incomplete ATK factor), 10110301061 (unclosed/nested status brackets), 10800100036 (malformed `${3-second` token), 10220300020 (Physical/Physical DEF ambiguity), and 10220300381/10220300404 (`Reply` HP wording). Existing semantic-review holds are unchanged. New English export/context is needed before changing these meanings.
- In guild/object alerts, the guild-name and object-name placeholders keep their original order and ownership. Directional, safe-zone, shielding and rescue instructions follow English; enemy/player/monster names are not globally replaced.

## Validation and delivery

This is a text/data-only update using runtime schema 2. No DLL, EXE, game script or new Release is changed. Runtime v0.4.1 can receive these translations through the existing main feed on startup. Restart with internet access; an offline or invalid response keeps the verified cache or bundled fallback.

The numeric-rule test renderer now uses the existing single-pass wire decoder for CR, LF, tabs and escaped backslashes; a regression check rejects double-decoding literal backslash-n. No production/runtime code was changed for this test correction.

Authoring QA and decoded-control number QA, corruption rejection, complete English-base audit, runtime rules, language-hook harness and updater tests must pass before publication. Public feed verification checks all three file hashes and all 952 latest ID/target pairs through the actual HTTPS updater. The generated runtime English/Thai fields are also checked against the canonical merged table for all 33,513 IDs. These are automated data/mock-runtime checks, not Windows/game UI tests.

**This is not a completed translation of every game text.** The remaining current-export review is recorded in REMAINING-TRANSLATION-STATUS.md; proper names, technical formats and source ambiguities are intentionally retained English. In-game wording, layout and clipping have not been verified.

## Quoted TSV field correction

The actual HTTPS smoke test exposed a generator parsing error for 10320000172: ordinary CSV quoting removed its enclosing literal quotation marks, so the source English no longer matched the actual game string. The generator now reads raw runtime TSV with `csv.QUOTE_NONE`, preserving literal quotes rather than treating them as CSV field delimiters. This repairs 26 runtime rows (including English-base-only rows) without changing translation IDs, source text, DLLs or executables. A regression compares every generated English/Thai field byte-for-byte with the canonical merged TSV before publication. Source snapshot TSV parts retain their existing proper CSV reader; only the raw generated TSV reader was corrected.

## Update — batches 079–087

| Batch | Added IDs | Scope |
|---|---:|---|
| 079 | 54 | Survivor Battle revival/ranks, guild dungeon reward counts, siege attack/defense, note puzzles and character presets |
| 080 | 28 | Stalls, pet hatching/EXP/stars/slots/skills, dismantling, synthesis, skill casting order and party roles |
| 081 | 33 | Phantom Realm, Hidden Land, raid reward counts/auctions, Final Trial and detailed airship boss mechanics |
| 082 | 30 | Season rewards, guild league scoring/roles, caravans, trade history and Guild Black Market |
| 083 | 47 | Auto bidding, treasure hunts, Territory War phases, crafting/gathering, cooking and music-event progress |
| 084 | 47 | Map hunt counters, rift rewards, talent/class advance, Dragon Coffin Island and casting/target/chase settings |
| 085 | 60 | Boss mechanics: Ilmata, Golden Thief Bug, Orc Hero, Drake, Mistress, Maya, Osiris, Pharaoh, Moonlight Flower, Baphomet and music encounters |
| 086 | 73 | Further boss skill variants and the unknown-skill-effect notice |
| 087 | 5 | Legacy pet/dismantling guide descriptions, translating the current English as written without inventing missing content |

Every one of the 133 source IDs in the 121101 boss-mechanic family is now authored. This is a scoped family completion, not completion of all game text. All IDs 10960000120–10960000201 and 10960000203–10960000364 are authored. ID 10960000202 is an incomplete fragment and retains English; 10960000019 remains held because “Defeats” does not establish whether kills or losses grant prestige. Pure FPS/EXP/token format rows in this family retain English.

Proper names, currencies, named skills, rank names and all bracketed labels remain English. Casting-setting explanations are translated around the protected English labels. Literal n/X/TBD notices, the ≥ sign, degrees, multiplication signs and existing missing parentheses are not repaired into guessed gameplay values. Realm zone instructions preserve the English's unusual claim that Guild Members can be attacked; no guild/enemy distinction was silently changed. Pet reset/refund uses the existing reviewed pet-menu meaning of Respawn.

### Numeric-rule ambiguity safeguard

The rendered-value test found that 10960000329 could split `123.5.123.5` at the wrong decimal boundary when optional style tags were absent. The generator now omits only global numeric rules whose consecutive numeric placeholders have no reliable separator (empty, period or comma, ignoring optional style tags). Their complete original/Thai mappings remain in the ID table. Seven IDs are ID-only for this safeguard: 21172, 10960000329, 23714, 90045, 37823, 42002 and 63545. This is a generated-data safeguard, not a DLL or EXE change. Distinct Pharaoh descriptions 12110100079/12110100082 also use matching unstyled whitespace so the same rendered English cannot select inconsistent Thai spacing.

Validation includes every generated runtime field against the canonical 33,513-ID merged table; all styled/unstyled numeric rules; unambiguous capture boundaries; source numbers, token order, bracket labels and controls; language-hook checks; updater integrity/caching; and actual public HTTPS feed verification of all 377 latest translations. The feed has 2,506 global numeric rules. No game or Windows UI test has been performed.

## Update — batches 088–091, three-agent translation team

The user requested three additional agents. Each received an immutable 210-ID English queue, disjoint from the others, with read-only access to the shared repository. The parent translated a separate 120-ID queue. Only the parent imported and published the approved batches.

| Batch | Contributor | Added IDs | Scope |
|---|---|---:|---|
| 088 | Agent A | 210 | 103 item/cosmetic/mount descriptions (112101) and 107 system messages (106300) |
| 089 | Agent B | 210 | 56 Survivor Battle skill/item descriptions (117701), 72 trivia/help statements (124100), 82 tutorial messages (105400) |
| 090 | Agent C | 210 | Combat, boss, guild/event, reward/gift and countdown announcements (103200) |
| 091 | Parent | 120 | Party/dungeon entry, cards/pets, auction/stall restrictions, skill use, Soul Echo, matchmaking and assistance messages (106300) |

The agents completed all 630 assigned entries; none were deferred. All four local draft checks passed, followed by parent review of high-risk item/system conditions, every 117701 skill/item entry, and event/time/reward cases. Parent review clarified old-fashioned as retro styling rather than worn condition (11210100107), retained the Elite membership name (10630000286), and explicitly identified the caster as the ATK owner in 11770100019. Item/CD spellings were aligned, and 22 exact-English duplicates reused the approved fresh target so identical source strings do not conflict.

### Source limitations retained

- Source percentages without an ATK/maximum-HP basis remain percentages; no missing formula basis is invented.
- Trailing style tokens in 11770100010/11770100037 remain unchanged. Trap Hunter in 11770100037/11770100038 is retained as written.
- Trivia may include intentional true/false statements. These are translated from English, not rewritten to match assumed game facts.
- Ilmata/Irmata and Gala Invitation/Festival Invitation spellings are preserved per source ID, not silently merged.
- Unitless countdowns (10320000512/10320000514) remain unitless; the unlabeled gift fields in 10320000310 do not gain guessed item/count units.
- Placement confirmation in certain entry/matchmaking messages and Neutral in the appearance restriction retain their unresolved English terms while surrounding prose is translated.
- Diagnostics, unusual rank-versus-point comparisons and the escort Trade wording are literal game data, not executed instructions or guessed mechanics.

### Validation

750 new authored IDs, bringing the total to 11,584. Source fidelity is checked across all 33,513 runtime rows. The complete feed contains 11,572 targets differing from English, 21,941 English targets and 2,745 numeric rules. Local tests passed: 189,695 runtime checks (including 90 mocked language-bridge checks), 27 compiled-plugin hook checks, 57 updater checks and authoring/corruption/decoded-number checks. Public verification additionally compares all 750 new targets, every manifest hash, the runtime TSV bytes against the canonical merged table, and the actual HTTPS updater/cache behavior.

No DLL/EXE, game script, installer behavior or Release changed. Runtime v0.4.1 receives the new data through the existing main feed on startup. This is not a completed translation of every game text, and no real-game or Windows UI testing has been performed.

## Latest update — batches 092–095: quest localization

Three translation agents completed disjoint quest queues and the parent added activity/reward objectives. Added 1,482 IDs: 434 quest descriptions (131502), 446 quest objectives/progression steps (131500), 350 NPC/dialogue/response records (103700), and 252 encounter/activity/reward conditions (110810/111901). All are translated from the verified current English, not legacy Thai. Eight assigned IDs retain English because of ambiguous Utility/faction wording or pure names.

Overall authored total is 13,066; the full 33,513-ID feed contains 13,054 targets differing from English and 20,459 English targets, with 2,856 numeric rules. All source fields, protected names/tokens, numbers, controls and styled/unstyled runtime rules are verified. Exact-English duplicates use the approved fresh target; a preflight also rejects conflicting rendered Thai when English becomes identical after numeric/style substitution. Local checks passed: 191,417 runtime checks, 27 compiled-plugin hook checks and 57 updater checks. Actual public HTTPS verification checks all 1,482 new ID/target pairs and manifest hashes/cache behavior.

The text/data-only update does not change DLLs, EXEs, game scripts, installation or Releases. Restart online with runtime v0.4.1 to receive the current main feed. Other quest/title/dialogue IDs and other families remain; this is not all-game translation completion. No real-game/Windows UI tests have been run. See `QUEST-TRANSLATION-STATUS.md` and `quest-translation-coverage.json` for source concerns and deferred IDs.

## Update — batches 096–099

| 096 | 599 | Quest objectives: remaining 131500 plus remaining 131506 stage trackers and Relax in 131502 |
| 097 | 682 | NPC dialogue, choices, services and quest storyline: first remaining 103700 half |
| 098 | 680 | NPC dialogue, choices, services and quest storyline: second remaining 103700 half |
| 099 | 628 | Descriptive quest titles, stage headings, job progression, party/guild activity labels (131501) |

2,589 new quest objective/tracker, NPC dialogue and descriptive-title IDs. 43 assigned pure-name/ambiguous rows retain English; see QUEST-TRANSLATION-STATUS.md and quest-translation-coverage.json. No new executable or Release. All local validation and runtime/hook/updater checks passed; public verification covers every new target. Actual game/Windows UI testing remains outstanding.

## Update — batches 100–103

| 100 | 1,179 | All remaining 106300 system/account/server/service error and status notices |
| 101 | 775 | 100105 combat/stat explanations, 111900 requirements, 105400 tutorial steps, 101401 mail/system announcements |
| 102 | 933 | 101000 quest/inventory/filter labels, 101600 market/settings screens and 109601 guide/UI headings |
| 103 | 967 | 107901 sources/shop headings, 101400 mail subjects, 120500 score/actions, 123400 event phases, 105700 requirements, all remaining readable 103200 gameplay/dialogue messages |

3,854 new higher-ID system/requirements/stat/tutorial/mail/UI/gameplay strings; 280 assigned names/formats/ambiguities retain English. See SYSTEM-TRANSLATION-STATUS.md and system-translation-coverage.json. English-only authoring and all local/runtime/hook/updater checks passed. No executable or Release change; actual game UI review remains outstanding.

## Update — batches 104–107

| 104 | 433 | Class/build/talent descriptions, buff/profession/crafting effects, cart/headwear lore and combat labels |
| 105 | 570 | Settings/social/guild/shop/auction/pass/service menus, emotive actions and scene labels |
| 106 | 640 | Quest/achievement requirements, activity/season/story/defense messages and book/lore text |
| 107 | 524 | Combat/stat labels and remaining readable below100000 UI candidates, with explicit canonical stat abbreviations |

2,167 new class/quest/profession/lore/UI/stat IDs, including 21 canonical English stat labels and 55 below100000 UI records. 1,004 assigned names/formats/ambiguities retained English. See CLASS-UI-TRANSLATION-STATUS.md. No executable/Release update. Real-game UI review remains outstanding.

## Update — batches 108–111

2,346 new authored IDs; 24,974 overall. 6 additional below100000 UI IDs; 6,777 authored and 283 retained in that inventory. See REMAINING-TRANSLATION-STATUS.md for exhaustive round-start ID accounting and the distinction between individual candidate review and catalog role/shape classification. Not every game string is Thai; no game/Windows UI test. Data-only main feed, no executable/Release change.

## Update — batches 112–115

952 new descriptive buff/effect/state labels, 24,974 authored IDs overall. All 1,515 remaining buff/effect catalog entries individually re-reviewed; 563 true names/formats/ambiguities retained English. See BUFF-EFFECT-TRANSLATION-STATUS.md. No new below100000 UI ID in this scope, no executable/Release change and no game/Windows UI test.
