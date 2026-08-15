# Security And Boundaries

Release packages write only the version's Korean server-mod folder and `BepInEx\plugins\GoLani.KoreanModFix.dll` when the user extracts them. They contain no installer, command script, executable, absolute path, parent traversal, SPT database replacement, BepInEx core file, or unrelated mod.

The release builder may delete and recreate only paths proven to be descendants of this repository's `artifacts` directory. It reads generated locale data from the sibling translation repository but never modifies that repository.

This project does not need tokens, cookies, account credentials, launcher authentication data, or SPT profile contents.
