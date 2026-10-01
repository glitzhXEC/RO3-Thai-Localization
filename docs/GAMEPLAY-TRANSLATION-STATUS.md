# Gameplay translation progress — batches 074–078

The verified current English game export is the only source. Existing fresh terminology is reused where the English text and meaning match; old Thai translation files are not imported.

| Batch | Added IDs | Scope |
|---|---:|---|
| 074 | 60 | Equipment forging/appraisal/refine/enhance, wardrobe/shop, guild funds/supplies/tactics/management, trade, gifts and mounts |
| 075 | 54 | Tavern/relationships, MVP reward rules, AFK/field drops, expedition rewards, Soul Echo/Totem/card guides, Codex and Survivor Battle help |
| 076 | 27 | Remaining readable skill/item descriptions and literal test/designer notices |
| 077 | 166 | Boss/dragon mechanics, siege and territory warnings, map-object status, killstreaks and combat instructions |
| 078 | 54 | Airship encounters, grappling/cannon instructions, boss ground-warning order, Dracula/bride rescue and spawn alerts |

361 new authored IDs; 10,457 authored IDs overall. Of the complete 33,513-ID runtime table, 10,445 IDs differ from English and 23,068 retain English. Overall values exclude no source IDs; five semantic-review IDs and seven unchanged authored targets retain English.

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

Authoring QA and decoded-control number QA, corruption rejection, complete English-base audit, runtime rules, language-hook harness and updater tests must pass before publication. Public feed verification checks all three file hashes and all 361 new ID/target pairs through the actual HTTPS updater. These are automated data/mock-runtime checks, not Windows/game UI tests.

**This is not a completed translation of every game text.** Other guide, event and quest/dialogue families remain; proper names and technical formats are intentionally retained. In-game wording, layout and clipping have not been verified.
