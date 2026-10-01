# Menu translation progress — batches 046–053

English from the actual game export is the source of truth. Native Thai from the supplied ZIP is a terminology reference only; literal `None`, ID-only values and translations that contradict English are not copied. Existing approved fresh translations are reused for identical English labels to maintain consistent terminology.

## Delivered menu batches

| Batch | Added IDs | Scope |
|---|---:|---|
| 046 | 264 | Common controls and settings |
| 047 | 267 | Backpack, mail, maps, quests and chat |
| 048 | 839 | Exact common UI labels repeated across menu IDs |
| 049 | 190 | Party/raid invitations, leadership, follow, recruitment and messages |
| 050 | 406 | Guild creation, impeachment, management, buildings, caravan and trade |
| 051 | 220 | Feature unlocks, auto-battle, navigation, login/network, revive and skill builds |
| 052 | 234 | Trade market, reservations, penalties, search, combine/dismantle |
| 053 | 220 | Equipment appraisal, crafting, smelting, refine/enhance and character stats |

- 2,109 new IDs in the latest update; 2,640 authored menu IDs across batches 046–053.
- 6,020 authored translation IDs across all batches. Runtime retains English for five held semantic-review IDs and seven authored rows whose target equals English: 6,008 runtime IDs differ from English.
- **This is not a completed translation of every menu or every game text.** Dedicated pet, appearance, events, achievements, presets, newer gameplay systems and other groups still need review. The exact unreviewed UI-candidate inventory is in `menu-translation-coverage.json`; these candidates also contain proper names, technical labels and format-only rows, so their count is not the count of menus requiring Thai.

## Reference corrections and held context

- 1066 `${1}m` means minutes, not points.
- 27009 `Chase Distance` means chase distance, not attack range.
- 27081 `Modern Mode` is not Fashion.
- 27110: the player continuously normal-attacks the selected target, not the reverse.
- 34075: join a party, not create one.
- 34108: leave a channel; removing another player is a separate action/confirmation.
- `Mount` is translated as a noun for mount categories only. The mount/dismount action (49023) and artillery label (61046) remain English pending context. `Deploy` uses the reviewed unit/card/pet/formation context.
- `Call Mio!!`, isolated `Placement confirmation`, `Misc`-specific party restrictions, `Charisma Baby`, `Stunt`, `Vivify`, `Quasi-Stats` and `Locate` are not guessed.
- Some bracket-only UI labels remain English because the current runtime protects bracket contents as names. Proper item/location/class names, technical labels, frame rates and format-only strings remain unchanged.
- Party source data contains inconsistent bonus descriptions: 24043 differs from 24112–24116; 24115 and 24116 both say 4 players with different values. Translations preserve the English values exactly; no gameplay values were invented or corrected.

## Delivery and validation

This is a text/data-only update. No installer or runtime DLL change is required, and no new executable release is required. Runtime v0.4.0 or later checks the verified schema-2 data feed on game startup. Restart the game with internet access to check for updates; offline/invalid downloads retain the verified cache or bundled fallback.

Validation checks placeholder/tag order, numbers, math symbols, bracket names, escaped controls, Unicode, exact ID/original matching, runtime rules and updater integrity. Additional menu QA decodes escaped control boundaries before counting numbers (so a number immediately after `\n` is still checked). Windows game UI layout, text clipping and in-game behavior have not been tested for these batches.
