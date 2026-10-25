---
title: Harden response handling and map server status codes to exceptions
labels: bug,protocol,priority:P2
---

## Findings
- `HandleReceivedResponse` parses `ResponseContent` with `FromNJson<JObject>()` unconditionally; when `Accept` selects another serializer the content is not JSON and the client throws before correlating the request.
- `ThunderPropagatorClient` indexes `_channels[clientBaseResponse.Route.Channel]` / `_channels[match.Groups[1].Value]`; an unknown channel throws `KeyNotFoundException` inside the receive loop (which then sleeps 10 s).
- `ThunderPropagatorPingRequest` handling calls `Console.WriteLine(content)`.
- Failed requests throw a generic `Exception`/`AggregateException`; request timeout is hard-coded to 40 s.
- Unknown or invalid push payloads must be logged and skipped (spec 18.3), not crash the receive loop.

## Server codes to map
200 OK; 400 JSON/KeyNotFound/ArgumentOutOfRange; 403 forbidden key/field, bad credentials, role/policy failure (note: the server uses 403, not 401, for bad credentials); 405 NotImplemented; 406 missing/invalid keys or fields, disabled channel, duplicate RequestId, missing credentials; 422 invalid operation/cast/data; 500 other. A suspected server bug may surface real errors nested as 500 (`TargetInvocationException` not unwrapped): verify before relying on codes.

## To do
Typed exceptions (`ThunderPropagatorAuthenticationException`, `...RequestRejectedException`, etc.), configurable request timeout, safe dispatch for unknown channels, tests.
