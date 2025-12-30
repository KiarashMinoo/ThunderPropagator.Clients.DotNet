# ThunderPropagator Client Library - Architectural Schema

## Overview

ThunderPropagator is a real-time data streaming framework supporting multiple transport protocols. This document defines the canonical architecture that all client library implementations must follow.

## Core Principles

### 1. Protocol Agnosticism
The client API should be consistent regardless of underlying transport (WebSocket, QUIC, InfiniteDataStream).

### 2. Layered Architecture
Three distinct layers with clear separation of concerns:
- **Connection Layer**: Protocol-specific transport
- **Channel Layer**: Logical communication channels
- **Client Layer**: User-facing API facade

### 3. Type Safety
Strong typing for configurations, messages, events, and state transitions.

### 4. Async/Await Pattern
All I/O operations are asynchronous.

### 5. Observable State
Components expose state changes via events/observers.

---

## Architecture Layers

### Layer 1: Connection Layer

**Responsibility**: Manage protocol-specific transport, connection lifecycle, raw message receipt.

#### Abstract Base: `AbstractConnection<TConfig>`

**Properties**:
- `ConnectionId: string` - Unique connection identifier
- `State: ConnectionState` - Current connection state
- `ConnectionInfo: ConnectionInfo` - Metadata from server
- `Configuration: TConfig` - Protocol-specific configuration
- `IsConnected: bool` - Quick state check

**Methods**:
- `ConnectAsync(): Task` - Establish connection
- `DisconnectAsync(): Task` - Close connection gracefully
- `SendAsync(message: string): Task` - Send raw message
- `DisposeAsync(): ValueTask` - Cleanup resources

**Events**:
- `StateChanged(oldState, newState)` - Connection state transitions
- `MessageReceived(message: string)` - Raw message received
- `ErrorOccurred(error)` - Connection errors

**State Machine**:
```
Ready → Connecting → Open → Closing → Closed
                      ↓
                   HasError
```

#### Protocol Implementations

**WebSocketConnection**:
- Uses native WebSocket client
- Supports WSS (TLS)
- Configurable headers, timeouts

**QuicConnection**:
- Uses QUIC protocol (HTTP/3)
- Multiplexed streams
- Built-in encryption

**InfiniteDataStreamConnection**:
- Custom protocol for high-throughput
- Chunked transfer
- Compression support

---

### Layer 2: Channel Layer

**Responsibility**: Logical communication channels within a connection, subscriptions, encryption, metadata.

#### Abstract Base: `AbstractChannel`

**Properties**:
- `ChannelId: string` - Unique channel identifier
- `Name: string` - Channel name
- `State: ChannelState` - Current channel state
- `Metadata: ChannelMetadata` - Server-provided metadata
- `Connection: IConnection` - Parent connection
- `IsOpen: bool` - Quick state check

**Methods**:
- `OpenAsync(): Task` - Open channel
- `CloseAsync(): Task` - Close channel
- `SubscribeAsync(subscription): Task` - Subscribe to data
- `UnsubscribeAsync(subscriptionId): Task` - Unsubscribe
- `SendRequestAsync(request): Task<Response>` - Send request, await response
- `DisposeAsync(): ValueTask` - Cleanup

**Events**:
- `StateChanged(oldState, newState)` - Channel state transitions
- `MessageReceived(message)` - Typed message received
- `MetadataReceived(metadata)` - Channel metadata from server
- `ErrorOccurred(error)` - Channel errors

**State Machine**:
```
Ready → Opening → Open → Closing → Closed
```

#### Key Concepts

**Subscriptions**:
- Represent data streams within a channel
- Include filters, parameters
- Server assigns subscription ID
- Can be paused/resumed

**Encryption**:
- **Auth Encryption**: Username/password encrypted during handshake
- **Message Encryption**: AES or RSA for message payloads
- Configured via `ChannelMetadata.MessageEncryption`

**Request/Response**:
- Channels track pending requests by correlation ID
- Responses matched to requests via ID
- Timeout handling (default 30s)

---

