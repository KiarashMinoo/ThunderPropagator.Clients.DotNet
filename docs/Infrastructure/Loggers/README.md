# Infrastructure / Loggers

## Contents

- [Overview](#overview)
- [Files](#files)
- [Types & Members](#types--members)
- [See Also](#see-also)

## Overview

Custom lightweight logging abstraction providing `ILogger`, `ILoggerProvider`, `LogLevel`, and `EventId` types. **This is NOT Microsoft.Extensions.Logging**. The library uses its own logging interfaces to avoid external logging dependencies.

## Files

| File | Primary Type | LOC (approx) | Responsibility |
|------|-------------|--------------|----------------|
| ILogger.cs | ILogger | ~15 | Logger interface with Log methods |
| ILoggerProvider.cs | ILoggerProvider | ~10 | Logger factory interface |
| LogLevel.cs | LogLevel | ~20 | Log level enumeration |
| EventId.cs | EventId | ~10 | Event identifier structure |

## Types & Members

### ILogger

**Kind**: Interface (public)  
**Namespace**: `ThunderPropagator.Clients.DotNet.Infrastructure.Loggers`

Defines logging operations.

**Key Methods**:

```csharp
void Log(LogLevel logLevel, Exception? exception, string? message, params object?[] args)
```

Logs a message with level, optional exception, and format args.

[↑ Back to top](#contents)

---

### ILoggerProvider

**Kind**: Interface (public)  
**Namespace**: `ThunderPropagator.Clients.DotNet.Infrastructure.Loggers`

Logger factory for creating logger instances.

**Key Methods**:

```csharp
ILogger CreateLogger(string categoryName)
```

Creates a logger for the specified category (typically class name).

[↑ Back to top](#contents)

---

### LogLevel

**Kind**: Enum (public)  
**Namespace**: `ThunderPropagator.Clients.DotNet.Infrastructure.Loggers`

Log severity levels:

- `Trace` — Most detailed
- `Debug` — Debug information
- `Information` — Informational messages
- `Warning` — Warnings
- `Error` — Errors
- `Critical` — Critical failures
- `None` — Logging disabled

[↑ Back to top](#contents)

---

### EventId

**Kind**: Struct (public)  
**Namespace**: `ThunderPropagator.Clients.DotNet.Infrastructure.Loggers`

Identifier for log events. Contains `Id` (int) and optional `Name` (string).

[↑ Back to top](#contents)

---

## See Also

- [Infrastructure](../README.md) — Parent infrastructure overview

[↑ Back to top](#contents)
