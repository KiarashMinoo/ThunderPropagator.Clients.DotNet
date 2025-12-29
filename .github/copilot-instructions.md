# ThunderPropagator .NET Client Library - AI Coding Agent Guide

## Project Overview
Real-time data streaming client library for .NET 8/9/10 supporting WebSocket, QUIC, and InfiniteDataStream protocols. Distributed as multi-platform NuGet packages (ARM64, x64, x86, AnyCPU).

**Current Version**: `1.0.1-beta.14`  
**ThunderPropagator Framework Version**: `1.0.1-beta.15`  
**BuildingBlocks Version**: `1.0.1-beta.14`

## Core Architecture

### Three-Layer Pattern
All protocol implementations follow this hierarchy:
```
ThunderPropagatorClient (protocol-agnostic facade)
├── Connection Layer (AbstractThunderPropagatorConnection<T>)
│   ├── ThunderPropagatorWebSocketConnection
│   ├── ThunderPropagatorQuicConnection
│   └── ThunderPropagatorInfiniteDataStreamConnection
├── Channel Layer (AbstractThunderPropagatorChannel)
│   ├── ThunderPropagatorWebSocketChannel
│   ├── ThunderPropagatorQuicChannel
│   └── ThunderPropagatorInfiniteDataStreamChannel
└── Client Layer (protocol-specific facades)
    ├── ThunderPropagatorWebSocketClient
    ├── ThunderPropagatorQuicClient
    └── ThunderPropagatorInfiniteDataStreamClient
```

- **Connection**: Manages protocol-specific transport, connection state, message receipt
- **Channel**: Handles logical communication channels, subscriptions, encryption, metadata
- **Client**: Protocol-specific entry point, delegates to `ThunderPropagatorClient` base

### Key Design Patterns

**Configuration inheritance**: All configurations extend `AbstractThunderPropagatorConfiguration` (itself extends `ServiceConfiguration` from BuildingBlocks). Use getter/setter pattern with `Get<T>()` and `Set()` methods (see [ThunderPropagatorWebSocketConnectionConfiguration.cs](Connections/WebSocket/ThunderPropagatorWebSocketConnectionConfiguration.cs)).

**Conditional sealing**: Classes use `#if !DEBUG sealed #endif` to allow inheritance in debug builds only (e.g., [CipheringMetadata.cs](Models/CipheringMetadata.cs), all Client classes).

**Abstract internal implementations**: Core logic in `AbstractThunderPropagatorConnection<T>` and `AbstractThunderPropagatorChannel` marked `internal abstract`, with public interfaces exposed via `IThunderPropagatorConnection` and `IThunderPropagatorChannel`.

**INotifyPropertyChanged**: All major components implement this with `SetField<T>` helper for property change notifications.

## Dependencies

### ThunderPropagator.BuildingBlocks
Core dependency providing foundational utilities:
- `ServiceConfiguration`: Base for all configuration classes with `Get<T>()`/`Set()` pattern
- `DisposableObject`: Base for disposable resources with sync/async disposal
- `BindingDictionary<TKey, TValue>`: Thread-safe dictionary implementation
- `FeederMessage`: Dictionary-based message abstraction with correlation ID
- `Telemetry`: OpenTelemetry integration (Activities, Counters, Histograms)
- `ToNJson()`/`FromNJson<T>()`: JSON serialization helpers (uses Newtonsoft.Json)
- Ciphering utilities: AES, RSA encryption/decryption
- Serialization helpers: JSON, YAML, ProtoBuf, MessagePack support

**Platform-specific packaging**: Debug builds reference `ThunderPropagator.BuildingBlocks.Debug[.Platform]`, release builds reference `ThunderPropagator.BuildingBlocks[.Platform]` (see [ThunderPropagator.Clients.DotNet.csproj](ThunderPropagator.Clients.DotNet.csproj)).

### NuGet Source
Uses GitHub Packages feed `https://nuget.pkg.github.com/KiarashMinoo/index.json` for ThunderPropagator packages. Configured in [nuget.config](nuget.config).

## Build Configuration

### Multi-Platform/Multi-Framework
- Target frameworks: `net8.0`, `net9.0`, `net10.0`
- Platforms: AnyCPU, x86, x64, ARM64
- Configurations: Debug, Release

