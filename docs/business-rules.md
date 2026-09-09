# Business Rules

## Release format and compatibility

2.1.0 uses one plugin-only ZIP, `SPT-KR-2.1.0.zip`, containing both Korean and Korean–English modes. Users install the BepInEx folder at the game root. No installer, server mod, status companion, game executable, save or personal configuration is included.

Explicit SPT profiles are 3.8.3, 3.9.8, 3.10.5, 3.11.4, 4.0.13, 4.1.0, 4.1.2, 4.1.3 and 4.1.5. Unlisted stable patches from 4.1.2 up to, but excluding, 4.2.0 require matching EFT build, English source and native hooks. Exact profiles cannot silently fall back after a validation failure. 4.1.1, prereleases and other unlisted families are rejected.

The matching local server database is required. A remote-server-only client is outside the supported installation model. Runtime fixture coverage must be distinguished from actual installed-client inspection and visual testing.

## Translation ownership and behavior

Locale files come from generated outputs of the sibling `spt-korean-translate` repository. Do not hand-edit duplicate translation snapshots here. Payload hashes, key order and the installed English key/value set must validate before localization starts.

The client provides adjacent native language choices and switching without restart, preserves unknown mod keys and does not rewrite server databases. Already-rendered server/mod messages are not guaranteed to be translated. The game log reports version and initialization results; there is no separate server startup notice.

## Updating and publication

Users back up and remove old Korean server modules, test status companions and duplicate client DLLs before installing. Backups must be outside mod/plugin discovery paths. See [installation and rollback](releases/2.1.0-install.md).

The release is prepared locally before publication. Ordinary source commits and pushes do not publish a GitHub release. The author creates the release/tag and uploads the verified ZIP following [upload instructions](releases/2.1.0-upload-instructions.md).
