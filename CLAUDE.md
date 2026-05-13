# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

> **Primary instruction source:** `AGENTS.md` at repo root — read it first. This file extends it with Claude Code–specific notes.

## Commands

```powershell
# Always run before build/test (downloads shared props into .shared-props/)
dotnet restore

# Build
dotnet build -c Release -p:Platform=AnyCPU   # AnyCPU only
dotnet build -c Release                       # all configured platforms (net8.0, net9.0, net10.0)

# Test
dotnet test Tests/UnitTests/UnitTests.csproj
dotnet test Tests/ArchTests/ArchTests.csproj

# Clean (removes .shared-props/ — must restore again afterward)
dotnet clean
```

## Architecture

Three strict layers — never skip or cross them:

```
Application
  ↓
ThunderPropagatorClient          # protocol selection, channel factory, payload routing
  ↓
AbstractThunderPropagatorChannel # request/response tracking, auth encryption, decryption, subscriptions
  ↓
AbstractThunderPropagatorConnection<T> # transport lifecycle, receive loop
```

Protocol implementations live in `Connections/`, `Channels/`, `Clients/`. Abstractions live exclusively in `Infrastructure/`.

**Payload routing in `ThunderPropagatorClient`:**
- JSON payloads → `HandleReceivedResponse`
- Non-JSON → regex split `<channel>,<message>` → `HandleReceivedMessageAsync`
- First non-`PROBE` received frame is always treated as `ThunderPropagatorConnectionResponse` (connection info), before normal dispatch begins.

**Three protocol implementations:** WebSocket, QUIC (runtime-gated via `QuicConnection.IsSupported`), InfiniteDataStream.

## Key conventions

- **Logging:** Use `Infrastructure/Loggers/ILogger.cs` and `ILoggerProvider.cs` — never `Microsoft.Extensions.Logging`.
- **JSON:** Use `ToNJson()` / `FromNJson<T>()` from BuildingBlocks — no ad-hoc serializers.
- **Configuration:** Use `Get<T>()` / `Set()` property pattern from `AbstractThunderPropagatorConfiguration` — no private backing fields.
- **Sealing:** Concrete protocol/client/channel classes use `#if !DEBUG sealed #endif` — never add `sealed` without this pattern.
- **Naming:** Architecture tests enforce `ThunderPropagator*` prefix for all public types (narrow exceptions: `*Helper`, `*Descriptor`, `*Metadata`). Violations will fail `ArchTests`.
- **Shared props:** `Directory.Build.props` fetches `ThunderPropagator.SharedBuild` at restore time. Build fails fast with cascading type errors if `.shared-props/` is missing — always restore first.

## Reference docs

- `docs/README.md` — top-level design intent  
- `docs/Clients/README.md`, `docs/Channels/README.md`, `docs/Connections/README.md`, `docs/Infrastructure/README.md`, `docs/Models/README.md`  
- `Tests/ArchTests/ArchitectureTests.cs` — enforced naming and dependency rules
