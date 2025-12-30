# ThunderPropagator Rust Client - Implementation Prompt

## Project Goal
Create a Rust client library for ThunderPropagator real-time data streaming framework supporting WebSocket, QUIC, and custom protocols.

## Target Environment
- **Rust Version**: 1.70+ (2021 edition)
- **Async Runtime**: Tokio
- **Package Distribution**: crates.io
- **Testing**: Built-in test framework + cargo-nextest

---

## Project Structure

```
thunderpropagator-rust/
├── Cargo.toml
├── README.md
├── LICENSE
├── CHANGELOG.md
├── .gitignore
├── src/
│   ├── lib.rs
│   ├── client.rs
│   ├── connections/
│   │   ├── mod.rs
│   │   ├── base.rs
│   │   ├── websocket.rs
│   │   ├── quic.rs
│   │   └── infinite_data_stream.rs
│   ├── channels/
│   │   ├── mod.rs
│   │   ├── base.rs
│   │   └── channel.rs
│   ├── models/
│   │   ├── mod.rs
│   │   ├── configuration.rs
│   │   ├── enums.rs
│   │   ├── messages.rs
│   │   ├── metadata.rs
│   │   └── subscriptions.rs
│   ├── crypto/
│   │   ├── mod.rs
│   │   └── encryption.rs
│   └── error.rs
├── tests/
│   ├── integration_test.rs
│   └── common/
│       └── mod.rs
├── examples/
│   ├── websocket_example.rs
│   ├── quic_example.rs
│   └── advanced_example.rs
└── benches/
    └── benchmarks.rs
```

---

## Core Dependencies

**Cargo.toml**:
```toml
[package]
name = "thunderpropagator-client"
version = "1.0.0"
edition = "2021"
rust-version = "1.70"
authors = ["Your Name <your.email@example.com>"]
license = "MIT"
description = "Real-time data streaming client for Rust"
homepage = "https://github.com/yourusername/thunderpropagator-rust"
repository = "https://github.com/yourusername/thunderpropagator-rust"
documentation = "https://docs.rs/thunderpropagator-client"
readme = "README.md"
keywords = ["websocket", "quic", "streaming", "real-time", "thunderpropagator"]
categories = ["network-programming", "asynchronous", "api-bindings"]
exclude = [".github/", "tests/", "benches/"]

[dependencies]
# Async runtime
tokio = { version = "1.35", features = ["full"] }
tokio-util = { version = "0.7", features = ["codec"] }
futures = "0.3"

# WebSocket
tokio-tungstenite = { version = "0.21", features = ["native-tls"] }

# QUIC
quinn = "0.10"
rustls = "0.21"

# HTTP client
reqwest = { version = "0.11", features = ["json"] }

# Serialization
serde = { version = "1.0", features = ["derive"] }
serde_json = "1.0"

# Error handling
thiserror = "1.0"
anyhow = "1.0"

# Logging
tracing = "0.1"
tracing-subscriber = "0.3"

# Cryptography
aes-gcm = "0.10"
rsa = "0.9"
rand = "0.8"

# Utilities
bytes = "1.5"
uuid = { version = "1.6", features = ["v4", "serde"] }
url = "2.5"

[dev-dependencies]
tokio-test = "0.4"
criterion = "0.5"
mockito = "1.2"

[profile.release]
opt-level = 3
lto = true
codegen-units = 1
```

---

## Design Patterns & Idioms

### 1. Trait-Based Abstraction

```rust
use async_trait::async_trait;
use std::sync::Arc;
use tokio::sync::RwLock;

#[async_trait]
pub trait Connection: Send + Sync {
    /// Establish connection to server
    async fn connect(&mut self) -> Result<(), ConnectionError>;
    
    /// Disconnect from server
    async fn disconnect(&mut self) -> Result<(), ConnectionError>;
    
    /// Send message
    async fn send(&self, message: &str) -> Result<(), ConnectionError>;
    
    /// Check if connected
    fn is_connected(&self) -> bool;
    
    /// Get connection ID
    fn connection_id(&self) -> Option<&str>;
}

pub struct AbstractConnection<T: ConnectionConfig> {
    config: T,
    state: Arc<RwLock<ConnectionState>>,
    connection_id: Option<String>,
    tx: Option<mpsc::Sender<String>>,
}
```

### 2. Enums and Pattern Matching

