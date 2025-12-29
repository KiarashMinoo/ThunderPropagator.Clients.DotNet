# Infrastructure / Responses

## Contents

- [Overview](#overview)
- [Files](#files)
- [Types & Members](#types--members)
- [See Also](#see-also)

## Overview

Base response class for handling server responses to client requests.

## Files

| File | Primary Type | LOC (approx) | Responsibility |
|------|-------------|--------------|----------------|
| ThunderPropagatorResponseBase.cs | ThunderPropagatorResponseBase | ~20 | Abstract base for all responses |

## Types & Members

### ThunderPropagatorResponseBase

**Kind**: Abstract class (public)  
**Namespace**: `ThunderPropagator.Clients.DotNet.Infrastructure.Responses`

Base class for all ThunderPropagator server responses.

**Key Properties**:

- `RequestId: string` — Corresponds to originating request
- `Success: bool` — Indicates operation success
- `ErrorMessage: string?` — Error details if unsuccessful
- `Data: object?` — Response payload

Responses are matched to pending requests via `RequestId` for request/response correlation.

[↑ Back to top](#contents)

---

## See Also

- [Infrastructure/Requests](../Requests/README.md) — Request base classes
- [Infrastructure](../README.md) — Parent infrastructure overview

[↑ Back to top](#contents)
