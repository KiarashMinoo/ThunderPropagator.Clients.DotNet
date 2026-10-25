---
title: Propagate trace context and expose active protocol / protocol-change events
labels: feature,observability,priority:P2
---

- Server honours `traceparent`, `tracestate` and `baggage` request keys (`ChannelManager.Receive.cs`). Add optional propagation from `Activity.Current` in `RequestAsync`.
- Spec §19 Observability: expose `ActiveProtocol` (string) and an `OnProtocolChanged(from, to)` event on `ThunderPropagatorClient` (currently only `ConnectionProtocol` fixed at construction).
- Structured logging via the repo's own `ILogger` only (see CLAUDE.md).