**NuGet package naming**:
- Debug AnyCPU: `ThunderPropagator.Clients.DotNet.Debug`
- Debug platform-specific: `ThunderPropagator.Clients.DotNet.Debug.{Platform}`
- Release AnyCPU: `ThunderPropagator.Clients.DotNet`
- Release platform-specific: `ThunderPropagator.Clients.DotNet.{Platform}`

### Preview Features
- `EnablePreviewFeatures=true`
- `RequiresPreviewFeatures` assembly attribute ([AssemblyInfo.cs](AssemblyInfo.cs))
- `LangVersion=latestmajor`

## Critical Workflows

### Building
```powershell
dotnet restore
dotnet build -c Release -p:Platform=AnyCPU
# Or build all platforms
dotnet build -c Release
```

### Testing Connection Protocols
Must instantiate protocol-specific client with corresponding configuration:
```csharp
var config = new ThunderPropagatorWebSocketConnectionConfiguration { Uri = "wss://..." };
var client = new ThunderPropagatorWebSocketClient(config, loggerProvider);
await client.ConnectAsync();
var channel = await client.CreateChannelAsync("channelName");
```

### Message Flow
1. Connection receives raw message → `OnMessageReceived`
2. First non-PROBE message is `ThunderPropagatorConnectionResponse` (sets `ConnectionInfo`)
3. Subsequent messages routed to channels by name via regex parsing:
   - JSON format: `{ "route": { "channel": "..." } }` → `HandleReceivedResponse`
   - CSV format: `channelName,data,...` → `HandleReceivedMessageAsync`

## Project Conventions

### Logging
Custom lightweight logging abstraction in `Infrastructure/Loggers/`. **Do not use Microsoft.Extensions.Logging**. Use `ILoggerProvider.CreateLogger()` and `ILogger.Log()`.

### Enums
All enums in `Models/Enums/` with `ThunderPropagator` prefix. Key states:
- `ThunderPropagatorConnectionState`: Ready, Connecting, Open, Closed, HasError
- `ThunderPropagatorChannelState`: Ready, Opening, Open, Closing, Closed
- `ThunderPropagatorProtocolType`: WebSocket, Quic, InfiniteDataStream

### Event Handlers
Typed delegates for all events (not generic `EventHandler<T>`):
- `ThunderPropagatorConnectionStateChangedEventHandler`
- `ThunderPropagatorChannelStatusChangedEventHandler`
- `ThunderPropagatorMessageReceivedEventHandler` (async Task-based)

### Message Parsing
Use compiled regex for performance:
- `[GeneratedRegex(..., RegexOptions.Compiled)]` partial static methods
- See [ThunderPropagatorClient.cs](ThunderPropagatorClient.cs) line 27, [AbstractThunderPropagatorChannel.cs](Infrastructure/Channels/AbstractThunderPropagatorChannel.cs) line 47

## Important Implementation Details

### Connection Lifecycle
Connections auto-reconnect with 24-hour timeout on receive loop (see [AbstractThunderPropagatorConnection.cs](Infrastructure/Connections/AbstractThunderPropagatorConnection.cs) lines 96-102). PROBE messages are filtered out in `OnMessageReceived`.

### Channel Encryption
Channels support two encryption layers:
1. **Auth encryption**: Username/password encrypted before transmission
2. **Message decryption**: Incoming messages decrypted based on `ChannelMetadata.MessageEncryption`

Built dynamically via `BuildAuthEncryptor()` and `BuildMessageDecryptor()` when metadata received.

### Request/Response Tracking
Channels maintain `BindingDictionary` of pending requests keyed by request ID. Use `ThunderPropagatorRequestBase` for all requests with `ThunderPropagatorRequestRoute` containing channel/endpoint info.

## Anti-Patterns
- ❌ Don't use `sealed` without `#if !DEBUG` conditional
- ❌ Don't bypass `Get<T>()`/`Set()` in configuration classes
- ❌ Don't reference Microsoft.Extensions.Logging types
- ❌ Don't create platform-specific conditional compilation beyond existing patterns
- ❌ Don't manually manage JSON serialization—use `ToNJson()`/`FromNJson<T>()` extensions
