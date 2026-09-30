# Third-party components in the partial skills payload

BepInEx 5.4.23.5 (LGPL-2.1), XUnity AutoTranslator 5.6.2 / ResourceRedirector 2.1.0 (MIT), and a TextMeshPro fallback font asset are runtime dependencies.

Compatibility DLLs are extracted from the pinned bundle documented in `docs/runtime-provenance.json`. Corresponding compatibility modification sources are available at https://github.com/glitzhXEC/RO3_Asia_Thai_Patch/tree/main/tools/legacy-runtime-patching . This reuses runtime compatibility code, not its translations or its old custom localization plugin.

BepInEx: https://github.com/BepInEx/BepInEx
XUnity: https://github.com/bbepis/XUnity.AutoTranslator
Font bundle asset origin: XUnity official TMP_Font_AssetBundles_2025-05-12.7z, release v5.4.5. Asset SHA256: de18a759d475e01f90cffee16bdc861bf3aa09bc501f3c622e5822622b5e1606 . Upstream: https://github.com/bbepis/XUnity.AutoTranslator/releases/tag/v5.4.5

The full license texts and runtime notice are embedded in the payload under `BepInEx/config/RO3.ThaiSkills.Licenses`. Third-party names and trademarks belong to their respective owners.
