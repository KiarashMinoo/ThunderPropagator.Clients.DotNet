# ThunderPropagator .NET Client Library Documentation

This is the comprehensive documentation for **ThunderPropagator.Clients.DotNet**, a real-time data streaming client library for .NET 8/9/10 supporting WebSocket, QUIC, and InfiniteDataStream protocols.

## Contents

- [Overview](#overview)
- [Architecture Areas](#architecture-areas)
- [Getting Started](#getting-started)
- [NuGet Packages](#nuget-packages)
- [Coverage Audit](#coverage-audit)

## Overview

ThunderPropagator.Clients.DotNet is a multi-protocol, multi-platform client library enabling real-time communication with ThunderPropagator servers. The library implements a three-layer architecture (Client → Channel → Connection) with support for:

- **Protocols**: WebSocket, QUIC, InfiniteDataStream
- **Frameworks**: .NET 8.0, 9.0, 10.0
- **Platforms**: AnyCPU, x86, x64, ARM64
- **Features**: Connection management, channel subscriptions, encryption, request/response tracking

The library is distributed as NuGet packages via GitHub Packages (`https://nuget.pkg.github.com/KiarashMinoo/index.json`).

**Current Version**: `1.0.1-beta.14`  
**ThunderPropagator Framework Version**: `1.0.1-beta.15`  
**BuildingBlocks Version**: `1.0.1-beta.14`

## Architecture Areas

The documentation is organized into the following major areas:

### [Clients](./Clients/README.md)
Protocol-specific client facades providing entry points for WebSocket, QUIC, and InfiniteDataStream connections. Each client delegates to the protocol-agnostic `ThunderPropagatorClient` base.

### [Channels](./Channels/README.md)
Protocol-specific channel implementations handling logical communication channels, subscriptions, encryption, and metadata management.

### [Connections](./Connections/README.md)
Transport layer implementations for each protocol (WebSocket, QUIC, InfiniteDataStream), managing connection state, message receipt, and protocol-specific behavior.

### [Infrastructure](./Infrastructure/README.md)
Core abstractions and base implementations including:
- Abstract connection and channel base classes
- Configuration patterns
- Logging abstraction
- Request/response infrastructure

### [Models](./Models/README.md)
Data models including:
- Enumerations (connection states, channel states, protocol types)
- Metadata structures (channel, authentication, encryption)
- Request/response models
- Subscription models
- Received message structures

## Getting Started

### Installation

Configure NuGet to use the GitHub Packages feed:

```bash
dotnet nuget add source https://nuget.pkg.github.com/KiarashMinoo/index.json \
  -n github \
  -u KiarashMinoo \
  -p <your-github-token> \
  --store-password-in-clear-text
```

Install the appropriate package:

```bash
# AnyCPU (Debug or Release)
dotnet add package ThunderPropagator.Clients.DotNet --version 1.0.1-beta.14

# Platform-specific (better performance)
dotnet add package ThunderPropagator.Clients.DotNet.x64 --version 1.0.1-beta.14
```

### Basic Usage

```csharp
using ThunderPropagator.Clients.DotNet;
using ThunderPropagator.Clients.DotNet.Clients;
using ThunderPropagator.Clients.DotNet.Connections.WebSocket;

// Create configuration
var config = new ThunderPropagatorWebSocketConnectionConfiguration 
{ 
    Uri = "wss://your-server.com/thunderpropagator" 
};

// Create client with logger provider
var client = new ThunderPropagatorWebSocketClient(config, loggerProvider);

// Connect
await client.ConnectAsync();

// Create channel
var channel = await client.CreateChannelAsync("myChannel");

// Subscribe to messages
channel.ReceivedMessage += (sender, message, args) => 
{
    Console.WriteLine($"Received: {message.Data}");
};

// Open channel
await channel.OpenAsync("username", "password");

// Subscribe to data stream
await channel.SubscribeAsync(new ThunderPropagatorSubscriptionRequest 
{ 
    // subscription details 
});
```

## NuGet Packages

ThunderPropagator packages are hosted on GitHub Packages:

| Package | Version | Description |
|---------|---------|-------------|
| ThunderPropagator.Clients.DotNet | 1.0.1-beta.14 | AnyCPU release build |
| ThunderPropagator.Clients.DotNet.Debug | 1.0.1-beta.14 | AnyCPU debug build |
| ThunderPropagator.Clients.DotNet.x64 | 1.0.1-beta.14 | x64 optimized release build |
| ThunderPropagator.Clients.DotNet.x86 | 1.0.1-beta.14 | x86 optimized release build |
| ThunderPropagator.Clients.DotNet.ARM64 | 1.0.1-beta.14 | ARM64 optimized release build |
| ThunderPropagator.BuildingBlocks | 1.0.1-beta.14 | Core utilities and abstractions |

**Feed URL**: `https://nuget.pkg.github.com/KiarashMinoo/index.json`

## Coverage Audit

Documentation coverage for ThunderPropagator.Clients.DotNet repository:

| Folder | Status | Sections | Diagrams | Notes |
|--------|--------|----------|----------|-------|
| Root | ✅ | Complete | ✓ | Core client facade |
| Clients/ | ✅ | Complete | ✓ | 3 files documented |
| Channels/ | ✅ | Complete | ✓ | 3 files documented |
| Connections/ | ✅ | Complete | ✓ | All protocols covered |
| Connections/WebSocket/ | ✅ | Complete | ✓ | 2 files documented |
| Connections/Quic/ | ✅ | Complete | ✓ | 2 files documented |
| Connections/InfiniteDataStream/ | ✅ | Complete | ✓ | 2 files documented |
| Infrastructure/ | ✅ | Complete | ✓ | Core abstractions |
| Infrastructure/Channels/ | ✅ | Complete | ✓ | 2 files documented |
| Infrastructure/Connections/ | ✅ | Complete | ✓ | 3 files documented |
| Infrastructure/Loggers/ | ✅ | Complete | ✗ | 4 files documented |
| Infrastructure/Requests/ | ✅ | Complete | ✗ | 2 files documented |
| Infrastructure/Responses/ | ✅ | Complete | ✗ | 1 file documented |
| Models/ | ✅ | Complete | ✓ | 1 file + 6 subfolders |
| Models/Enums/ | ✅ | Complete | ✗ | 9 enums documented |
| Models/Metadata/ | ✅ | Complete | ✓ | 7 files documented |
| Models/Connections/ | ✅ | Complete | ✗ | 2 files documented |
| Models/ReceivedMessage/ | ✅ | Complete | ✗ | 2 files documented |
| Models/Requests/ | ✅ | Complete | ✗ | 5 files documented |
| Models/Subscriptions/ | ✅ | Complete | ✓ | 5 files documented |

**Last generated:** December 29, 2025

---

[↑ Back to top](#contents)
