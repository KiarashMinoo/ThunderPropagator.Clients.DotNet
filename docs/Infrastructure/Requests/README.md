# Infrastructure / Requests

## Contents

- [Overview](#overview)
- [Files](#files)
- [Types & Members](#types--members)
- [See Also](#see-also)

## Overview

Base request classes and routing structures for channel operations including metadata requests, subscriptions, pings, and custom requests.

## Files

| File | Primary Type | LOC (approx) | Responsibility |
|------|-------------|--------------|----------------|
| ThunderPropagatorRequestBase.cs | ThunderPropagatorRequestBase | ~30 | Abstract base for all requests |
| ThunderPropagatorRequestRoute.cs | ThunderPropagatorRequestRoute | ~20 | Request routing information |

## Types & Members

### ThunderPropagatorRequestBase

**Kind**: Abstract class (public)  
**Namespace**: `ThunderPropagator.Clients.DotNet.Infrastructure.Requests`

Base class for all ThunderPropagator requests.

**Key Properties**:

- `RequestId: string` — Unique request identifier
- `Route: ThunderPropagatorRequestRoute` — Routing information (channel, endpoint)
- `CorrelationId: string?` — Optional correlation tracking

**Key Methods**:

```csharp
public ManualResetEvent SetAwaitable()
```

Makes request awaitable by returning a `ManualResetEvent` for synchronization.

[↑ Back to top](#contents)

---

### ThunderPropagatorRequestRoute

**Kind**: Class (public)  
**Namespace**: `ThunderPropagator.Clients.DotNet.Infrastructure.Requests`

Contains routing information for requests.

**Key Properties**:

- `Channel: string` — Target channel name
- `Endpoint: string` — Target endpoint within channel
- `Action: string?` — Optional action specifier

[↑ Back to top](#contents)

---

## See Also

- [Models/Requests](../../Models/Requests/README.md) — Concrete request implementations
- [Infrastructure](../README.md) — Parent infrastructure overview

[↑ Back to top](#contents)
