# 0003 Version-Specific Release Layout

## Status

Accepted; the exact-version rule is superseded for SPT 4.1 patch releases by decision 0004, and the asset count and names are revised by decision 0005.

## Context

SPT 3.x, 4.0, and 4.1 load server mods from different root-relative paths. A single archive that contains all three paths is easy to extract but leaves inactive payloads and newly created folders in every installation. A plain ZIP cannot select a destination based on the installed SPT version.

## Decision

Publish Korean-only and Korean-English ZIP files for each exact supported SPT version. Every archive contains the universal BepInEx client DLL and only the matching server mod path and locale payload. Decision 0004 replaces only the exact patch-version rule for the bounded SPT 4.1 family; it does not change this layout isolation.

Users install by extracting one matching ZIP at the SPT root. No installer or cleanup script is included.

## Consequences

The original matrix had 12 ZIP assets instead of two. Decision 0005 expands it to 14 by restoring an exact SPT 4.1.0 pair while combining 4.1.2–4.1.3. Installations remain clean, versions outside each exact or bounded declaration are rejected by server metadata, and every archive can be validated independently against its declared English key order and generated locale source.
