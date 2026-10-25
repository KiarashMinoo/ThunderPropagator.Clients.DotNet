---
title: Handle cluster responses: ClusterPeers, 307/503 Retry-After, leader address, transport-not-supported
labels: feature,cluster,priority:P2
---

## Server behaviour (cluster builds)
- Subscribe responses may carry `ClusterPeers` (URI list) intended for client failover/reconnect.
- Leader-only HTTP endpoints return 307 + `Location` (leader known) or 503 + `Retry-After: 5` (no leader); `X-Leader-Address` header on responses; Web-level gateway errors: 503 `cluster_no_leader` / `cluster_leader_changed` / `cluster_no_websocket_capacity`, 508 hop/loop limits, 502/504.
- Followers of type ApiOnly/WebSocketOnly answer 400 `realtime_transport_not_supported` for transports they do not host.
- `GET /cluster/capabilities` is unauthenticated.

## To do
- Parse `ClusterPeers` from the Subscribe response and use it as the reconnect candidate list.
- Honour `Retry-After` on 503 and fail over on 400 `realtime_transport_not_supported`.
- Surface leader/peer info as events; unit tests with captured responses.