```rust
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
pub enum ConnectionState {
    Ready,
    Connecting,
    Open,
    Closing,
    Closed,
    HasError,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum ProtocolType {
    WebSocket,
    Quic,
    InfiniteDataStream,
}

impl ConnectionState {
    pub fn can_transition_to(&self, new_state: ConnectionState) -> bool {
        use ConnectionState::*;
        matches!(
            (self, new_state),
            (Ready, Connecting) | (Connecting, Open) | (Open, Closing) | (_, Closed) | (_, HasError)
        )
    }
}
```

### 3. Builder Pattern with Type State

```rust
pub struct WebSocketConfigBuilder {
    uri: Option<String>,
    headers: HashMap<String, String>,
    connection_timeout: Duration,
    keep_alive_interval: Duration,
}

impl WebSocketConfigBuilder {
    pub fn new() -> Self {
        Self {
            uri: None,
            headers: HashMap::new(),
            connection_timeout: Duration::from_secs(30),
            keep_alive_interval: Duration::from_secs(20),
        }
    }
    
    pub fn uri(mut self, uri: impl Into<String>) -> Self {
        self.uri = Some(uri.into());
        self
    }
    
    pub fn header(mut self, key: impl Into<String>, value: impl Into<String>) -> Self {
        self.headers.insert(key.into(), value.into());
        self
    }
    
    pub fn connection_timeout(mut self, timeout: Duration) -> Self {
        self.connection_timeout = timeout;
        self
    }
    
    pub fn build(self) -> Result<WebSocketConfig, ConfigError> {
        let uri = self.uri.ok_or(ConfigError::MissingUri)?;
        
        if !uri.starts_with("ws://") && !uri.starts_with("wss://") {
            return Err(ConfigError::InvalidUri("URI must start with ws:// or wss://".into()));
        }
        
        Ok(WebSocketConfig {
            uri,
            headers: self.headers,
            connection_timeout: self.connection_timeout,
            keep_alive_interval: self.keep_alive_interval,
        })
    }
}
```

### 4. Error Handling with thiserror

```rust
use thiserror::Error;

#[derive(Error, Debug)]
pub enum ConnectionError {
    #[error("Connection failed: {0}")]
    ConnectionFailed(String),
    
    #[error("Already connected")]
    AlreadyConnected,
    
    #[error("Not connected")]
    NotConnected,
    
    #[error("Send failed: {0}")]
    SendFailed(String),
    
    #[error("Receive failed: {0}")]
    ReceiveFailed(String),
    
    #[error("Timeout after {0:?}")]
    Timeout(Duration),
    
    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),
    
    #[error("WebSocket error: {0}")]
    WebSocket(#[from] tokio_tungstenite::tungstenite::Error),
}

pub type Result<T> = std::result::Result<T, ConnectionError>;
```

---

## Implementation Details

### WebSocket Connection

**src/connections/websocket.rs**:
```rust
use tokio_tungstenite::{connect_async, WebSocketStream, MaybeTlsStream};
use tokio::net::TcpStream;
use futures::{StreamExt, SinkExt};
use tracing::{info, error, debug};

pub struct WebSocketConnection {
    config: WebSocketConfig,
    state: Arc<RwLock<ConnectionState>>,
    ws_stream: Option<WebSocketStream<MaybeTlsStream<TcpStream>>>,
    message_tx: mpsc::UnboundedSender<String>,
    message_rx: Arc<RwLock<mpsc::UnboundedReceiver<String>>>,
}

impl WebSocketConnection {
    pub fn new(config: WebSocketConfig) -> Self {
        let (message_tx, message_rx) = mpsc::unbounded_channel();
        
        Self {
            config,
            state: Arc::new(RwLock::new(ConnectionState::Ready)),
            ws_stream: None,
            message_tx,
            message_rx: Arc::new(RwLock::new(message_rx)),
        }
    }
    
    pub async fn connect(&mut self) -> Result<()> {
        let mut state = self.state.write().await;
        if *state != ConnectionState::Ready {
            return Err(ConnectionError::AlreadyConnected);
        }
        *state = ConnectionState::Connecting;
        drop(state);
        
        info!("Connecting to {}", self.config.uri);
        
        let (ws_stream, _) = connect_async(&self.config.uri)
            .await
            .map_err(|e| ConnectionError::ConnectionFailed(e.to_string()))?;
        
        info!("WebSocket connected");
        
        self.ws_stream = Some(ws_stream);
        
        let mut state = self.state.write().await;
        *state = ConnectionState::Open;
        
        // Start receive loop
        self.start_receive_loop();
        
        Ok(())
    }
    
    fn start_receive_loop(&mut self) {
        let mut ws_stream = self.ws_stream.take().unwrap();
        let message_tx = self.message_tx.clone();
        let state = Arc::clone(&self.state);
        
        tokio::spawn(async move {
            while let Some(msg) = ws_stream.next().await {
                match msg {
                    Ok(Message::Text(text)) => {
                        if text.trim() != "PROBE" {
                            if let Err(e) = message_tx.send(text) {
                                error!("Failed to forward message: {}", e);
                                break;
                            }
                        }
                    }
                    Ok(Message::Close(_)) => {
                        info!("WebSocket closed by server");
                        break;
                    }
                    Err(e) => {
                        error!("WebSocket error: {}", e);
                        break;
                    }
                    _ => {}
                }
            }
            
            let mut state = state.write().await;
            *state = ConnectionState::Closed;
        });
    }
    
    pub async fn send(&self, message: &str) -> Result<()> {
        let state = self.state.read().await;
        if *state != ConnectionState::Open {
            return Err(ConnectionError::NotConnected);
        }
        drop(state);
        
        if let Some(ref ws_stream) = self.ws_stream {
            ws_stream.send(Message::Text(message.to_string()))
                .await
                .map_err(|e| ConnectionError::SendFailed(e.to_string()))?;
        }
        
        Ok(())
    }
    
    pub async fn receive(&self) -> Option<String> {
        let mut rx = self.message_rx.write().await;
        rx.recv().await
    }
}
```

