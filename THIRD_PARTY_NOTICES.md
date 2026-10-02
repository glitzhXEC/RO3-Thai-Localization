# Third-party components in the partial skills payload

BepInEx 5.4.23.5 (LGPL-2.1) is the runtime dependency in the minimal installer payload. The project-owned translation plugin uses Harmony and its own approved dictionaries/data feed. XUnity AutoTranslator, XUnity ResourceRedirector, and the XUnity TMP font bundle are not included in the minimal payload.

Compatibility DLLs are extracted from the pinned bundle documented in `docs/runtime-provenance.json`. Corresponding compatibility modification sources are available at https://github.com/glitzhXEC/RO3_Asia_Thai_Patch/tree/main/tools/legacy-runtime-patching . This reuses runtime compatibility code, not its translations or its old custom localization plugin.

BepInEx: https://github.com/BepInEx/BepInEx
The BepInEx license text and runtime notice are embedded in the payload under `BepInEx/config/RO3.ThaiSkills.Licenses`. Third-party names and trademarks belong to their respective owners.
