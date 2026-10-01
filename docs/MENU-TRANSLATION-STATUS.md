# Menu translation progress — batches 046–056

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
| 054 | 297 | Pet formation, hatching, growth, skills, collection and help text |
| 055 | 209 | Appearance/engraving, presets, training grounds and achievements |
| 056 | 232 | Card/bond/slot controls, titles/item restrictions and event calendars |

- 232 new IDs in the latest update; 3,378 authored menu IDs across batches 046–056.
- 6,758 authored translation IDs across all batches. Runtime retains English for five held semantic-review IDs and seven authored rows whose target equals English: 6,746 runtime IDs differ from English.
- **This is not a completed translation of every menu or every game text.** Newer gameplay systems and other groups still need review; some card/title/event labels also remain English as protected formats, technical names or ambiguous text. Some pet/appearance/preset rows remain held as described below. The exact unreviewed UI-candidate inventory is in `menu-translation-coverage.json`; these candidates also contain proper names, technical labels and format-only rows, so their count is not the count of menus requiring Thai.

## Reference corrections and held context

- 1066 `${1}m` means minutes, not points.
- 27009 `Chase Distance` means chase distance, not attack range.
- 27081 `Modern Mode` is not Fashion.
- 27110: the player continuously normal-attacks the selected target, not the reverse.
- 34075: join a party, not create one.
- 34108: leave a channel; removing another player is a separate action/confirmation.
- `Mount` is translated as a noun for mount categories only. The mount/dismount action (49023) and artillery label (61046) remain English pending context. `Deploy` uses the reviewed unit/card/pet/formation context.
- `Call Mio!!`, isolated `Placement confirmation`, `Misc`-specific party restrictions, `Charisma Baby`, `Stunt`, `Vivify`, `Quasi-Stats` and `Locate` are not guessed.
- Pet ID 40147 (`Get ${1} Off Summons`) lacks clear discount-unit context and remains English. The isolated Active skill-type label (40205) stays English instead of borrowing the unrelated active-state translation. Named event heading 40992 and preset `Stunt` label 64029 remain English.
- `Respawn` in the pet menu is a reset/refund action as explicitly defined by IDs 40051 and 40123, not character revival. The translation uses คืนค่าสัตว์เลี้ยง. Pet numbers, hatching chances, star/slot counts and stat percentages retain the source values.
- Some bracket-only UI labels remain English because the current runtime protects bracket contents as names. Proper item/location/class names, technical labels, frame rates and format-only strings remain unchanged.
- Party source data contains inconsistent bonus descriptions: 24043 differs from 24112–24116; 24115 and 24116 both say 4 players with different values. Translations preserve the English values exactly; no gameplay values were invented or corrected.

## Delivery and validation

Batch 056 itself is a text/data-only update. The separate language-table hotfix v0.4.1 changes the runtime DLL and requires a one-time plugin/installer update; see LANGUAGE-TABLE-HOTFIX.md. Subsequent text updates do not require new executable releases. Runtime v0.4.0 or later checks the verified schema-2 data feed on game startup. Restart the game with internet access to check for updates; offline/invalid downloads retain the verified cache or bundled fallback.

Validation checks placeholder/tag order, numbers, math symbols, bracket names, escaped controls, Unicode, exact ID/original matching, runtime rules and updater integrity. Additional menu QA decodes escaped control boundaries before counting numbers (so a number immediately after `\n` is still checked). Windows game UI layout, text clipping and in-game behavior have not been tested for these batches.
