---
title: Wire format drift: client expects JSON handshake/responses and sends JSON requests; server uses Key=Value text
labels: bug,protocol,priority:P0
---

## Summary
The client assumes JSON on the control plane. The current server (ThunderPropagator core, post `ThunderPropagator#102`, 2026-05-21) emits and parses `Key=Value` text. Against the current server the first frame cannot be deserialized and requests cannot be parsed.

## Evidence (server)
- Handshake is `new ConnectionPushingMessage(null, connectionInfo, ...)` -> lines `Key=Value` joined by newline (`ThunderPropagator.Application/PushModules/Messages/ConnectionPushingMessage.cs`, enqueued in `AbstractConnectionContainer.cs:257`). Keys: `ConnectionId`, `ClientIPAddress`, `ClientPort`, `ConnectionDateTime`, `IsAvailable`, `User`.
- Responses are pushed the same way (`ChannelManager.Receive.cs` ~L292): `ConnectionId`, `RequestId`, `Route`, `ResponseCode`, `ContentType`, `ResponseContent` (serialized body string).
- Requests are parsed by `RequestContentFormCollection.Parse`: split on newline or `;`, then `key=value` (`RequestId`, `Route=Channel/RequestType[/k=v][?q]`, `Content-Type`, `Accept`, `FormData=<payload>`, extra keys PascalCased). A JSON body has no `RequestId=` key and is rejected.
- Before #102 the handshake/response messages were JSON (`ConnectionPushingJsonMessage`); the Key=Value request parser predates that.

## Evidence (client)
- `AbstractThunderPropagatorConnection.OnMessageReceived`: `message.FromNJson<ThunderPropagatorConnectionResponse>()` on the first non-PROBE frame.
- `ThunderPropagatorClient.ThunderPropagatorConnectionOnMessageReceived`: routes frames starting with `{` to `HandleReceivedResponse`.
- `AbstractThunderPropagatorChannel.RequestAsync`: `ThunderPropagatorConnection.SendAsync(request.ToNJson())`.

## To do
1. Reproduce against a current server build (WebSocket + IDS) and confirm. Note: the server's own `wwwroot/ChannelUI.js` also still speaks JSON, so first decide which side is the intended contract (server fix vs client fix).
2. If the server's Key=Value contract stands: implement a handshake parser (Key=Value), request writer (`RequestId`, `Route`, `Content-Type`, `Accept`, `FormData=<json>`, `Token`/`Username`/`Password`), and response parser (`ResponseCode`, `ResponseContent`); keep JSON as a compat path if the server still offers it.
3. Reconcile `ThunderPropagatorConnectionResponse` (it declares `PushMessageConfiguration`, which the server does not send) with the real handshake fields.
4. Add unit tests with captured real frames.

## Acceptance
- Client connects, reads `ConnectionId`/`IsAvailable`, fetches metadata, subscribes and receives updates against a current server on WebSocket and IDS.
- Spec §4/§7/§8 updated to match the decided contract (see Clients spec issue).
