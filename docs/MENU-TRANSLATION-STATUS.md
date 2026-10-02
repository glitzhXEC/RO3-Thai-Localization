# Menu translation progress — batches 046–073

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
| 057 | 298 | AFK/field battles, expeditions/chests, stalls, profiles and stickers |
| 058 | 183 | Spirit Tower teams/challenges/sweeps and auction/bidding/pre-purchase controls |
| 059 | 186 | Guild PvE dungeons, dragon defense, event management and battle notifications |
| 060 | 152 | Guild league schedules, battlefield commands, flags, scores and rankings |
| 061 | 357 | Rentals/cart, shop/gifts, friends/groups/chat, mounts and ranking rewards |
| 062 | 178 | Party/raid dungeon ready checks, objectives/rewards, matchmaking and auctions |
| 063 | 270 | PC/mobile control modes, HUD/chat settings, Tavern voice/gifts/moderation |
| 064 | 186 | Siege/territory battles, war machines, commands, markers and contribution reports |
| 065 | 291 | Territory bidding, guild supply trade/escorts, fatigue alerts and festival quizzes |
| 066 | 284 | Battle of Survivors queues/builds, loot, safe zones, rescues, scores and match messages |
| 067 | 222 | Job advancement, growth paths, power guidance, battle statistics and Divine Art Totems |
| 068 | 206 | Solo/duo actions, rifts, journey stories, season events and rewards |
| 069 | 82 | Encounter decks, profile cosmetics, monster hunts, requests and crystal donation events |
| 070 | 243 | Login/update/download/account menus, Adventure Handbook/Codex and Soul Echoes |
| 071 | 23 | Live-export lore and age/identity/payment notices, preserving CR/tab controls |
| 072 | 153 | Remaining common controls/errors, trading alerts, MVP rules and rankings |
| 073 | 24 | Remaining menu headings/help placeholders, guild messages and format labels |

- 953 new IDs in the latest update; 6,716 authored menu IDs across batches 046–073.
- 15,655 authored translation IDs across all batches. Runtime retains English for five held semantic-review IDs and seven authored rows whose target equals English: 13,054 runtime IDs differ from English.
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
- Lore ID 44010 and age notice ID 53001 are now translated from the verified current English export, including its CR/tab controls. Authoring QA now uses all 33,513 current English IDs. Regression tests reject dropping CR from 44010 or tab from 53001. No runtime DLL or game script was changed. The notices retain the source ages, times, RMB limits and institutions; they are translations of in-game policy text, not statements of Thai law.
- Survivor Battle keeps named mechanics, class names, currencies, rank names and NPC/event names in English; UI/control text around these names is translated. Connect/Locate compound labels now translate their surrounding text but preserve the uncertain source term. The previously held settings label 27003 (Log), standalone ambiguous labels and bracket-only button/help text still require review. Developer help placeholders are translated literally without inventing missing content.
- Some bracket-only UI labels remain English because the current runtime protects bracket contents as names. Proper item/location/class names, technical labels, frame rates and format-only strings remain unchanged.
- Party source data contains inconsistent bonus descriptions: 24043 differs from 24112–24116; 24115 and 24116 both say 4 players with different values. Translations preserve the English values exactly; no gameplay values were invented or corrected.

## Delivery and validation

Batches 067–073 are a text/data-only update. The separate language-table hotfix v0.4.1 changes the runtime DLL and requires a one-time plugin/installer update; see LANGUAGE-TABLE-HOTFIX.md. Subsequent text updates do not require new executable releases. Runtime v0.4.0 or later checks the verified schema-2 data feed on game startup. Restart the game with internet access to check for updates; offline/invalid downloads retain the verified cache or bundled fallback.

Validation checks placeholder/tag order, numbers, math symbols, bracket names, escaped controls, Unicode, exact ID/original matching, runtime rules and updater integrity. Additional menu QA decodes escaped control boundaries before counting numbers (so a number immediately after `\n` is still checked). Windows game UI layout, text clipping and in-game behavior have not been tested for these batches.

## Remaining review inventory

`menu-untranslated-review.json` lists the 344 source IDs below 100000 not present in authored batches. This is a scoped UI-review inventory, not an exhaustive game-text backlog. It separates technical/format strings, explicitly retained names, bracket-protected text and unresolved context. The current DLL treats bracket content as protected names even in some highlighted UI prose; changing those clauses would require a compatible runtime feature, not simply a TSV edit. They retain the English base for now.

## Gameplay descriptions beyond the menu inventory

Batches 074–078 add 361 IDs above 100000, covering equipment/card/AFK/guild guides, remaining readable skill/item descriptions, boss mechanics, siege alerts and airship encounters. These are not added to the below-100000 menu totals. See `GAMEPLAY-TRANSLATION-STATUS.md`. Overall authored totals above include these batches.

Batches 079–087 add a further 377 high-ID guide and boss-mechanic translations. The below-100000 menu review totals remain unchanged.

Batches 088–091 add 750 high-ID item/skill descriptions, tutorial/trivia text, gameplay announcements and system messages. Three additional translation agents supplied 210 disjoint IDs each; the parent translated 120 and performed final integration/QA. These do not change the below-100000 menu review inventory.

Batches 092–095 add 1,482 quest descriptions/objectives, NPC dialogues and activity/reward conditions. These are high-ID records and leave the below-100000 review inventory unchanged. See `QUEST-TRANSLATION-STATUS.md`.

Batches 096–099 add 2,589 further high-ID quest trackers, all remaining readable assigned NPC dialogue and descriptive quest titles. The below-100000 review inventory remains unchanged; actual game UI has not been tested.
