# 0004 Bounded SPT 4.1 Patch Compatibility

## Status

Accepted

## Context

SPT 4.1.0 is not compatible with the server and client APIs used by the later 4.1 package, so it remains a separate mod generation. SPT 4.1.2 and 4.1.3 have the same global locale inputs, EFT build, client patch targets, and server APIs used by this project. Publishing another binary that differs only by an exact patch-version gate creates unnecessary splits.

An unbounded compatibility declaration would also admit prereleases, SPT 4.2, and later API families that have not been inspected.

## Decision

Build the SPT 4.1 server DLL against the lowest admitted 4.1.2 packages and declare `>=4.1.2 <=4.1.3`. The client uses the equivalent stable-release rule. Both sides reject 4.1.0, 4.1.1, prereleases, unverified later 4.1 patches, and 4.2 or later.

The `4.1.2-4.1.3`-labelled archive is shared by the two verified patches. A future stable 4.1.x patch may satisfy the binary gate but does not become a release target until its server API references, client patch targets, locale data, and runtime behavior are checked and the release label is updated.

The release workflow always rebuilds and executes compatibility contracts for the exact 4.1.0 target and the bounded 4.1.2+ target against both the client predicate and server semantic-version range before packaging. Skipping the build is not supported.

## Consequences

SPT 4.1 users no longer need separate binaries for 4.1.2 and 4.1.3. The bounded rule avoids the known-incompatible early 4.1 patches and future 4.2 API drift. Documentation and release status must distinguish versions admitted by the loader from versions that have completed static and in-game verification.
