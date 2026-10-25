---
title: Spec §3 features missing: protocol negotiation, IDS failover, shadow connection, autoUpgrade; WebTransport/MQTT not supported
labels: feature,transport,priority:P2
---

- The client picks one protocol at construction (`ThunderPropagatorProtocolType`), with no negotiation order, no pre-warmed IDS shadow connection, no failover on heartbeat timeout and no auto-upgrade probe (spec §3, checklist "Connection").
- IDS limits to design around: `POST /ThunderPropagator/v{version}/{channelName}/subscribe` (or `/{channelKey:guid}/subscribe`), body `SubscribingKeys`/`SubscribingFields`/`SubscriptionMode`/`RequestId`; pushes are newline-terminated chunks; the IDS connection cannot receive requests (no Metadata/Ping/Unsubscribe over it).
- Server also exposes WebTransport (`/thunderPropagator`, single stream, no framing) and an optional MQTT handler; neither has a client implementation. Quic is license-gated and off by default.
- Decide the target design (spec vs what the server can actually do), then implement failover on top of the heartbeat/reconnect issue.
