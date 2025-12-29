# RapidStreamer .NET Client Library - AI Coding Agent Guide

## Project Overview
Real-time data streaming client library for .NET 8/9 supporting WebSocket, QUIC, and InfiniteDataStream protocols. Distributed as multi-platform NuGet packages (ARM64, x64, x86, AnyCPU).

## Core Architecture

### Three-Layer Pattern
All protocol implementations follow this hierarchy:
```
RapidStreamerClient (protocol-agnostic facade)
├── Connection Layer (AbstractRapidStreamerConnection<T>)
│   ├── RapidStreamerWebSocketConnection
│   ├── RapidStreamerQuicConnection
│   └── RapidStreamerInfiniteDataStreamConnection
├── Channel Layer (AbstractRapidStreamerChannel)
│   ├── RapidStreamerWebSocketChannel
│   ├── RapidStreamerQuicChannel
│   └── RapidStreamerInfiniteDataStreamChannel
└── Client Layer (protocol-specific facades)
    ├── RapidStreamerWebSocketClient
    ├── RapidStreamerQuicClient
    └── RapidStreamerInfiniteDataStreamClient
```

- **Connection**: Manages protocol-specific transport, connection state, message receipt
- **Channel**: Handles logical communication channels, subscriptions, encryption, metadata
- **Client**: Protocol-specific entry point, delegates to `RapidStreamerClient` base

### Key Design Patterns

**Configuration inheritance**: All configurations extend `AbstractRapidStreamerConfiguration` (itself extends `ServiceConfiguration` from BuildingBlocks). Use getter/setter pattern with `Get<T>()` and `Set()` methods (see [RapidStreamerWebSocketConnectionConfiguration.cs](Connections/WebSocket/RapidStreamerWebSocketConnectionConfiguration.cs)).

**Conditional sealing**: Classes use `#if !DEBUG sealed #endif` to allow inheritance in debug builds only (e.g., [CipheringMetadata.cs](Models/CipheringMetadata.cs), all Client classes).

**Abstract internal implementations**: Core logic in `AbstractRapidStreamerConnection<T>` and `AbstractRapidStreamerChannel` marked `internal abstract`, with public interfaces exposed via `IRapidStreamerConnection` and `IRapidStreamerChannel`.

**INotifyPropertyChanged**: All major components implement this with `SetField<T>` helper for property change notifications.

## Dependencies

### RapidStreamer.BuildingBlocks
Core dependency providing foundational utilities:
- `ServiceConfiguration`: Base for all configuration classes
- `DisposableObject`: Base for disposable resources
- `BindingDictionary<TKey, TValue>`: Thread-safe dictionary
- `ToNJson()`/`FromNJson<T>()`: JSON serialization helpers (uses Newtonsoft.Json)
- Ciphering utilities: AES, RSA encryption/decryption

**Platform-specific packaging**: Debug builds reference `RapidStreamer.BuildingBlocks.Debug[.Platform]`, release builds reference `RapidStreamer.BuildingBlocks[.Platform]` (see [RapidStreamer.Clients.DotNet.csproj](RapidStreamer.Clients.DotNet.csproj) lines 47-58).

### Custom NuGet Source
Uses private feed `https://nuget.rapidstreamer.com/v3/index.json` for RapidStreamer packages. Configured in [nuget.config](nuget.config) with GitHub fallback for legacy ThunderPropagator packages.

## Build Configuration

### Multi-Platform/Multi-Framework
- Target frameworks: `net8.0`, `net9.0`
- Platforms: AnyCPU, x86, x64, ARM64
- Configurations: Debug, Release

**NuGet package naming**:
- Debug AnyCPU: `RapidStreamer.Clients.DotNet.Debug`
- Debug platform-specific: `RapidStreamer.Clients.DotNet.Debug.{Platform}`
- Release AnyCPU: `RapidStreamer.Clients.DotNet`
- Release platform-specific: `RapidStreamer.Clients.DotNet.{Platform}`

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
var config = new RapidStreamerWebSocketConnectionConfiguration { Uri = "wss://..." };
var client = new RapidStreamerWebSocketClient(config, loggerProvider);
await client.ConnectAsync();
var channel = await client.CreateChannelAsync("channelName");
```

### Message Flow
1. Connection receives raw message → `OnMessageReceived`
2. First non-PROBE message is `RapidStreamerConnectionResponse` (sets `ConnectionInfo`)
3. Subsequent messages routed to channels by name via regex parsing:
   - JSON format: `{ "route": { "channel": "..." } }` → `HandleReceivedResponse`
   - CSV format: `channelName,data,...` → `HandleReceivedMessageAsync`

## Project Conventions

### Logging
Custom lightweight logging abstraction in `Infrastructure/Loggers/`. **Do not use Microsoft.Extensions.Logging**. Use `ILoggerProvider.CreateLogger()` and `ILogger.Log()`.

### Enums
All enums in `Models/Enums/` with `RapidStreamer` prefix. Key states:
- `RapidStreamerConnectionState`: Ready, Connecting, Open, Closed, HasError
- `RapidStreamerChannelState`: Ready, Opening, Open, Closing, Closed
- `RapidStreamerProtocolType`: WebSocket, Quic, InfiniteDataStream

### Event Handlers
Typed delegates for all events (not generic `EventHandler<T>`):
- `RapidStreamerConnectionStateChangedEventHandler`
- `RapidStreamerChannelStatusChangedEventHandler`
- `RapidStreamerMessageReceivedEventHandler` (async Task-based)

### Message Parsing
Use compiled regex for performance:
- `[GeneratedRegex(..., RegexOptions.Compiled)]` partial static methods
- See [RapidStreamerClient.cs](RapidStreamerClient.cs) line 27, [AbstractRapidStreamerChannel.cs](Infrastructure/Channels/AbstractRapidStreamerChannel.cs) line 47

## Important Implementation Details

### Connection Lifecycle
Connections auto-reconnect with 24-hour timeout on receive loop (see [AbstractRapidStreamerConnection.cs](Infrastructure/Connections/AbstractRapidStreamerConnection.cs) lines 96-102). PROBE messages are filtered out in `OnMessageReceived`.

### Channel Encryption
Channels support two encryption layers:
1. **Auth encryption**: Username/password encrypted before transmission
2. **Message decryption**: Incoming messages decrypted based on `ChannelMetadata.MessageEncryption`

Built dynamically via `BuildAuthEncryptor()` and `BuildMessageDecryptor()` when metadata received.

### Request/Response Tracking
Channels maintain `BindingDictionary` of pending requests keyed by request ID. Use `RapidStreamerRequestBase` for all requests with `RapidStreamerRequestRoute` containing channel/endpoint info.

## Anti-Patterns
- ❌ Don't use `sealed` without `#if !DEBUG` conditional
- ❌ Don't bypass `Get<T>()`/`Set()` in configuration classes
- ❌ Don't reference Microsoft.Extensions.Logging types
- ❌ Don't create platform-specific conditional compilation beyond existing patterns
- ❌ Don't manually manage JSON serialization—use `ToNJson()`/`FromNJson<T>()` extensions
