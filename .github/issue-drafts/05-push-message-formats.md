---
title: Support the server's push message formats (client only decodes the proprietary comma format)
labels: feature,protocol,priority:P1
---

## Summary
Since `ThunderPropagator#102` the server formats subscription pushes through a formatter registry selected by the server-side `SerializerType` of each transport configuration. The client only decodes the proprietary format (`channel,requestId,fromSnapshot,status,keys,values`), and nothing in the client sends or reads a format.

## Server facts
- `SerializerType` is **server config per transport**, default NJson (`IPushMessageConfiguration`). No client negotiation (`PushMessageFormat`) exists server-side today.
- Formatters registered in the Web host: MessagePack, NetJson, Protobuf, Toon, Xml, Yaml; Json/NJson/Proprietary come from core.
- Structured payload (`SubscriptionMessagePayload`): `ChannelName`, `Header{RequestId, FromSnapshot, RecordStatus}`, `Keys{SubscriptionId}`, `NodeRowIds?[{Index,Value}]`, `Values[{Index,Value}]`; `RecordStatus` is the enum name (`Neutral|Added|Modified|Deleted`).
- Proprietary (type 0, `application/tpg`): `ChannelName,<enc(RequestId,fromSnapshot 0/1,status N|A|M|D,SubscriptionId,[idx=val;...],]idx=val;idx=val)>`; may be split into several frames at `MaxPushSize`.
- WebSocket sends everything as Text frames, so binary formatters are text-encoded on that transport.

## To do
- Add a pluggable push-message decoder; ship Json/NJson first (default), keep Proprietary, then Yaml/Xml; document MessagePack/Protobuf/Toon as unsupported until the server's text-encoding is defined.
- Add a client option for the expected format (and detect it from the first message if possible) until the server advertises it in the handshake.
- Verify the subscription id/RequestId mapping: Subscribe responses do not return `SubscriptionId`; it is learned from pushes.
