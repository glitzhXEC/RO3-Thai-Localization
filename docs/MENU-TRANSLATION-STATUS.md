# Menu translation batch 046

This batch adds 264 manually reviewed, ID-scoped Thai UI translations. English from the game export is the source of truth. The Thai export is a terminology reference only: literal `None`, ID-only values, and translations that contradict English are not imported.

## Coverage

- Common UI: confirm/cancel, backpack, weapons/armor, back/exit, filters, notifications, naming, obtain/use, weekdays and time units.
- Settings: basic controls, targeting, skill bars, keyboard/mouse controls, account/logout, sound, language, display, graphics, HP/name colors, UI layout/scale, item bars and camera controls.
- 264 new source IDs; 3,644 authored translation rows in total. Runtime retains English for five held semantic-review IDs and seven authored rows whose value is English; 3,632 runtime IDs therefore differ from English.
- This is not a complete translation of all menus or all game text. Party/guild/trading/quests and other dedicated menu groups need further batches.

## Reference corrections

- ID 1066 `${1}m`: minutes (`${1} นาที`), not points as in the supplied Thai export.
- ID 27009 `Chase Distance`: `ระยะไล่ตาม`, not attack range.
- ID 27081 `Modern Mode`: `โหมดสมัยใหม่`, not Fashion.
- ID 27110: the player continuously normal-attacks the selected target; the subject is not reversed.
- Key labels inside brackets remain English. Placeholder order, numbers, math symbols and escaped line breaks are preserved.

## Intentionally left English in these groups

Format-only strings, sprite/color/link markup, frame-rate values, Hard/Semi-Soft/Full Soft Lock labels, and ambiguous labels such as IDs 27003, 27070, 27097 and 27143 were not guessed. ID 1050 protocol diagnostic is not part of this menu batch.

## Delivery and testing

The existing data-feed workflow regenerates the root override table and the schema-2 English-base runtime feed. No installer or runtime code change is required, and no new executable release is required for this text-only update. Use runtime v0.4.0 or later and restart the game with internet access to check for updated data; if offline, the runtime uses its existing verified cache/bundled data.

Automated validation covers placeholder/tag order, numeric values, mathematical symbols, bracket names, line breaks, Unicode, runtime ID matching and updater integrity. Native Windows UI layout, text clipping, and real-game behavior have not been tested for this batch.
