# UI compatibility investigation / hotfix

## Evidence from user runtime log (no private paths or full log published)

The game reports `System.Runtime.Serialization.DataContractAttribute..ctor()` missing while `System.MonoCustomAttrs.IsDefined` enumerates types during `SoftMasking.SoftMask..ctor()`. SoftMask subsequently throws null-reference errors during UI rendering. The updater's newly added wire classes carried these attributes. This is a direct compatibility defect in the patch and a strong causal lead for broad UI failures, not evidence of a specific mistranslated localization ID.

The same log rejects `DataContractJsonSerializer` and `HttpWebRequest.set_ReadWriteTimeout(int)`. The offline dictionary still loads (3,375 IDs / 1,848 rules), but the alpha.1 online updater does not function in that runtime.

## Main-table ID audit

3,380 unique IDs; no duplicate ID, no changed English relative to the English snapshot. No missing/reordered protected placeholders/tags, changed numeric/math counts, changed line breaks, corrupt Unicode or changed bracketed status names detected by the structural audit. This is not a semantic or in-game display certification.

18 IDs contain an odd number of style markers already in English; the identical marker sequence is retained in Thai. Odd counts do not prove invalid formatting, since style switches can remain active to the end of a string. Do not invent closing markers or delete these IDs solely from this heuristic:

10110300396, 10800100137, 10110300181, 10110300193, 10110300528, 10110300610, 10800100235, 10800100236, 12390100279, 12390100278, 12390100277, 12390100276, 12390100275, 12390100274, 10110300829, 10110300915, 10110300590, 10110300234

## Short-ID/global UI collision risk

Examples found in main: `10110300700` = None, `10220300175` = Stun, `10220300525` = Burn, `10110300423` = Mining, `10110300424` = Logging, `10110300105` = Collect. The old global exact-match branch ran before the short-text guard, so these dictionary entries could also replace unrelated UI labels with the same English text. This does not explain the SoftMask exception, but is a separate scope bug.

The hotfix requires at least 45 characters for global exact-text fallback, while short effects/descriptions remain available through exact ID + original English. Numeric-template rules remain fully anchored and tested. Main translations are not deleted or rewritten. Add regression checks for common short UI labels.

## Comparison to legacy patch

Legacy main config uses `OverrideFont=Arial`, `FallbackFont=Arial`; the new alpha changed both to Tahoma. This hotfix restores Arial defaults to avoid an unnecessary global font/layout change. Both use `FallbackFontTextMeshPro=arialuni_sdf_u2022`, `EnableUIResizing=True`, `ForceUIResizing=False`, and the same rich-text settings. The supplied log also reports a TMP fallback asset version mismatch (1.1.0 vs TMP 1.4.0); this is a separate follow-up, not proof that translations corrupt tags. The legacy configuration source is https://github.com/glitzhXEC/RO3_Asia_Thai_Patch/blob/main/Client/BepInEx/config/AutoTranslatorConfig.ini . No old Thai translations are used.

## Hotfix

- Remove all System.Runtime.Serialization references and wire attributes from the engine. Use explicit bounded JSON parsing/writing without assembly-scan-visible serializer annotations.
- Remove ReadWriteTimeout and Flush(bool) calls; use ordinary Flush and an Abort watchdog for bounded network operations.
- Preserve schema-1 manifest/cache format, trusted HTTPS origin, SHA-256, immutable versioned feed, atomic cache, offline fallback and ownership/uninstall protection.
- Add regression checks for unsupported assembly references/attributes and strict duplicate-key, number, trailing-data and Unicode rejection.
- Build rejects metadata containing the incompatible serialization/API references.
- Restore legacy Arial font defaults; do not overwrite the game's System*.dll, Unity DLLs or SoftMask binaries.

Net472 compilation, net8 dictionary/updater/install tests and real HTTPS feed download are checked. Actual stripped-runtime UI behavior must be retested by the user. This is a targeted hotfix Alpha, not a claim that all UI defects are resolved.
