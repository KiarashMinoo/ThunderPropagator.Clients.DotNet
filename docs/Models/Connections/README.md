# Models / Connections

## Contents

- [Overview](#overview)
- [Files](#files)
- [Key Types](#key-types)
- [See Also](#see-also)

## Overview

Connection-related models including server connection responses and push message configuration.

## Files

| File | Primary Type | LOC (approx) | Responsibility |
|------|-------------|--------------|----------------|
| ThunderPropagatorConnectionResponse.cs | ThunderPropagatorConnectionResponse | ~20 | Server connection details |
| ThunderPropagatorPushMessageConfiguration.cs | ThunderPropagatorPushMessageConfiguration | ~15 | Push notification config |

## Key Types

### ThunderPropagatorConnectionResponse

**Namespace**: `ThunderPropagator.Clients.DotNet.Models.Connections`

First message received from server after connection establishment.

**Key Properties**:

- `ConnectionId: string` — Unique connection identifier
- `ServerVersion: string` — Server version info
- `Capabilities: string[]` — Supported server capabilities
- `Timestamp: DateTime` — Connection timestamp

### ThunderPropagatorPushMessageConfiguration

**Namespace**: `ThunderPropagator.Clients.DotNet.Models.Connections`

Configuration for push message behavior.

**Key Properties**:

- `Enabled: bool` — Whether push is enabled
- `QueueSize: int` — Max queue size
- `BatchSize: int` — Messages per batch

[↑ Back to top](#contents)

---

## See Also

- [Models](../README.md) — Parent models overview
- [Infrastructure/Connections](../../Infrastructure/Connections/README.md) — Connections using these models

[↑ Back to top](#contents)
