# ThunderPropagator.Clients.Net

**ThunderPropagator** is a cutting-edge software solution designed to redefine real-time data streaming. Our mission is to provide **effortless, blazingly fast, and cloud-native streaming capabilities** for maximum impact. This repository contains the foundational libraries, **ThunderPropagator.Providers.DotNet.ActiveMQ**, **ThunderPropagator.Providers.DotNet.Kafka**, **ThunderPropagator.Providers.DotNet.Mqtt**, **ThunderPropagator.Providers.DotNet.NATS**, **ThunderPropagator.Providers.DotNet.Pulsar**, **ThunderPropagator.Providers.DotNet.RabbitMQ**, **ThunderPropagator.Providers.DotNet.RedisPubSub**, **ThunderPropagator.Providers.DotNet.TcpSocket**, **ThunderPropagator.Providers.DotNet.UdpClient**, **ThunderPropagator.Providers.DotNet.WebApi** and **ThunderPropagator.Providers.DotNet.WebSocket**, which empower developers to build scalable, high-performance streaming applications with ease.

These libraries support **.NET 9** and **.NET 8**, and are configured to work across multiple platforms, including **ARM64**, **x64**, **x86**, and **AnyCPU**. They are available as **NuGet packages** and can be installed from the custom NuGet repository:
**`https://nuget.thunderpropagator.com/v3/index.json`**.

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Supported Platforms](#supported-platforms)
- [Documentation](#documentation)
- [Installation](#installation)
- [License](#license)

---

## Overview

ThunderPropagator is designed to revolutionize real-time data streaming by providing:

- **Effortless Integration**: Simple and intuitive APIs for seamless integration into your applications.
- **Blazingly Fast Performance**: Optimized for low-latency, high-throughput streaming.
- **Cloud-Native Architecture**: Built for modern cloud environments, enabling scalability and resilience.
- **Cross-Platform Support**: Compatible with ARM64, x64, x86, and AnyCPU platforms.

Whether you're building real-time analytics, live event processing, or IoT data pipelines, ThunderPropagator empowers you to deliver maximum impact with minimal effort.

---

## Features

- **Cross-Platform Support**: Works seamlessly on ARM64, x64, x86, and AnyCPU platforms.
- **.NET Compatibility**: Fully compatible with .NET 9 and .NET 8.
- **Debug and Release Configurations**: Pre-configured for both debug and release builds.
- **High Performance**: Optimized for low-latency, high-throughput streaming.
- **Cloud-Native**: Designed for modern cloud environments with built-in scalability and resilience.
- **NuGet Packages**: Easily installable via a custom NuGet repository.

---

## Supported Platforms

The projects support the following platforms:

- **ARM64**
- **x64**
- **x86**
- **AnyCPU**

Both **Debug** and **Release** configurations are available for all platforms.

---

## Documentation

Comprehensive documentation for the ThunderPropagator.Clients.DotNet library is available under [`/docs`](docs/README.md). The documentation provides detailed API references, usage examples, diagrams, and architecture guides.

### Documentation Catalog

- **[Root / ThunderPropagatorClient](docs/README.md)** `Types:1` `Files:1` `Diagrams:✓`
  - Protocol-agnostic client facade implementing three-layer architecture
  
- **[Clients](docs/Clients/README.md)** `Types:3` `Files:3` `Diagrams:✓`
  - Protocol-specific client facades (WebSocket, QUIC, InfiniteDataStream)
  
- **[Channels](docs/Channels/README.md)** `Types:3` `Files:3` `Diagrams:✓`
  - Logical communication channels with subscriptions and encryption
  
- **[Connections](docs/Connections/README.md)** `Types:6` `Files:6` `Diagrams:✓`
  - Transport layer implementations for each protocol
  - [WebSocket](docs/Connections/WebSocket/README.md) `Types:2` `Files:2` `Diagrams:✓`
  - [Quic](docs/Connections/Quic/README.md) `Types:2` `Files:2` `Diagrams:✓`
  - [InfiniteDataStream](docs/Connections/InfiniteDataStream/README.md) `Types:2` `Files:2` `Diagrams:✓`
  
- **[Infrastructure](docs/Infrastructure/README.md)** `Types:12` `Files:12` `Diagrams:✓`
  - Core abstractions and base implementations
  - [Channels](docs/Infrastructure/Channels/README.md) `Types:2` `Files:2` `Diagrams:✓`
  - [Connections](docs/Infrastructure/Connections/README.md) `Types:3` `Files:3` `Diagrams:✓`
  - [Loggers](docs/Infrastructure/Loggers/README.md) `Types:4` `Files:4` `Diagrams:✗`
  - [Requests](docs/Infrastructure/Requests/README.md) `Types:2` `Files:2` `Diagrams:✗`
  - [Responses](docs/Infrastructure/Responses/README.md) `Types:1` `Files:1` `Diagrams:✗`
  
- **[Models](docs/Models/README.md)** `Types:36` `Files:36` `Diagrams:✓`
  - Data models, enumerations, and structures
  - [Enums](docs/Models/Enums/README.md) `Types:10` `Files:10` `Diagrams:✗`
  - [Metadata](docs/Models/Metadata/README.md) `Types:7` `Files:7` `Diagrams:✓`
  - [Connections](docs/Models/Connections/README.md) `Types:2` `Files:2` `Diagrams:✗`
  - [ReceivedMessage](docs/Models/ReceivedMessage/README.md) `Types:2` `Files:2` `Diagrams:✗`
  - [Requests](docs/Models/Requests/README.md) `Types:5` `Files:5` `Diagrams:✗`
  - [Subscriptions](docs/Models/Subscriptions/README.md) `Types:5` `Files:5` `Diagrams:✓`

**Last generated:** December 29, 2025

---

## Installation

### Step 1: Add the Custom NuGet Repository
To install the libraries as NuGet packages, you need to add the custom NuGet repository to your NuGet configuration.

#### Using Visual Studio:
1. Open Visual Studio.
2. Go to **Tools** > **NuGet Package Manager** > **Package Manager Settings**.
3. Under **Package Sources**, click the **+** button to add a new source.
4. Enter the following details:
  - **Name**: `ThunderPropagator`
  - **Source**: `https://nuget.thunderpropagator.com/v3/index.json`
5. Click **Update** and then **OK**.

#### Using the Command Line:
Add the NuGet source using the following command:
```bash
dotnet nuget add source --name ThunderPropagator --source https://nuget.thunderpropagator.com/v3/index.json
```

#### Create or Update `nuget.config`
If you don’t already have a `nuget.config` file in your project or solution directory, create one. If you do, update it to include the custom repository.

Here’s an example of what the `nuget.config` file should look like:
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <!-- Add the official NuGet.org source -->
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <!-- Add the custom ThunderPropagator NuGet repository -->
    <add key="ThunderPropagator" value="https://nuget.thunderpropagator.com/v3/index.json" />
  </packageSources>
</configuration>
```

Place the `nuget.config` file in the root of your solution or project directory. This ensures that all projects in the solution can access the custom NuGet repository.

### Step 2: Verify the Configuration

To verify that the custom repository is correctly configured, you can use the following command in the terminal:
```bash
dotnet nuget list source
```
This will list all configured NuGet sources. You should see something like this in the output:
```text
Registered Sources:
  1.  nuget.org [Enabled]
      https://api.nuget.org/v3/index.json
  2.  ThunderPropagator [Enabled]
      https://nuget.thunderpropagator.com/v3/index.json
```

### Step 3: Install the NuGet Packages
You can now install the packages using the following commands:

```bash
dotnet add package ThunderPropagator.Clients.DotNet
```

Alternatively, you can install the packages via the NuGet Package Manager in Visual Studio.

## License
This project is licensed under the **MIT License**.

© 2024 ThunderPropagator. All rights reserved.