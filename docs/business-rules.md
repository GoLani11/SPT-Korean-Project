# Business Rules

## Release format and compatibility

2.1.1 uses one plugin-only ZIP, `SPT-KR-2.1.1.zip`, containing both Korean and Korean–English modes. Users install the BepInEx folder at the game root. No installer, server mod, status companion, game executable, save or personal configuration is included.

Explicit SPT profiles are 3.8.3, 3.9.8, 3.10.5, 3.11.4, 4.0.13, 4.1.0, 4.1.2, 4.1.3 and 4.1.5. Unlisted stable patches from 4.1.2 up to, but excluding, 4.2.0 require matching EFT build, English source and native hooks. Exact profiles cannot silently fall back after a validation failure. 4.1.1, prereleases and other unlisted families are rejected.

The matching local server database is required. A remote-server-only client is outside the supported installation model. Runtime fixture coverage must be distinguished from actual installed-client inspection and visual testing.

## Translation ownership and behavior

Locale files come from generated outputs of the sibling `spt-korean-translate` repository. Do not hand-edit duplicate translation snapshots here. Payload hashes, key order and the installed English key/value set must validate before localization starts.

The client provides adjacent native language choices and switching without restart, preserves unknown mod keys and does not rewrite server databases. Already-rendered server/mod messages are not guaranteed to be translated. The game log reports version and initialization results; there is no separate server startup notice.

## Updating and publication

Users back up and remove old Korean server modules, test status companions and duplicate client DLLs before installing. Backups must be outside mod/plugin discovery paths. See [installation and rollback](releases/2.1.1-install.md).

The release is prepared locally before publication. Ordinary source commits and pushes do not publish a GitHub release. The author creates the release/tag and uploads the verified ZIP following [upload instructions](releases/2.1.1-upload-instructions.md).

## Automatic preservation of mod text (2.1.1)

The client compares raw Korean backend entries with the installed Korean database (English fallback for missing entries). Unchanged entries receive our translation; changed and new entries keep the server value in both Korean and bilingual modes. No user configuration or mod-specific allowlist is required. Modified text is preserved as a whole, including its original language; it is not machine-translated. Translation payload integrity checks remain enabled.

Raw server fragments are retained per localization-manager instance and mirrored before translation so later partial updates cannot erase earlier mod changes. A server update restoring an original value restores our translation; restarting after uninstalling a mod also starts with a fresh comparison. Direct client-side UI replacements and on-disk modifications to the comparison database are outside this detection model.
