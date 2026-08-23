# 0004 Bounded SPT 4.1 Patch Compatibility

## Status

Accepted

## Context

SPT 4.1.0 is not compatible with the server and client APIs used by the later 4.1 package, so it remains a separate mod generation. SPT 4.1.2 and 4.1.3 have the same global locale inputs, EFT build, client patch targets, and server APIs used by this project. Publishing another binary that differs only by an exact patch-version gate creates unnecessary splits.

An unbounded compatibility declaration would also admit prereleases, SPT 4.2, and later API families that have not been inspected.

## Decision

Build the SPT 4.1 server DLL against the lowest admitted 4.1.2 packages and declare `~4.1.2`. The client uses the equivalent stable-release rule: `4.1.x` with a patch number of at least 2. Both sides reject 4.1.0, 4.1.1, prereleases, and 4.2 or later.

The latest version-labelled 4.1 archive is shared by the admitted patch family. SPT 4.1.2 and 4.1.3 are verified now. A future stable 4.1.x patch may load automatically but is marked verified only after its server API references, client patch targets, locale data, and runtime behavior are checked.

The release workflow always rebuilds and executes one compatibility contract against both the client predicate and the server semantic-version range before packaging. Skipping the build is not supported.

## Consequences

SPT 4.1 users no longer need separate binaries for 4.1.2 and 4.1.3. The bounded rule avoids the known-incompatible early 4.1 patches and future 4.2 API drift. Documentation and release status must distinguish versions admitted by the loader from versions that have completed static and in-game verification.
