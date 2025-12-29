# Models / ReceivedMessage

## Contents

- [Overview](#overview)
- [Files](#files)
- [Key Types](#key-types)
- [See Also](#see-also)

## Overview

Structures for messages received from channels including headers and data payloads.

## Files

| File | Primary Type | LOC (approx) | Responsibility |
|------|-------------|--------------|----------------|
| ChannelReceivedMessage.cs | ChannelReceivedMessage | ~30 | Complete received message |
| ChannelReceivedMessageHeader.cs | ChannelReceivedMessageHeader | ~20 | Message header information |

## Key Types

### ChannelReceivedMessage

**Namespace**: `ThunderPropagator.Clients.DotNet.Models.ReceivedMessage`

Complete message received from a channel.

**Key Properties**:

- `Header: ChannelReceivedMessageHeader` — Message metadata
- `Data: string` — Message payload (JSON/CSV)
- `Timestamp: DateTime` — Receive timestamp

### ChannelReceivedMessageHeader

**Namespace**: `ThunderPropagator.Clients.DotNet.Models.ReceivedMessage`

Message header with routing and metadata.

**Key Properties**:

- `Channel: string` — Source channel
- `MessageType: string` — Type of message
- `SequenceNumber: long?` — Optional sequence number
- `CorrelationId: string?` — Request correlation

[↑ Back to top](#contents)

---

## See Also

- [Models](../README.md) — Parent models overview
- [Infrastructure/Channels](../../Infrastructure/Channels/README.md) — Channels producing received messages

[↑ Back to top](#contents)