### Channel Implementation

**src/channels/channel.rs**:
```rust
use std::collections::HashMap;
use tokio::sync::{RwLock, oneshot};
use uuid::Uuid;

pub struct Channel {
    name: String,
    state: Arc<RwLock<ChannelState>>,
    connection: Arc<dyn Connection>,
    metadata: Arc<RwLock<Option<ChannelMetadata>>>,
    pending_requests: Arc<RwLock<HashMap<Uuid, oneshot::Sender<serde_json::Value>>>>,
}

impl Channel {
    pub fn new(name: String, connection: Arc<dyn Connection>) -> Self {
        Self {
            name,
            state: Arc::new(RwLock::new(ChannelState::Ready)),
            connection,
            metadata: Arc::new(RwLock::new(None)),
            pending_requests: Arc::new(RwLock::new(HashMap::new())),
        }
    }
    
    pub async fn open(&self) -> Result<(), ChannelError> {
        let mut state = self.state.write().await;
        if *state != ChannelState::Ready {
            return Err(ChannelError::InvalidState(*state));
        }
        *state = ChannelState::Opening;
        drop(state);
        
        let open_request = serde_json::json!({
            "route": {
                "channel": self.name,
                "endpoint": "open"
            },
            "timestamp": chrono::Utc::now().to_rfc3339(),
        });
        
        self.connection.send(&open_request.to_string()).await?;
        
        // Wait for metadata
        tokio::time::timeout(
            Duration::from_secs(30),
            self.wait_for_metadata()
        ).await??;
        
        let mut state = self.state.write().await;
        *state = ChannelState::Open;
        
        Ok(())
    }
    
    pub async fn close(&self) -> Result<(), ChannelError> {
        let mut state = self.state.write().await;
        if *state == ChannelState::Closed {
            return Ok(());
        }
        *state = ChannelState::Closing;
        drop(state);
        
        let close_request = serde_json::json!({
            "route": {
                "channel": self.name,
                "endpoint": "close"
            }
        });
        
        self.connection.send(&close_request.to_string()).await?;
        
        let mut state = self.state.write().await;
        *state = ChannelState::Closed;
        
        Ok(())
    }
    
    pub async fn subscribe(&self, subscription: SubscriptionRequest) -> Result<SubscriptionResponse, ChannelError> {
        let request_id = Uuid::new_v4();
        let (tx, rx) = oneshot::channel();
        
        {
            let mut pending = self.pending_requests.write().await;
            pending.insert(request_id, tx);
        }
        
        let request = serde_json::json!({
            "id": request_id,
            "route": {
                "channel": self.name,
                "endpoint": "subscribe"
            },
            "payload": subscription
        });
        
        self.connection.send(&request.to_string()).await?;
        
        let response = tokio::time::timeout(Duration::from_secs(30), rx)
            .await
            .map_err(|_| ChannelError::Timeout)?
            .map_err(|_| ChannelError::RequestCancelled)?;
        
        serde_json::from_value(response)
            .map_err(|e| ChannelError::DeserializationFailed(e.to_string()))
    }
    
    pub async fn handle_message(&self, message: serde_json::Value) -> Result<(), ChannelError> {
        // Check if it's a response to pending request
        if let Some(id) = message.get("id").and_then(|v| v.as_str()) {
            if let Ok(uuid) = Uuid::parse_str(id) {
                let mut pending = self.pending_requests.write().await;
                if let Some(tx) = pending.remove(&uuid) {
                    let _ = tx.send(message["payload"].clone());
                    return Ok(());
                }
            }
        }
        
        // Check if it's metadata
        if let Some(metadata) = message.get("metadata") {
            let channel_metadata: ChannelMetadata = serde_json::from_value(metadata.clone())
                .map_err(|e| ChannelError::DeserializationFailed(e.to_string()))?;
            
            let mut meta = self.metadata.write().await;
            *meta = Some(channel_metadata);
            return Ok(());
        }
        
        // Regular message - emit event or store
        Ok(())
    }
    
    async fn wait_for_metadata(&self) -> Result<(), ChannelError> {
        loop {
            {
                let meta = self.metadata.read().await;
                if meta.is_some() {
                    return Ok(());
                }
            }
            tokio::time::sleep(Duration::from_millis(100)).await;
        }
    }
}
```

