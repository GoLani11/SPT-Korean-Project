# 0003 Version-Specific Release Layout

## Status

Accepted

## Context

SPT 3.x, 4.0, and 4.1 load server mods from different root-relative paths. A single archive that contains all three paths is easy to extract but leaves inactive payloads and newly created folders in every installation. A plain ZIP cannot select a destination based on the installed SPT version.

## Decision

Publish Korean-only and Korean-English ZIP files for each exact supported SPT version. Every archive contains the universal BepInEx client DLL and only the matching server mod path and locale payload.

Users install by extracting one matching ZIP at the SPT root. No installer or cleanup script is included.

## Consequences

The release has 12 ZIP assets instead of two. Installations remain clean, version mismatches are rejected by server metadata, and every archive can be validated independently against its exact English key order and generated locale source.
