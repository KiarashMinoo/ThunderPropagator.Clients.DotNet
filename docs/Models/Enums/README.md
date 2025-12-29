# Models / Enums

## Contents

- [Overview](#overview)
- [Files](#files)
- [Enumeration Types](#enumeration-types)
- [See Also](#see-also)

## Overview

Enumeration types defining connection states, channel states, protocol types, authentication types, subscription modes, and other operational states used throughout the library.

All enums follow the naming convention `ThunderPropagator*` prefix.

## Files

| File | Primary Type | Values | Responsibility |
|------|-------------|--------|----------------|
| ThunderPropagatorConnectionState.cs | ThunderPropagatorConnectionState | 5 | Connection lifecycle states |
| ThunderPropagatorChannelState.cs | ThunderPropagatorChannelState | 8 | Channel lifecycle states |
| ThunderPropagatorProtocolType.cs | ThunderPropagatorProtocolType | 3 | Supported protocol types |
| ThunderPropagatorChannelAuthenticationType.cs | ThunderPropagatorChannelAuthenticationType | 3 | Authentication methods |
| ThunderPropagatorChannelFieldType.cs | ThunderPropagatorChannelFieldType | ~5 | Field data types |
| ThunderPropagatorChannelStorageType.cs | ThunderPropagatorChannelStorageType | ~3 | Storage backend types |
| ThunderPropagatorPingPongState.cs | ThunderPropagatorPingPongState | 3 | Health check states |
| ThunderPropagatorRecordStatus.cs | ThunderPropagatorRecordStatus | ~4 | Record status types |
| ThunderPropagatorSubscriptionMode.cs | ThunderPropagatorSubscriptionMode | 3 | Subscription delivery modes |
| ThunderPropagatorSubscriptionStatus.cs | ThunderPropagatorSubscriptionStatus | ~4 | Subscription states |

## Enumeration Types

### ThunderPropagatorConnectionState

Defines connection lifecycle states:

- `Ready` — Connection created, not yet connected
- `Connecting` — Connection in progress
- `Open` — Connection established and active
- `Closed` — Connection closed normally
- `HasError` — Connection encountered error

### ThunderPropagatorChannelState

Defines channel lifecycle states:

- `Ready` — Channel created
- `RequestingMetadata` — Fetching channel metadata
- `HasMetadata` — Metadata received
- `Opening` — Authentication in progress
- `Open` — Channel ready for operations
- `Closing` — Channel closing
- `Closed` — Channel closed
- `HasError` — Channel error state

### ThunderPropagatorProtocolType

Supported transport protocols:

- `WebSocket` — Standard WebSocket protocol
- `Quic` — QUIC (HTTP/3) protocol
- `InfiniteDataStream` — HTTP-based streaming protocol

### ThunderPropagatorChannelAuthenticationType

Authentication methods for channels:

- `None` — No authentication
- `Basic` — Username/password authentication
- `OAuth2` — OAuth2 token authentication

### ThunderPropagatorPingPongState

Health check states:

- `NotPinged` — No ping sent yet
- `Pinged` — Ping sent, awaiting pong
- `Ponged` — Pong received, healthy

### ThunderPropagatorSubscriptionMode

Subscription data delivery modes:

- `Full` — Full record updates
- `Delta` — Only changed fields
- `Snapshot` — Snapshot + incremental updates

### ThunderPropagatorSubscriptionStatus

Subscription lifecycle states:

- `Pending` — Subscription requested
- `Active` — Subscription active, receiving data
- `Paused` — Subscription paused
- `Cancelled` — Subscription cancelled

### ThunderPropagatorChannelFieldType

Field data types in channel schemas:

- `String` — Text field
- `Integer` — Integer number
- `Decimal` — Decimal number
- `Boolean` — Boolean flag
- `DateTime` — Date/time value

### ThunderPropagatorChannelStorageType

Storage backend types:

- `Memory` — In-memory storage
- `Redis` — Redis backend
- `Database` — Relational database

### ThunderPropagatorRecordStatus

Record status in subscriptions:

- `Active` — Record is active
- `Deleted` — Record deleted
- `Updated` — Record updated
- `Inserted` — Record inserted

[↑ Back to top](#contents)

---

## See Also

- [Models](../README.md) — Parent models overview
- [Infrastructure/Connections](../../Infrastructure/Connections/README.md) — Using connection states
- [Infrastructure/Channels](../../Infrastructure/Channels/README.md) — Using channel states

[↑ Back to top](#contents)
