# ThunderPropagator Client Libraries - Implementation Instructions

This directory contains comprehensive documentation for implementing ThunderPropagator client libraries across multiple programming languages.

## Contents

- **[schema.md](schema.md)** - Complete architectural schema and design patterns
- **[python-implementation-prompt.md](python-implementation-prompt.md)** - Python implementation guide
- **[javascript-implementation-prompt.md](javascript-implementation-prompt.md)** - JavaScript/TypeScript implementation guide  
- **[java-implementation-prompt.md](java-implementation-prompt.md)** - Java implementation guide
- **[rust-implementation-prompt.md](rust-implementation-prompt.md)** - Rust implementation guide
- **[go-implementation-prompt.md](go-implementation-prompt.md)** - Go implementation guide
- **[cpp-implementation-prompt.md](cpp-implementation-prompt.md)** - C++ implementation guide

## Purpose

These documents serve as:
1. **Architecture Reference** - Detailed schema of the three-layer pattern (Connection/Channel/Client)
2. **Implementation Prompts** - Language-specific guides for AI coding agents or developers
3. **Cross-Language Consistency** - Ensuring all client libraries follow the same design patterns

## Quick Start

### For Developers
1. Read [schema.md](schema.md) to understand the core architecture
2. Select your target language's implementation guide
3. Follow the patterns and conventions outlined for consistent behavior

### For AI Coding Agents
Each language-specific prompt file contains:
- Complete project structure
- Package/dependency management
- Core design patterns adapted to language idioms
- Protocol implementations (WebSocket, QUIC, InfiniteDataStream)
- Testing strategies
- Build and distribution guidelines

## Core Concepts

All ThunderPropagator client libraries share:

### Three-Layer Architecture
```
Client Layer → Channel Layer → Connection Layer
     ↓              ↓                ↓
  Facade      Subscriptions    Protocol Transport
```

### Supported Protocols
- **WebSocket** - Standard real-time communication
- **QUIC** - Modern UDP-based protocol with multiplexing
- **InfiniteDataStream** - Custom high-throughput streaming

### Key Features
- Multi-protocol support
- Channel-based communication
- Two-layer encryption (auth + message)
- Auto-reconnection
- Request/response tracking
- Property change notifications

## Version Information

Current .NET Implementation:
- **Version**: 1.0.1-beta.14
- **ThunderPropagator Framework**: 1.0.1-beta.15
- **Target Frameworks**: .NET 8.0, 9.0, 10.0

## Contributing

When implementing for a new language:
1. Follow the architectural schema strictly
2. Adapt design patterns to language idioms
3. Maintain feature parity across protocols
4. Include comprehensive tests
5. Document language-specific quirks

## License

Refer to the main project LICENSE file.