### Layer 3: Client Layer

**Responsibility**: Protocol-specific facade, delegates to `ThunderPropagatorClient` base.

#### Base Client

**Properties**:
- `ProtocolType: ProtocolType` - WebSocket, QUIC, or InfiniteDataStream
- `Connection: IConnection` - Underlying connection
- `Channels: Dictionary<string, IChannel>` - Open channels

**Methods**:
- `ConnectAsync(): Task` - Connect to server
- `DisconnectAsync(): Task` - Disconnect from server
- `CreateChannelAsync(name): Task<IChannel>` - Create/open channel
- `GetChannelAsync(name): IChannel?` - Get existing channel
- `DisposeAsync(): ValueTask` - Cleanup

**Events**:
- `Connected()` - Connection established
- `Disconnected()` - Connection closed
- `ChannelCreated(channel)` - New channel opened
- `ErrorOccurred(error)` - Client-level errors

#### Protocol-Specific Clients

**WebSocketClient**:
- Configuration: `WebSocketConnectionConfiguration`
  - `Uri: string` (required)
  - `Headers: Dictionary<string, string>`
  - `ConnectionTimeout: TimeSpan`
  - `KeepAliveInterval: TimeSpan`

**QuicClient**:
- Configuration: `QuicConnectionConfiguration`
  - `Host: string` (required)
  - `Port: int` (required)
  - `MaxBidirectionalStreams: int`
  - `MaxUnidirectionalStreams: int`
  - `ClientCertificate: X509Certificate?`

**InfiniteDataStreamClient**:
- Configuration: `InfiniteDataStreamConnectionConfiguration`
  - `Host: string` (required)
  - `Port: int` (required)
  - `CompressionEnabled: bool`
  - `ChunkSize: int`

---

## Data Models

### Configuration Hierarchy

```
AbstractConfiguration (base)
├── Get<T>(key): T
├── Set(key, value): void
└── Protocol-specific fields
```

All configurations:
- Extend base configuration class
- Use getter/setter pattern for extensibility
- Validate required fields
- Provide sensible defaults

### Messages

**FeederMessage**:
- Dictionary-based structure
- Correlation ID for tracking
- Route information (channel, endpoint)
- Payload data

**ConnectionResponse**:
- Connection ID
- Server version
- Capabilities
- Session info

**ChannelMetadata**:
- Encryption settings
- Available endpoints
- Channel capabilities
- Custom properties

### Subscriptions

**SubscriptionRequest**:
- Channel name
- Data type/filter
- Parameters
- Priority

**SubscriptionResponse**:
- Subscription ID
- Status (active, pending, rejected)
- Error message (if rejected)

---

## Message Flow

### Connection Handshake
```
1. Client → ConnectAsync()
2. Connection establishes transport
3. Server → ConnectionResponse
4. Client stores ConnectionInfo
5. Event: Connected
```

### Channel Creation
```
1. Client → CreateChannelAsync(name)
2. Channel → OpenAsync()
3. Connection → Send channel open request
4. Server → ChannelMetadata
5. Channel stores metadata
6. Channel state = Open
7. Event: ChannelCreated
```

### Message Routing
```
1. Connection receives raw message
2. Parse message format:
   - JSON: Extract route.channel
   - CSV: First field is channel name
3. Route to appropriate channel
4. Channel decrypts if needed
5. Channel parses typed message
6. Event: MessageReceived on channel
```

### Request/Response Pattern
```
1. Client → SendRequestAsync(request)
2. Generate correlation ID
3. Store in pending requests map
4. Send request via connection
5. Wait for response (async)
6. Server → Response with matching ID
7. Match response to request
8. Remove from pending map
9. Return response to caller
```

---

## Error Handling

### Connection Errors
- Network failures → Auto-reconnect (exponential backoff)
- Authentication errors → ErrorOccurred event, no reconnect
- Protocol errors → Close connection, report error

### Channel Errors
- Channel not found → Throw exception
- Encryption failure → Report to channel error handler
- Timeout → Cancel request, notify caller

