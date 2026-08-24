# 0005 Short Release Names And SPT 4.1 Split

## Status

Accepted

## Context

The previous ZIP names were unnecessarily long. SPT 4.1.0 also needs its own server and client compatibility gates, while SPT 4.1.2 and 4.1.3 have equivalent English, KR, and KR-EN locale payloads and can share one package.

## Decision

Use `SPT-KR-<version-or-range>.zip` for the Korean variant and `SPT-KR-EN-<version-or-range>.zip` for the bilingual variant. Publish exact SPT 4.1.0 assets and explicitly label the shared assets as `4.1.2-4.1.3`.

Build the 4.1.0 client and server DLLs separately with exact compatibility gates. The release builder must reject the shared 4.1.2–4.1.3 package if either version's English, KR, or KR-EN JSON values or key order differ.

## Consequences

The release contains 14 ZIP assets. Users can distinguish exact 4.1.0 from the shared 4.1.2–4.1.3 package directly from the filename, and future locale drift cannot silently produce a falsely shared archive.