### Client Implementation

**src/client.rs**:
```rust
use std::collections::HashMap;
use std::sync::Arc;
use tokio::sync::RwLock;

pub struct ThunderPropagatorClient {
    connection: Arc<dyn Connection>,
    channels: Arc<RwLock<HashMap<String, Arc<Channel>>>>,
}

impl ThunderPropagatorClient {
    pub fn new(connection: Arc<dyn Connection>) -> Self {
        Self {
            connection,
            channels: Arc::new(RwLock::new(HashMap::new())),
        }
    }
    
    pub async fn connect(&self) -> Result<(), ConnectionError> {
        self.connection.connect().await
    }
    
    pub async fn disconnect(&self) -> Result<(), ConnectionError> {
        // Close all channels
        let channels = self.channels.read().await;
        for channel in channels.values() {
            let _ = channel.close().await;
        }
        drop(channels);
        
        self.connection.disconnect().await
    }
    
    pub async fn create_channel(&self, name: impl Into<String>) -> Result<Arc<Channel>, ChannelError> {
        let name = name.into();
        
        {
            let channels = self.channels.read().await;
            if let Some(channel) = channels.get(&name) {
                return Ok(Arc::clone(channel));
            }
        }
        
        let channel = Arc::new(Channel::new(name.clone(), Arc::clone(&self.connection)));
        channel.open().await?;
        
        let mut channels = self.channels.write().await;
        channels.insert(name, Arc::clone(&channel));
        
        Ok(channel)
    }
    
    pub async fn get_channel(&self, name: &str) -> Option<Arc<Channel>> {
        let channels = self.channels.read().await;
        channels.get(name).map(Arc::clone)
    }
}

pub struct WebSocketClient {
    client: ThunderPropagatorClient,
}

impl WebSocketClient {
    pub fn new(config: WebSocketConfig) -> Self {
        let connection = Arc::new(WebSocketConnection::new(config));
        let client = ThunderPropagatorClient::new(connection);
        
        Self { client }
    }
    
    pub async fn connect(&self) -> Result<(), ConnectionError> {
        self.client.connect().await
    }
    
    pub async fn create_channel(&self, name: impl Into<String>) -> Result<Arc<Channel>, ChannelError> {
        self.client.create_channel(name).await
    }
}
```

---

## Usage Example

```rust
use thunderpropagator_client::prelude::*;
use std::time::Duration;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    // Initialize tracing
    tracing_subscriber::fmt::init();
    
    // Configure client
    let config = WebSocketConfig::builder()
        .uri("wss://example.com/thunder")
        .header("Authorization", "Bearer token123")
        .connection_timeout(Duration::from_secs(30))
        .build()?;
    
    // Create client
    let client = WebSocketClient::new(config);
    
    // Connect
    client.connect().await?;
    
    // Create channel
    let channel = client.create_channel("market-data").await?;
    
    // Subscribe
    let subscription = SubscriptionRequest {
        data_type: "quotes".to_string(),
        symbols: vec!["AAPL".to_string(), "GOOGL".to_string()],
    };
    
    let response = channel.subscribe(subscription).await?;
    println!("Subscribed: {}", response.subscription_id);
    
    // Keep running
    tokio::time::sleep(Duration::from_secs(60)).await;
    
    Ok(())
}
```

---

## Testing

