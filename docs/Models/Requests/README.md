# Models / Requests

## Contents

- [Overview](#overview)
- [Files](#files)
- [Key Types](#key-types)
- [See Also](#see-also)

## Overview

Concrete request implementations for channel operations including metadata requests, subscriptions, pings, unsubscribe, and a helper for generating request IDs.

All request types inherit from `ThunderPropagatorRequestBase` in Infrastructure.

## Files

| File | Primary Type | LOC (approx) | Responsibility |
|------|-------------|--------------|----------------|
| ThunderPropagatorMetadataRequest.cs | ThunderPropagatorMetadataRequest | ~15 | Request channel metadata |
| ThunderPropagatorSubscriptionRequest.cs | ThunderPropagatorSubscriptionRequest | ~40 | Subscribe to data stream |
| ThunderPropagatorUnsubscribeRequest.cs | ThunderPropagatorUnsubscribeRequest | ~15 | Unsubscribe from data |
| ThunderPropagatorPingRequest.cs | ThunderPropagatorPingRequest | ~10 | Health check ping |
| RequestIdHelper.cs | RequestIdHelper | ~10 | Generate unique request IDs |

## Key Types

### ThunderPropagatorMetadataRequest

**Namespace**: `ThunderPropagator.Clients.DotNet.Models.Requests`  
**Inherits**: `ThunderPropagatorRequestBase`

Requests channel metadata from the server.

**Usage**:

```csharp
var request = new ThunderPropagatorMetadataRequest
{
    Route = new ThunderPropagatorRequestRoute 
    { 
        Channel = "myChannel", 
        Endpoint = "metadata" 
    }
};
```

### ThunderPropagatorSubscriptionRequest

**Namespace**: `ThunderPropagator.Clients.DotNet.Models.Requests`  
**Inherits**: `ThunderPropagatorRequestBase`

Subscribes to a data stream with specified keys and fields.

**Key Properties**:

- `SubscribingKeys: IReadOnlyDictionary<string, string>` — Key-value pairs for subscription
- `SubscribingFields: IReadOnlyCollection<string>` — Fields to receive
- `SubscriptionMode: ThunderPropagatorSubscriptionMode` — Full/Delta/Snapshot

**Usage**:

```csharp
var request = new ThunderPropagatorSubscriptionRequest
{
    Route = new ThunderPropagatorRequestRoute { Channel = "trades", Endpoint = "subscribe" },
    SubscribingKeys = new Dictionary<string, string> { ["symbol"] = "AAPL" },
    SubscribingFields = new[] { "price", "volume", "timestamp" },
    SubscriptionMode = ThunderPropagatorSubscriptionMode.Full
};
```

### ThunderPropagatorUnsubscribeRequest

**Namespace**: `ThunderPropagator.Clients.DotNet.Models.Requests`  
**Inherits**: `ThunderPropagatorRequestBase`

Unsubscribes from a data stream.

**Key Properties**:

- `SubscriptionId: string` — ID of subscription to cancel

### ThunderPropagatorPingRequest

**Namespace**: `ThunderPropagator.Clients.DotNet.Models.Requests`  
**Inherits**: `ThunderPropagatorRequestBase`

Health check request to verify channel connectivity.

### RequestIdHelper

**Namespace**: `ThunderPropagator.Clients.DotNet.Models.Requests`

Helper class for generating unique request identifiers.

**Key Methods**:

```csharp
public static string GenerateId()
```

Generates a unique request ID (typically GUID-based).

[↑ Back to top](#contents)

---

## See Also

- [Infrastructure/Requests](../../Infrastructure/Requests/README.md) — Base request class
- [Models](../README.md) — Parent models overview

[↑ Back to top](#contents)
