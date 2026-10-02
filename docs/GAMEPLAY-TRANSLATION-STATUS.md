# Gameplay translation progress — batches 074–087

The verified current English game export is the only source. Existing fresh terminology is reused where the English text and meaning match; old Thai translation files are not imported.

| Batch | Added IDs | Scope |
|---|---:|---|
| 074 | 60 | Equipment forging/appraisal/refine/enhance, wardrobe/shop, guild funds/supplies/tactics/management, trade, gifts and mounts |
| 075 | 54 | Tavern/relationships, MVP reward rules, AFK/field drops, expedition rewards, Soul Echo/Totem/card guides, Codex and Survivor Battle help |
| 076 | 27 | Remaining readable skill/item descriptions and literal test/designer notices |
| 077 | 166 | Boss/dragon mechanics, siege and territory warnings, map-object status, killstreaks and combat instructions |
| 078 | 54 | Airship encounters, grappling/cannon instructions, boss ground-warning order, Dracula/bride rescue and spawn alerts |

738 new authored IDs across batches 074–087, including 377 in the latest 079–087 update; 10,834 authored IDs overall. Of the complete 33,513-ID runtime table, 10,822 IDs differ from English and 22,691 retain English. Overall values exclude no source IDs; five semantic-review IDs and seven unchanged authored targets retain English.

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

Authoring QA and decoded-control number QA, corruption rejection, complete English-base audit, runtime rules, language-hook harness and updater tests must pass before publication. Public feed verification checks all three file hashes and all 377 latest ID/target pairs through the actual HTTPS updater. The generated runtime English/Thai fields are also checked against the canonical merged table for all 33,513 IDs. These are automated data/mock-runtime checks, not Windows/game UI tests.

**This is not a completed translation of every game text.** Other guide, event and quest/dialogue families remain; proper names and technical formats are intentionally retained. In-game wording, layout and clipping have not been verified.

## Quoted TSV field correction

The actual HTTPS smoke test exposed a generator parsing error for 10320000172: ordinary CSV quoting removed its enclosing literal quotation marks, so the source English no longer matched the actual game string. The generator now reads raw runtime TSV with `csv.QUOTE_NONE`, preserving literal quotes rather than treating them as CSV field delimiters. This repairs 26 runtime rows (including English-base-only rows) without changing translation IDs, source text, DLLs or executables. A regression compares every generated English/Thai field byte-for-byte with the canonical merged TSV before publication. Source snapshot TSV parts retain their existing proper CSV reader; only the raw generated TSV reader was corrected.

## Latest update — batches 079–087

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
