# AGENTS.md

## What this repo is
- `ThunderPropagator.Clients.DotNet` is a multi-protocol .NET client library built around a strict 3-layer flow: **Client -> Channel -> Connection**.
- The canonical implementation entrypoint is `ThunderPropagatorClient` (`ThunderPropagatorClient.cs`), with thin protocol facades under `Clients/`.

## Architecture map (read this first)
- **Client layer**: protocol selection + channel factory (`ThunderPropagatorClient.cs`).
- **Channel layer**: request/response tracking, auth encryption, message decryption, subscriptions (`Infrastructure/Channels/AbstractThunderPropagatorChannel.cs`).
- **Connection layer**: transport lifecycle and receive loop (`Infrastructure/Connections/AbstractThunderPropagatorConnection.cs`).
- Protocol implementations:
  - WebSocket: `Connections/WebSocket/ThunderPropagatorWebSocketConnection.cs`
  - QUIC: `Connections/Quic/ThunderPropagatorQuicConnection.cs`
  - InfiniteDataStream: `Connections/InfiniteDataStream/ThunderPropagatorInfiniteDataStreamConnection.cs`

## Critical runtime behavior
- First non-`PROBE` received frame is treated as connection info and deserialized to `ThunderPropagatorConnectionResponse` before normal message dispatch.
- `ThunderPropagatorClient` routes incoming payloads by channel key:
  - JSON payloads -> `HandleReceivedResponse`
  - non-JSON payloads -> regex split `<channel>,<message>` then `HandleReceivedMessageAsync`
- Channel state transitions are evented (`ChannelStatusChanged`, `MetadataUpdated`, `ReceivedMessage`) and should be preserved when extending behavior.

## Build/test workflow specifics
- Always run `dotnet restore` first: `Directory.Build.props` auto-downloads required shared props into `.shared-props/` and build fails fast if missing.
- Main library targets `net8.0;net9.0;net10.0` (`Directory.Build.props`); tests target `net10.0` (`Tests/*/*.csproj`).
- Build commands:
  - `dotnet build -c Release -p:Platform=AnyCPU`
  - `dotnet build -c Release` (all configured platforms)
- Use explicit test project runs:
  - `dotnet test Tests/UnitTests/UnitTests.csproj`
  - `dotnet test Tests/ArchTests/ArchTests.csproj`
- `dotnet clean` removes `.shared-props/`; next restore re-downloads it.

## High-impact pitfalls
- If restore cannot download shared props from `ThunderPropagator.SharedBuild`, compile will fail with cascading type-not-found errors. Check `.shared-props/` and rerun `dotnet restore`.
- Do not introduce `Microsoft.Extensions.Logging` dependencies in library code; use `Infrastructure/Loggers/ILogger.cs` and `Infrastructure/Loggers/ILoggerProvider.cs`.
- Keep JSON serialization on BuildingBlocks helpers (`ToNJson()` / `FromNJson<T>()`), not ad-hoc serializers.
- Keep first non-`PROBE` frame handling unchanged: it is treated as `ThunderPropagatorConnectionResponse` before channel dispatch.
- QUIC support is runtime-dependent (`QuicConnection.IsSupported`), so avoid assumptions in tests and examples.

## Anti-patterns
- Avoid bypassing the `Get/Set` configuration property pattern with private backing fields.
- Avoid adding `sealed` without existing `#if !DEBUG` conditional pattern for protocol/client/channel classes.
- Avoid placing protocol-specific concrete types under `Infrastructure/`.

## Project-specific conventions to follow
- Public type naming is enforced by architecture tests (`Tests/ArchTests/ArchitectureTests.cs`): `ThunderPropagator*` (with narrow exceptions).
- Keep abstractions in `Infrastructure/*`; concrete protocol types stay in `Connections/*`, `Channels/*`, `Clients/*`.
- Most concrete protocol classes use conditional sealing (`#if !DEBUG sealed #endif`) to allow debug-time extensibility.
- Connection/configuration classes derive from `AbstractThunderPropagatorConfiguration` and use `ServiceConfiguration` `Get/Set` property pattern (not backing fields).
- Logging uses repo-local interfaces (`Infrastructure/Loggers/ILogger.cs`, `Infrastructure/Loggers/ILoggerProvider.cs`), not `Microsoft.Extensions.Logging` directly.

## Integration points and dependencies
- Core dependency is dynamically named ThunderPropagator BuildingBlocks package via `$(BuildingBlocksPackageId)` / `$(BuildingBlocksVersion)` in `ThunderPropagator.Clients.DotNet.csproj`.
- Shared build logic is fetched from `ThunderPropagator.SharedBuild` at restore/build time (`Directory.Build.props`).
- QUIC transport requires runtime support (`QuicConnection.IsSupported`) and TLS 1.3/libmsquic availability.

## Where to pull deeper context
- Start with `docs/README.md`, then `docs/Clients/README.md`, `docs/Channels/README.md`, and `docs/Connections/README.md` for repo-aligned design intent and examples.
- For architecture constraints and naming rules, check `Tests/ArchTests/ArchitectureTests.cs`.

