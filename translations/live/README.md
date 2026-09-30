# Translation data feed — runtime schema 2

Generated from approved English-only batches and RO3.LocalizationOverrides.tsv. Never hand-edit a version directory.
manifest.json points to three SHA-256 checked, content-versioned TSV files. No DLL/EXE/ZIP or arbitrary URLs are supported by the client updater.
The full source table remains in translations/RO3.LocalizationOverrides.tsv. Known semantic-review IDs retain English. The merged table restores Chinese only against known original Chinese values for the same ID; arbitrary CJK text is never translated.
Publish a new feed by running scripts/prepare_live_translations.py and runtime tests, then committing the manifest and immutable version files together.
The updater checks only on startup, caches a validated bundle atomically, and keeps the previous dictionary when offline or validation fails.
SHA-256 checks consistency/integrity, not publisher identity; trust is the configured HTTPS GitHub repository.
