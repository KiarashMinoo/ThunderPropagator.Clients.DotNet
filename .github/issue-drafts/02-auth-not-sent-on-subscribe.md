---
title: Credentials are not attached to Subscribe/Unsubscribe requests (pattern-precedence bug)
labels: bug,auth,priority:P0
---

## Summary
In `AbstractThunderPropagatorChannel.RequestAsync`:
```csharp
if (request is not ThunderPropagatorMetadataRequest and ThunderPropagatorPingRequest)
```
`not` binds only to the next pattern, so this means `(not MetadataRequest) and PingRequest`, i.e. only **Ping** requests get credentials (and require `ChannelMetadata`). Subscribe/Unsubscribe on an auth-enabled channel go out without `Token` / `Username` / `Password`.

## Server behaviour
- Pipeline order: Ping 0, Metadata 100, Authentication 200, Authorization 300, Subscribe/Unsubscribe 1000. Ping and RequestMetadata are answered before authentication, so they do not need credentials; Subscribe/Unsubscribe do.
- Credentials travel in the request body: OAuth2 `Token`; Basic `Username` + `Password`, each RSA-encrypted with the channel public key.
- Failures: bad token/user -> 403 (not 401); missing `Token`/`Username`/`Password` -> 406.

## To do
- Fix the condition to `request is not (ThunderPropagatorMetadataRequest or ThunderPropagatorPingRequest)` and add unit tests for each request type x auth type (None, OAuth2, Basic).
- Do not send credentials on Ping/Metadata.
- Map 403/406 on auth failures to a clear exception type (see error-handling issue).

## Acceptance
Subscribe/Unsubscribe against OAuth2 and Basic channels succeeds; Metadata/Ping never carry credentials.