```rust
#[cfg(test)]
mod tests {
    use super::*;
    use tokio::test;
    
    #[test]
    async fn test_websocket_connection() {
        let config = WebSocketConfig::builder()
            .uri("wss://echo.websocket.org")
            .build()
            .unwrap();
        
        let mut connection = WebSocketConnection::new(config);
        
        connection.connect().await.unwrap();
        assert!(connection.is_connected());
        
        connection.send("test").await.unwrap();
        
        if let Some(msg) = connection.receive().await {
            assert_eq!(msg, "test");
        }
        
        connection.disconnect().await.unwrap();
    }
}
```

---

## Crates.io Publishing Guide

### 1. Prerequisites

```bash
# Install Rust
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh

# Install cargo tools
cargo install cargo-edit cargo-outdated cargo-audit

# Create crates.io account
# Visit https://crates.io/ and sign in with GitHub
```

### 2. Prepare Cargo.toml

Ensure all required fields are present:
- `name`, `version`, `edition`
- `authors`, `license`
- `description` (required for publishing)
- `homepage`, `repository`, `documentation`
- `readme`, `keywords`, `categories`

### 3. Add API Token

```bash
# Get token from https://crates.io/me
cargo login YOUR_API_TOKEN
```

### 4. Pre-Publishing Checks

```bash
# Format code
cargo fmt

# Run clippy
cargo clippy --all-targets --all-features -- -D warnings

# Run tests
cargo test --all-features

# Check documentation
cargo doc --no-deps --open

# Dry run publish
cargo publish --dry-run

# Check package contents
cargo package --list
```

### 5. Publishing

```bash
# Build and publish
cargo publish

# Check on crates.io
# https://crates.io/crates/thunderpropagator-client
```

### 6. Version Management

```bash
# Update version
cargo set-version 1.0.1

# Or manually edit Cargo.toml, then
git tag v1.0.1
git push origin v1.0.1
```

### 7. Automated Publishing with GitHub Actions

**.github/workflows/publish.yml**:
```yaml
name: Publish to crates.io

on:
  release:
    types: [published]

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Install Rust
        uses: dtolnay/rust-toolchain@stable
      
      - name: Cache cargo registry
        uses: actions/cache@v3
        with:
          path: ~/.cargo/registry
          key: ${{ runner.os }}-cargo-registry-${{ hashFiles('**/Cargo.lock') }}
      
      - name: Run tests
        run: cargo test --all-features
      
      - name: Publish to crates.io
        run: cargo publish --token ${{ secrets.CARGO_TOKEN }}
```

### 8. Documentation on docs.rs

docs.rs automatically builds documentation for all crates.io packages.

**Cargo.toml configuration**:
```toml
[package.metadata.docs.rs]
all-features = true
rustdoc-args = ["--cfg", "docsrs"]
```

### 9. Badges

**README.md**:
```markdown
[![Crates.io](https://img.shields.io/crates/v/thunderpropagator-client.svg)](https://crates.io/crates/thunderpropagator-client)
[![Documentation](https://docs.rs/thunderpropagator-client/badge.svg)](https://docs.rs/thunderpropagator-client)
[![License](https://img.shields.io/crates/l/thunderpropagator-client.svg)](https://github.com/yourusername/thunderpropagator-rust/blob/main/LICENSE)
[![Downloads](https://img.shields.io/crates/d/thunderpropagator-client.svg)](https://crates.io/crates/thunderpropagator-client)
```

---

## Key Rust-Specific Considerations

1. **Ownership & Borrowing**: Use `Arc<RwLock<T>>` for shared mutable state
2. **Async/Await**: Tokio runtime for async operations
3. **Error Handling**: `thiserror` for custom errors, `anyhow` for application errors
4. **Pattern Matching**: Exhaustive matching on enums
5. **Type Safety**: Leverage Rust's type system for correctness
6. **Zero-Cost Abstractions**: Traits compiled to static dispatch
7. **Cargo**: First-class package manager and build tool

---

## Implementation Checklist

- [ ] Set up Cargo project with proper metadata
- [ ] Implement trait-based `Connection` abstraction
- [ ] Implement `WebSocketConnection`
- [ ] Implement `QuicConnection`
- [ ] Implement `InfiniteDataStreamConnection`
- [ ] Implement `Channel` with async methods
- [ ] Implement `ThunderPropagatorClient`
- [ ] Create protocol-specific clients
- [ ] Define all models with Serde
- [ ] Implement error types with thiserror
- [ ] Implement encryption utilities
- [ ] Write unit tests
- [ ] Write integration tests
- [ ] Add documentation comments (///)
- [ ] Create usage examples in examples/
- [ ] Run cargo fmt, clippy
- [ ] Test with cargo test --all-features
- [ ] Build documentation
- [ ] Register on crates.io
- [ ] Publish to crates.io
- [ ] Monitor package analytics