### Message Errors
- Parse failure → Log error, discard message
- Routing failure → Log warning
- Validation failure → Notify sender

---

## Logging Strategy

### Custom Logging Abstraction
Do not depend on framework-specific logging (e.g., Microsoft.Extensions.Logging, java.util.logging).

**Interface**:
- `Log(level, message, exception?)` - Log entry
- `IsEnabled(level): bool` - Check if level is enabled

**Levels**:
- Trace, Debug, Information, Warning, Error, Critical

**Usage**:
- Connection lifecycle events → Information
- Message send/receive → Debug/Trace
- Errors → Error/Critical
- State transitions → Information

---

## Testing Strategy

### Unit Tests
- Configuration validation
- State machine transitions
- Message parsing/routing
- Encryption/decryption
- Request/response matching

### Integration Tests
- Connect to test server
- Create channels
- Send/receive messages
- Subscriptions
- Error scenarios

### Protocol Tests
- WebSocket handshake
- QUIC stream management
- InfiniteDataStream chunking
- Connection resilience
- Auto-reconnect

---

## Platform Considerations

### .NET
- Multi-target: net8.0, net9.0, net10.0
- Multi-platform: AnyCPU, x86, x64, ARM64
- Use `INotifyPropertyChanged` for observability
- Conditional sealing (`#if !DEBUG sealed`)

### Python
- Type hints (Python 3.10+)
- Async/await with asyncio
- dataclasses for models
- Protocol implementations as subclasses

### JavaScript/TypeScript
- TypeScript for type safety
- Promises/async-await
- EventEmitter for events
- WebSocket native support
- QUIC via libraries (e.g., @fails-components/webtransport)

### Java
- Java 17+ for modern APIs
- Virtual threads (Java 21+) for async
- Interfaces for abstractions
- Records for immutable models
- Builder pattern for configurations

---

## Versioning

### Semantic Versioning
- **Major**: Breaking API changes
- **Minor**: New features, backward compatible
- **Patch**: Bug fixes

### Protocol Version
- Client reports version in handshake
- Server validates compatibility
- Graceful degradation if possible

### Package Naming
- Language-specific conventions
- Include "ThunderPropagator" prefix
- Separate debug/release packages (optional)

---

## Distribution

### .NET
- NuGet packages
- Platform-specific variants
- GitHub Packages + NuGet.org

### Python
- PyPI packages
- Wheels for multiple platforms
- Source distribution

### JavaScript/TypeScript
- npm packages
- Bundled for browser
- ES modules + CommonJS

### Java
- Maven Central
- Gradle/Maven dependencies
- Fat JAR option

---

## Security Considerations

### Transport Security
- WebSocket: WSS (TLS 1.2+)
- QUIC: Built-in TLS 1.3
- InfiniteDataStream: Custom TLS wrapper

### Authentication
- Username/password encrypted with server public key
- Token-based auth support
- Certificate-based auth (QUIC)

### Message Encryption
- AES-256 for symmetric encryption
- RSA for asymmetric encryption
- Key exchange via channel metadata

---

## Performance Considerations

### Connection Pooling
- Reuse connections when possible
- Connection limits per host

### Message Batching
- Batch small messages
- Configurable batch size/timeout

### Memory Management
- Dispose resources properly
- Limit message queue sizes
- Stream large payloads

### Threading
- Async I/O throughout
- Non-blocking operations
- Thread-safe collections

---

## Extensibility Points

### Custom Protocols
- Implement `IConnection` interface
- Provide protocol-specific configuration
- Register with client factory

### Custom Serialization
- Pluggable serializers (JSON, MsgPack, Protobuf)
- Configure per channel or globally

### Custom Encryption
- Implement encryption interface
- Register custom algorithms
- Configure via metadata

### Middleware/Interceptors
- Pre-send message transformation
- Post-receive message processing
- Request/response interception

---

## Migration Guide

When updating major versions:
1. Review breaking changes
2. Update configuration objects
3. Handle new events
4. Test state transitions
5. Validate message formats
