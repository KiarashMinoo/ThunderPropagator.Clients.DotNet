---
title: Add heartbeat, reconnection with exponential backoff, and automatic re-subscription
labels: feature,reliability,priority:P1
---

## Summary
The client has `RequestPingAsync` but nothing calls it periodically; there is no reconnect logic (after an error the receive loop sleeps 10 s and retries `ReceiveAsync` on the dead socket), no resubscription, and the receive loop's 24 h cancel is the only timeout. Spec §13/§14 and checklist "Heartbeat & reconnection" are unmet.

## Server facts to design against
- Server sends the literal `PROBE` roughly every 30 s (to one connection per client IP); client already ignores it. WebSocket pings only if `KeepAliveInterval` is set. No idle-timeout disconnect.
- On disconnect the server drops all of the connection's subscriptions. The client must re-send `Subscribe` after reconnecting **with a new `RequestId`**: a duplicate `RequestId` is rejected (406 `DuplicatedRequestIdException`).
- Snapshot replay is queued asynchronously after the `Subscribed` response, with `FromSnapshot=true`.
- Slow-client queue (default 512, DropOldest) can silently drop pushes.

## To do
- Heartbeat timer (`HeartbeatIntervalMs`, timeout detection), exponential backoff with jitter, bounded retries, `ConnectionState` transitions.
- Re-create channel subscriptions after reconnect (new RequestIds), surface snapshot vs live updates.
- Events/options per spec §15.
