# ThunderPropagator Java Client - Implementation Prompt

## Project Goal
Create a Java client library for ThunderPropagator real-time data streaming framework supporting WebSocket, QUIC (HTTP/3), and InfiniteDataStream protocols.

## Target Environment
- **Java Version**: 17+ (LTS), 21+ recommended (for virtual threads)
- **Build Tool**: Maven or Gradle
- **Frameworks**: None required (stdlib implementation)
- **Testing**: JUnit 5 + Mockito

---

## Project Structure

```
thunderpropagator-java/
├── pom.xml (or build.gradle)
├── README.md
├── LICENSE
├── .gitignore
├── src/
│   ├── main/
│   │   ├── java/
│   │   │   └── com/thunderpropagator/client/
│   │   │       ├── ThunderPropagatorClient.java
│   │   │       ├── connections/
│   │   │       │   ├── AbstractConnection.java
│   │   │       │   ├── IConnection.java
│   │   │       │   ├── WebSocketConnection.java
│   │   │       │   ├── QuicConnection.java
│   │   │       │   └── InfiniteDataStreamConnection.java
│   │   │       ├── channels/
│   │   │       │   ├── AbstractChannel.java
│   │   │       │   ├── IChannel.java
│   │   │       │   └── Channel.java
│   │   │       ├── models/
│   │   │       │   ├── configuration/
│   │   │       │   │   ├── BaseConfiguration.java
│   │   │       │   │   ├── WebSocketConfiguration.java
│   │   │       │   │   ├── QuicConfiguration.java
│   │   │       │   │   └── InfiniteDataStreamConfiguration.java
│   │   │       │   ├── enums/
│   │   │       │   │   ├── ConnectionState.java
│   │   │       │   │   ├── ChannelState.java
│   │   │       │   │   └── ProtocolType.java
│   │   │       │   ├── messages/
│   │   │       │   │   ├── FeederMessage.java
│   │   │       │   │   ├── ConnectionResponse.java
│   │   │       │   │   └── Request.java
│   │   │       │   ├── metadata/
│   │   │       │   │   ├── ChannelMetadata.java
│   │   │       │   │   └── CipheringMetadata.java
│   │   │       │   └── subscriptions/
│   │   │       │       ├── SubscriptionRequest.java
│   │   │       │       └── SubscriptionResponse.java
│   │   │       ├── crypto/
│   │   │       │   ├── EncryptionUtils.java
│   │   │       │   └── CipherFactory.java
│   │   │       ├── events/
│   │   │       │   ├── ConnectionStateChangedEvent.java
│   │   │       │   ├── ChannelStateChangedEvent.java
│   │   │       │   └── MessageReceivedEvent.java
│   │   │       └── util/
│   │   │           ├── JsonUtils.java
│   │   │           └── ThreadUtils.java
│   │   └── resources/
│   │       └── META-INF/
│   │           └── MANIFEST.MF
│   └── test/
│       └── java/
│           └── com/thunderpropagator/client/
│               ├── connections/
│               │   └── WebSocketConnectionTest.java
│               ├── channels/
│               │   └── ChannelTest.java
│               └── integration/
│                   └── IntegrationTest.java
└── examples/
    ├── WebSocketExample.java
    ├── QuicExample.java
    └── AdvancedExample.java
```

---

## Core Dependencies

### Maven (pom.xml)

```xml
<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://maven.apache.org/POM/4.0.0"
         xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
         xsi:schemaLocation="http://maven.apache.org/POM/4.0.0 
         http://maven.apache.org/xsd/maven-4.0.0.xsd">
    <modelVersion>4.0.0</modelVersion>

    <groupId>com.thunderpropagator</groupId>
    <artifactId>thunderpropagator-client</artifactId>
    <version>1.0.0</version>
    <packaging>jar</packaging>

    <name>ThunderPropagator Java Client</name>
    <description>Real-time data streaming client for Java</description>

    <properties>
        <java.version>17</java.version>
        <maven.compiler.source>17</maven.compiler.source>
        <maven.compiler.target>17</maven.compiler.target>
        <project.build.sourceEncoding>UTF-8</project.build.sourceEncoding>
    </properties>

    <dependencies>
        <!-- WebSocket -->
        <dependency>
            <groupId>org.java-websocket</groupId>
            <artifactId>Java-WebSocket</artifactId>
            <version>1.5.5</version>
        </dependency>

        <!-- HTTP/3 QUIC -->
        <dependency>
            <groupId>io.netty.incubator</groupId>
            <artifactId>netty-incubator-codec-http3</artifactId>
            <version>0.0.23.Final</version>
        </dependency>

        <!-- JSON Processing -->
        <dependency>
            <groupId>com.fasterxml.jackson.core</groupId>
            <artifactId>jackson-databind</artifactId>
            <version>2.16.1</version>
        </dependency>

        <!-- Logging (SLF4J) -->
        <dependency>
            <groupId>org.slf4j</groupId>
            <artifactId>slf4j-api</artifactId>
            <version>2.0.11</version>
        </dependency>

        <!-- Testing -->
        <dependency>
            <groupId>org.junit.jupiter</groupId>
            <artifactId>junit-jupiter</artifactId>
            <version>5.10.1</version>
            <scope>test</scope>
        </dependency>
        <dependency>
            <groupId>org.mockito</groupId>
            <artifactId>mockito-core</artifactId>
            <version>5.10.0</version>
            <scope>test</scope>
        </dependency>
    </dependencies>

    <build>
        <plugins>
            <plugin>
                <groupId>org.apache.maven.plugins</groupId>
                <artifactId>maven-compiler-plugin</artifactId>
                <version>3.12.1</version>
            </plugin>
        </plugins>
    </build>
</project>
```

---

## Design Patterns & Idioms

### 1. Abstract Base Classes with Generics

```java
package com.thunderpropagator.client.connections;

import java.util.concurrent.CompletableFuture;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.function.Consumer;

public abstract class AbstractConnection<TConfig extends BaseConfiguration> 
        implements IConnection, AutoCloseable {
    
    protected final TConfig config;
    protected ConnectionState state = ConnectionState.READY;
    protected String connectionId;
    protected ConnectionResponse connectionInfo;
    
    // Event listeners
    private final CopyOnWriteArrayList<Consumer<StateChangedEvent>> stateChangedListeners = new CopyOnWriteArrayList<>();
    private final CopyOnWriteArrayList<Consumer<String>> messageReceivedListeners = new CopyOnWriteArrayList<>();
    private final CopyOnWriteArrayList<Consumer<Exception>> errorListeners = new CopyOnWriteArrayList<>();
    
    protected AbstractConnection(TConfig config) {
        this.config = config;
    }
    
    // Abstract methods for protocol-specific implementation
    protected abstract CompletableFuture<Void> connectImpl();
    protected abstract CompletableFuture<Void> disconnectImpl();
    protected abstract CompletableFuture<Void> sendImpl(String message);
    protected abstract void startReceiveLoop();
    
    @Override
    public CompletableFuture<Void> connectAsync() {
        if (state != ConnectionState.READY) {
            return CompletableFuture.failedFuture(
                new IllegalStateException("Cannot connect from state " + state)
            );
        }
        
        setState(ConnectionState.CONNECTING);
        
        return connectImpl()
            .thenRun(this::startReceiveLoop)
            .thenRun(() -> setState(ConnectionState.OPEN))
            .exceptionally(ex -> {
                setState(ConnectionState.HAS_ERROR);
                notifyError(ex);
                throw new RuntimeException(ex);
            });
    }
    
    @Override
    public CompletableFuture<Void> disconnectAsync() {
        if (state == ConnectionState.CLOSED || state == ConnectionState.CLOSING) {
            return CompletableFuture.completedFuture(null);
        }
        
        setState(ConnectionState.CLOSING);
        return disconnectImpl()
            .thenRun(() -> setState(ConnectionState.CLOSED));
    }
    
    @Override
    public CompletableFuture<Void> sendAsync(String message) {
        if (!isConnected()) {
            return CompletableFuture.failedFuture(
                new IllegalStateException("Connection is not open")
            );
        }
        return sendImpl(message);
    }
    
    @Override
    public boolean isConnected() {
        return state == ConnectionState.OPEN;
    }
    
    protected void setState(ConnectionState newState) {
        ConnectionState oldState = this.state;
        this.state = newState;
        notifyStateChanged(oldState, newState);
    }
    
    protected void notifyMessageReceived(String message) {
        // Filter PROBE messages
        if ("PROBE".equals(message.trim())) {
            return;
        }
        messageReceivedListeners.forEach(listener -> listener.accept(message));
    }
    
    protected void notifyStateChanged(ConnectionState oldState, ConnectionState newState) {
        var event = new StateChangedEvent(oldState, newState);
        stateChangedListeners.forEach(listener -> listener.accept(event));
    }
    
    protected void notifyError(Throwable error) {
        errorListeners.forEach(listener -> listener.accept((Exception) error));
    }
    
    // Event registration
    public void onStateChanged(Consumer<StateChangedEvent> listener) {
        stateChangedListeners.add(listener);
    }
    
    public void onMessageReceived(Consumer<String> listener) {
        messageReceivedListeners.add(listener);
    }
    
    public void onError(Consumer<Exception> listener) {
        errorListeners.add(listener);
    }
    
    @Override
    public void close() throws Exception {
        disconnectAsync().join();
    }
}
```

### 2. Records for Immutable Data (Java 17+)

```java
package com.thunderpropagator.client.models.messages;

import com.fasterxml.jackson.annotation.JsonProperty;
import java.time.Instant;
import java.util.Map;

public record FeederMessage(
    @JsonProperty("id") String id,
    @JsonProperty("route") Route route,
    @JsonProperty("payload") Map<String, Object> payload,
    @JsonProperty("timestamp") Instant timestamp
) {
    public record Route(
        @JsonProperty("channel") String channel,
        @JsonProperty("endpoint") String endpoint
    ) {}
}

public record ConnectionResponse(
    @JsonProperty("connectionId") String connectionId,
    @JsonProperty("serverVersion") String serverVersion,
    @JsonProperty("capabilities") List<String> capabilities
) {}

public record ChannelMetadata(
    @JsonProperty("channelId") String channelId,
    @JsonProperty("messageEncryption") String messageEncryption,
    @JsonProperty("endpoints") List<String> endpoints
) {}
```

### 3. Builder Pattern for Configuration

```java
package com.thunderpropagator.client.models.configuration;

import java.time.Duration;
import java.util.HashMap;
import java.util.Map;

public class WebSocketConfiguration extends BaseConfiguration {
    private final String uri;
    private final Map<String, String> headers;
    private final Duration keepAliveInterval;
    
    private WebSocketConfiguration(Builder builder) {
        super(builder.connectionTimeout, builder.reconnectEnabled, builder.reconnectMaxAttempts);
        this.uri = builder.uri;
        this.headers = new HashMap<>(builder.headers);
        this.keepAliveInterval = builder.keepAliveInterval;
    }
    
    public String getUri() {
        return uri;
    }
    
    public Map<String, String> getHeaders() {
        return new HashMap<>(headers);
    }
    
    public Duration getKeepAliveInterval() {
        return keepAliveInterval;
    }
    
    public static Builder builder() {
        return new Builder();
    }
    
    public static class Builder {
        private String uri;
        private Map<String, String> headers = new HashMap<>();
        private Duration connectionTimeout = Duration.ofSeconds(30);
        private Duration keepAliveInterval = Duration.ofSeconds(20);
        private boolean reconnectEnabled = true;
        private int reconnectMaxAttempts = 5;
        
        public Builder uri(String uri) {
            this.uri = uri;
            return this;
        }
        
        public Builder header(String key, String value) {
            this.headers.put(key, value);
            return this;
        }
        
        public Builder headers(Map<String, String> headers) {
            this.headers.putAll(headers);
            return this;
        }
        
        public Builder connectionTimeout(Duration timeout) {
            this.connectionTimeout = timeout;
            return this;
        }
        
        public Builder keepAliveInterval(Duration interval) {
            this.keepAliveInterval = interval;
            return this;
        }
        
        public Builder reconnectEnabled(boolean enabled) {
            this.reconnectEnabled = enabled;
            return this;
        }
        
        public Builder reconnectMaxAttempts(int attempts) {
            this.reconnectMaxAttempts = attempts;
            return this;
        }
        
        public WebSocketConfiguration build() {
            if (uri == null || uri.isBlank()) {
                throw new IllegalArgumentException("URI is required");
            }
            if (!uri.startsWith("ws://") && !uri.startsWith("wss://")) {
                throw new IllegalArgumentException("URI must start with ws:// or wss://");
            }
            return new WebSocketConfiguration(this);
        }
    }
}
```

### 4. Enums

```java
package com.thunderpropagator.client.models.enums;

public enum ConnectionState {
    READY,
    CONNECTING,
    OPEN,
    CLOSING,
    CLOSED,
    HAS_ERROR
}

public enum ChannelState {
    READY,
    OPENING,
    OPEN,
    CLOSING,
    CLOSED
}

public enum ProtocolType {
    WEBSOCKET("websocket"),
    QUIC("quic"),
    INFINITE_DATA_STREAM("infinite_data_stream");
    
    private final String value;
    
    ProtocolType(String value) {
        this.value = value;
    }
    
    public String getValue() {
        return value;
    }
}
```

---

## Implementation Details

### WebSocket Connection

```java
package com.thunderpropagator.client.connections;

import org.java_websocket.client.WebSocketClient;
import org.java_websocket.handshake.ServerHandshake;
import java.net.URI;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.TimeUnit;

public class WebSocketConnection extends AbstractConnection<WebSocketConfiguration> {
    private WebSocketClient webSocketClient;
    private final CountDownLatch connectLatch = new CountDownLatch(1);
    
    public WebSocketConnection(WebSocketConfiguration config) {
        super(config);
    }
    
    @Override
    protected CompletableFuture<Void> connectImpl() {
        return CompletableFuture.runAsync(() -> {
            try {
                URI uri = new URI(config.getUri());
                
                webSocketClient = new WebSocketClient(uri, config.getHeaders()) {
                    @Override
                    public void onOpen(ServerHandshake handshake) {
                        System.out.println("WebSocket connected to " + uri);
                        connectLatch.countDown();
                    }
                    
                    @Override
                    public void onMessage(String message) {
                        notifyMessageReceived(message);
                    }
                    
                    @Override
                    public void onClose(int code, String reason, boolean remote) {
                        setState(ConnectionState.CLOSED);
                    }
                    
                    @Override
                    public void onError(Exception ex) {
                        notifyError(ex);
                    }
                };
                
                webSocketClient.connect();
                
                // Wait for connection with timeout
                boolean connected = connectLatch.await(
                    config.getConnectionTimeout().toMillis(),
                    TimeUnit.MILLISECONDS
                );
                
                if (!connected) {
                    throw new RuntimeException("Connection timeout");
                }
            } catch (Exception e) {
                throw new RuntimeException("Failed to connect", e);
            }
        });
    }
    
    @Override
    protected CompletableFuture<Void> disconnectImpl() {
        return CompletableFuture.runAsync(() -> {
            if (webSocketClient != null) {
                webSocketClient.close();
                webSocketClient = null;
            }
        });
    }
    
    @Override
    protected CompletableFuture<Void> sendImpl(String message) {
        return CompletableFuture.runAsync(() -> {
            if (webSocketClient == null || !webSocketClient.isOpen()) {
                throw new IllegalStateException("WebSocket not connected");
            }
            webSocketClient.send(message);
        });
    }
    
    @Override
    protected void startReceiveLoop() {
        // WebSocket library handles receive loop automatically
    }
}
```

### Channel Implementation

```java
package com.thunderpropagator.client.channels;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.thunderpropagator.client.connections.IConnection;
import com.thunderpropagator.client.models.enums.ChannelState;
import com.thunderpropagator.client.models.metadata.ChannelMetadata;
import com.thunderpropagator.client.models.messages.FeederMessage;
import com.thunderpropagator.client.models.subscriptions.*;

import java.time.Instant;
import java.util.Map;
import java.util.UUID;
import java.util.concurrent.*;
import java.util.function.Consumer;

public class Channel implements IChannel {
    private final String name;
    private final IConnection connection;
    private ChannelState state = ChannelState.READY;
    private String channelId;
    private ChannelMetadata metadata;
    private final ObjectMapper objectMapper = new ObjectMapper();
    
    // Event listeners
    private final CopyOnWriteArrayList<Consumer<StateChangedEvent>> stateChangedListeners = new CopyOnWriteArrayList<>();
    private final CopyOnWriteArrayList<Consumer<FeederMessage>> messageReceivedListeners = new CopyOnWriteArrayList<>();
    private final CopyOnWriteArrayList<Consumer<ChannelMetadata>> metadataReceivedListeners = new CopyOnWriteArrayList<>();
    
    // Pending requests
    private final ConcurrentHashMap<String, CompletableFuture<Map<String, Object>>> pendingRequests = new ConcurrentHashMap<>();
    
    public Channel(String name, IConnection connection) {
        this.name = name;
        this.connection = connection;
    }
    
    @Override
    public CompletableFuture<Void> openAsync() {
        if (state != ChannelState.READY) {
            return CompletableFuture.failedFuture(
                new IllegalStateException("Cannot open channel from state " + state)
            );
        }
        
        setState(ChannelState.OPENING);
        
        try {
            var openRequest = Map.of(
                "route", Map.of("channel", name, "endpoint", "open"),
                "timestamp", Instant.now().toString()
            );
            
            String json = objectMapper.writeValueAsString(openRequest);
            
            return connection.sendAsync(json)
                .thenCompose(v -> waitForOpen(Duration.ofSeconds(30)))
                .thenRun(() -> setState(ChannelState.OPEN));
        } catch (Exception e) {
            setState(ChannelState.CLOSED);
            return CompletableFuture.failedFuture(e);
        }
    }
    
    @Override
    public CompletableFuture<Void> closeAsync() {
        if (state == ChannelState.CLOSED) {
            return CompletableFuture.completedFuture(null);
        }
        
        setState(ChannelState.CLOSING);
        
        try {
            var closeRequest = Map.of(
                "route", Map.of("channel", name, "endpoint", "close")
            );
            
            String json = objectMapper.writeValueAsString(closeRequest);
            
            return connection.sendAsync(json)
                .thenRun(() -> {
                    setState(ChannelState.CLOSED);
                    // Cancel all pending requests
                    pendingRequests.values().forEach(future -> 
                        future.completeExceptionally(new RuntimeException("Channel closed"))
                    );
                    pendingRequests.clear();
                });
        } catch (Exception e) {
            return CompletableFuture.failedFuture(e);
        }
    }
    
    @Override
    public CompletableFuture<SubscriptionResponse> subscribeAsync(SubscriptionRequest subscription) {
        String requestId = UUID.randomUUID().toString();
        
        try {
            var request = Map.of(
                "id", requestId,
                "route", Map.of("channel", name, "endpoint", "subscribe"),
                "payload", subscription
            );
            
            String json = objectMapper.writeValueAsString(request);
            
            CompletableFuture<Map<String, Object>> responseFuture = new CompletableFuture<>();
            pendingRequests.put(requestId, responseFuture);
            
            // Set timeout
            CompletableFuture.delayedExecutor(30, TimeUnit.SECONDS)
                .execute(() -> {
                    if (pendingRequests.remove(requestId) != null) {
                        responseFuture.completeExceptionally(new TimeoutException("Subscribe timeout"));
                    }
                });
            
            return connection.sendAsync(json)
                .thenCompose(v -> responseFuture)
                .thenApply(payload -> objectMapper.convertValue(payload, SubscriptionResponse.class));
        } catch (Exception e) {
            pendingRequests.remove(requestId);
            return CompletableFuture.failedFuture(e);
        }
    }
    
    public CompletableFuture<Void> handleReceivedMessage(Map<String, Object> message) {
        return CompletableFuture.runAsync(() -> {
            try {
                // Check if it's a response to pending request
                if (message.containsKey("id")) {
                    String id = (String) message.get("id");
                    CompletableFuture<Map<String, Object>> future = pendingRequests.remove(id);
                    if (future != null) {
                        @SuppressWarnings("unchecked")
                        Map<String, Object> payload = (Map<String, Object>) message.get("payload");
                        future.complete(payload);
                        return;
                    }
                }
                
                // Check if it's metadata
                if (message.containsKey("metadata")) {
                    this.metadata = objectMapper.convertValue(message.get("metadata"), ChannelMetadata.class);
                    notifyMetadataReceived(this.metadata);
                    return;
                }
                
                // Regular message
                FeederMessage feederMessage = objectMapper.convertValue(message, FeederMessage.class);
                notifyMessageReceived(feederMessage);
            } catch (Exception e) {
                System.err.println("Error handling message: " + e.getMessage());
            }
        });
    }
    
    @Override
    public boolean isOpen() {
        return state == ChannelState.OPEN;
    }
    
    @Override
    public String getName() {
        return name;
    }
    
    private void setState(ChannelState newState) {
        ChannelState oldState = this.state;
        this.state = newState;
        notifyStateChanged(oldState, newState);
    }
    
    private CompletableFuture<Void> waitForOpen(Duration timeout) {
        return CompletableFuture.runAsync(() -> {
            long endTime = System.currentTimeMillis() + timeout.toMillis();
            while (metadata == null && System.currentTimeMillis() < endTime) {
                try {
                    Thread.sleep(100);
                } catch (InterruptedException e) {
                    throw new RuntimeException(e);
                }
            }
            if (metadata == null) {
                throw new RuntimeException("Channel open timeout");
            }
        });
    }
    
    private void notifyStateChanged(ChannelState oldState, ChannelState newState) {
        var event = new StateChangedEvent(oldState, newState);
        stateChangedListeners.forEach(listener -> listener.accept(event));
    }
    
    private void notifyMessageReceived(FeederMessage message) {
        messageReceivedListeners.forEach(listener -> listener.accept(message));
    }
    
    private void notifyMetadataReceived(ChannelMetadata metadata) {
        metadataReceivedListeners.forEach(listener -> listener.accept(metadata));
    }
    
    public void onStateChanged(Consumer<StateChangedEvent> listener) {
        stateChangedListeners.add(listener);
    }
    
    public void onMessageReceived(Consumer<FeederMessage> listener) {
        messageReceivedListeners.add(listener);
    }
    
    public void onMetadataReceived(Consumer<ChannelMetadata> listener) {
        metadataReceivedListeners.add(listener);
    }
}
```

### ThunderPropagator Client

```java
package com.thunderpropagator.client;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.thunderpropagator.client.channels.Channel;
import com.thunderpropagator.client.channels.IChannel;
import com.thunderpropagator.client.connections.IConnection;
import com.thunderpropagator.client.connections.WebSocketConnection;
import com.thunderpropagator.client.models.configuration.WebSocketConfiguration;

import java.util.Map;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ConcurrentHashMap;

public class ThunderPropagatorClient implements AutoCloseable {
    protected final IConnection connection;
    protected final ConcurrentHashMap<String, Channel> channels = new ConcurrentHashMap<>();
    private final ObjectMapper objectMapper = new ObjectMapper();
    
    public ThunderPropagatorClient(IConnection connection) {
        this.connection = connection;
        this.connection.onMessageReceived(this::routeMessage);
    }
    
    public CompletableFuture<Void> connectAsync() {
        return connection.connectAsync();
    }
    
    public CompletableFuture<Void> disconnectAsync() {
        // Close all channels first
        CompletableFuture<?>[] closeFutures = channels.values().stream()
            .map(Channel::closeAsync)
            .toArray(CompletableFuture[]::new);
        
        return CompletableFuture.allOf(closeFutures)
            .thenCompose(v -> connection.disconnectAsync());
    }
    
    public CompletableFuture<IChannel> createChannelAsync(String name) {
        if (channels.containsKey(name)) {
            return CompletableFuture.completedFuture(channels.get(name));
        }
        
        Channel channel = new Channel(name, connection);
        channels.put(name, channel);
        return channel.openAsync().thenApply(v -> channel);
    }
    
    public IChannel getChannel(String name) {
        return channels.get(name);
    }
    
    private void routeMessage(String message) {
        try {
            // Try JSON format
            @SuppressWarnings("unchecked")
            Map<String, Object> data = objectMapper.readValue(message, Map.class);
            
            if (data.containsKey("route")) {
                @SuppressWarnings("unchecked")
                Map<String, Object> route = (Map<String, Object>) data.get("route");
                String channelName = (String) route.get("channel");
                
                Channel channel = channels.get(channelName);
                if (channel != null) {
                    channel.handleReceivedMessage(data);
                }
                return;
            }
        } catch (Exception e) {
            // Not JSON, try CSV
            String[] parts = message.split(",", 2);
            if (parts.length >= 2) {
                String channelName = parts[0];
                Channel channel = channels.get(channelName);
                if (channel != null) {
                    channel.handleReceivedMessage(Map.of("payload", parts[1]));
                }
            }
        }
    }
    
    @Override
    public void close() throws Exception {
        disconnectAsync().join();
    }
}

public class WebSocketClient extends ThunderPropagatorClient {
    public WebSocketClient(WebSocketConfiguration config) {
        super(new WebSocketConnection(config));
    }
}
```

---

## Usage Example

```java
import com.thunderpropagator.client.WebSocketClient;
import com.thunderpropagator.client.models.configuration.WebSocketConfiguration;
import com.thunderpropagator.client.models.subscriptions.SubscriptionRequest;

import java.time.Duration;

public class Example {
    public static void main(String[] args) throws Exception {
        // Configure
        var config = WebSocketConfiguration.builder()
            .uri("wss://example.com/thunder")
            .header("Authorization", "Bearer token123")
            .keepAliveInterval(Duration.ofSeconds(20))
            .build();
        
        // Create client
        try (var client = new WebSocketClient(config)) {
            // Connect
            client.connectAsync().join();
            
            // Create channel
            var channel = client.createChannelAsync("market-data").join();
            
            // Subscribe to messages
            channel.onMessageReceived(message -> {
                System.out.println("Received: " + message);
            });
            
            // Subscribe to data
            var subscription = new SubscriptionRequest(
                "quotes",
                Map.of("symbols", List.of("AAPL", "GOOGL"))
            );
            
            var response = channel.subscribeAsync(subscription).join();
            System.out.println("Subscribed: " + response.subscriptionId());
            
            // Keep running
            Thread.sleep(60000);
        }
    }
}
```

---

## Testing

```java
package com.thunderpropagator.client.connections;

import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.AfterEach;
import static org.junit.jupiter.api.Assertions.*;

import java.time.Duration;

class WebSocketConnectionTest {
    private WebSocketConnection connection;
    
    @BeforeEach
    void setUp() {
        var config = WebSocketConfiguration.builder()
            .uri("wss://echo.websocket.org")
            .connectionTimeout(Duration.ofSeconds(10))
            .build();
        
        connection = new WebSocketConnection(config);
    }
    
    @AfterEach
    void tearDown() throws Exception {
        if (connection.isConnected()) {
            connection.disconnectAsync().join();
        }
    }
    
    @Test
    void testConnect() {
        connection.connectAsync().join();
        assertTrue(connection.isConnected());
    }
    
    @Test
    void testSendReceive() throws Exception {
        connection.connectAsync().join();
        
        var received = new ArrayList<String>();
        connection.onMessageReceived(received::add);
        
        connection.sendAsync("test").join();
        
        Thread.sleep(1000);
        assertTrue(received.contains("test"));
    }
}
```

---

## Build & Distribution

```bash
# Build
mvn clean package

# Run tests
mvn test

# Install locally
mvn install

# Deploy to Maven Central
mvn deploy
```

---

## Maven Central Publishing Guide

### 1. Prerequisites for Maven Central

**Requirements:**
- GitHub/GitLab repository
- GPG key for signing artifacts
- Sonatype OSSRH account
- Project meets quality requirements (POM, Javadoc, sources)

### 2. Set Up GPG Signing

```bash
# Generate GPG key
gpg --gen-key
# Follow prompts, use your real name and email

# List keys
gpg --list-keys

# Export public key to keyserver
gpg --keyserver keyserver.ubuntu.com --send-keys YOUR-KEY-ID

# Export private key for CI/CD
gpg --export-secret-keys YOUR-KEY-ID > private.key
```

### 3. Register with Sonatype OSSRH

1. Create Jira account at https://issues.sonatype.org
2. Create a "New Project" ticket
3. Verify domain ownership (or use GitHub Pages)
4. Wait for approval (usually 1-2 business days)

### 4. Configure Maven Settings

**~/.m2/settings.xml**:
```xml
<settings>
  <servers>
    <server>
      <id>ossrh</id>
      <username>your-jira-username</username>
      <password>your-jira-password</password>
    </server>
  </servers>
  
  <profiles>
    <profile>
      <id>ossrh</id>
      <activation>
        <activeByDefault>true</activeByDefault>
      </activation>
      <properties>
        <gpg.executable>gpg</gpg.executable>
        <gpg.passphrase>your-gpg-passphrase</gpg.passphrase>
      </properties>
    </profile>
  </profiles>
</settings>
```

### 5. Complete POM Configuration

**pom.xml**:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<project xmlns="http://maven.apache.org/POM/4.0.0"
         xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
         xsi:schemaLocation="http://maven.apache.org/POM/4.0.0 
         http://maven.apache.org/xsd/maven-4.0.0.xsd">
    <modelVersion>4.0.0</modelVersion>

    <groupId>com.thunderpropagator</groupId>
    <artifactId>thunderpropagator-client</artifactId>
    <version>1.0.0</version>
    <packaging>jar</packaging>

    <name>ThunderPropagator Java Client</name>
    <description>Real-time data streaming client for Java</description>
    <url>https://github.com/yourusername/thunderpropagator-java</url>

    <licenses>
        <license>
            <name>MIT License</name>
            <url>https://opensource.org/licenses/MIT</url>
            <distribution>repo</distribution>
        </license>
    </licenses>

    <developers>
        <developer>
            <id>yourusername</id>
            <name>Your Name</name>
            <email>your.email@example.com</email>
            <organization>YourOrg</organization>
            <organizationUrl>https://yourorg.com</organizationUrl>
        </developer>
    </developers>

    <scm>
        <connection>scm:git:git://github.com/yourusername/thunderpropagator-java.git</connection>
        <developerConnection>scm:git:ssh://github.com:yourusername/thunderpropagator-java.git</developerConnection>
        <url>https://github.com/yourusername/thunderpropagator-java/tree/main</url>
    </scm>

    <properties>
        <java.version>17</java.version>
        <maven.compiler.source>17</maven.compiler.source>
        <maven.compiler.target>17</maven.compiler.target>
        <project.build.sourceEncoding>UTF-8</project.build.sourceEncoding>
    </properties>

    <dependencies>
        <!-- Your dependencies here -->
    </dependencies>

    <distributionManagement>
        <snapshotRepository>
            <id>ossrh</id>
            <url>https://s01.oss.sonatype.org/content/repositories/snapshots</url>
        </snapshotRepository>
        <repository>
            <id>ossrh</id>
            <url>https://s01.oss.sonatype.org/service/local/staging/deploy/maven2/</url>
        </repository>
    </distributionManagement>

    <build>
        <plugins>
            <!-- Compiler Plugin -->
            <plugin>
                <groupId>org.apache.maven.plugins</groupId>
                <artifactId>maven-compiler-plugin</artifactId>
                <version>3.12.1</version>
                <configuration>
                    <source>17</source>
                    <target>17</target>
                </configuration>
            </plugin>

            <!-- Source Plugin (required for Maven Central) -->
            <plugin>
                <groupId>org.apache.maven.plugins</groupId>
                <artifactId>maven-source-plugin</artifactId>
                <version>3.3.0</version>
                <executions>
                    <execution>
                        <id>attach-sources</id>
                        <goals>
                            <goal>jar-no-fork</goal>
                        </goals>
                    </execution>
                </executions>
            </plugin>

            <!-- Javadoc Plugin (required for Maven Central) -->
            <plugin>
                <groupId>org.apache.maven.plugins</groupId>
                <artifactId>maven-javadoc-plugin</artifactId>
                <version>3.6.3</version>
                <configuration>
                    <source>17</source>
                    <doclint>none</doclint>
                </configuration>
                <executions>
                    <execution>
                        <id>attach-javadocs</id>
                        <goals>
                            <goal>jar</goal>
                        </goals>
                    </execution>
                </executions>
            </plugin>

            <!-- GPG Plugin (for signing) -->
            <plugin>
                <groupId>org.apache.maven.plugins</groupId>
                <artifactId>maven-gpg-plugin</artifactId>
                <version>3.1.0</version>
                <executions>
                    <execution>
                        <id>sign-artifacts</id>
                        <phase>verify</phase>
                        <goals>
                            <goal>sign</goal>
                        </goals>
                        <configuration>
                            <gpgArguments>
                                <arg>--pinentry-mode</arg>
                                <arg>loopback</arg>
                            </gpgArguments>
                        </configuration>
                    </execution>
                </executions>
            </plugin>

            <!-- Nexus Staging Plugin -->
            <plugin>
                <groupId>org.sonatype.plugins</groupId>
                <artifactId>nexus-staging-maven-plugin</artifactId>
                <version>1.6.13</version>
                <extensions>true</extensions>
                <configuration>
                    <serverId>ossrh</serverId>
                    <nexusUrl>https://s01.oss.sonatype.org/</nexusUrl>
                    <autoReleaseAfterClose>true</autoReleaseAfterClose>
                </configuration>
            </plugin>

            <!-- Surefire Plugin (for tests) -->
            <plugin>
                <groupId>org.apache.maven.plugins</groupId>
                <artifactId>maven-surefire-plugin</artifactId>
                <version>3.2.3</version>
            </plugin>
        </plugins>
    </build>

    <profiles>
        <!-- Profile for releasing -->
        <profile>
            <id>release</id>
            <build>
                <plugins>
                    <plugin>
                        <groupId>org.apache.maven.plugins</groupId>
                        <artifactId>maven-gpg-plugin</artifactId>
                        <version>3.1.0</version>
                        <executions>
                            <execution>
                                <id>sign-artifacts</id>
                                <phase>verify</phase>
                                <goals>
                                    <goal>sign</goal>
                                </goals>
                            </execution>
                        </executions>
                    </plugin>
                </plugins>
            </build>
        </profile>
    </profiles>
</project>
```

### 6. Build and Deploy

```bash
# Clean and build
mvn clean package

# Verify all required artifacts
ls target/
# Should see: *-sources.jar, *-javadoc.jar, *.jar, *.pom, *.asc (signatures)

# Deploy to OSSRH (staging)
mvn clean deploy -P release

# Or use nexus-staging plugin to control release
mvn clean deploy -P release
mvn nexus-staging:release -P release
```

### 7. Gradle Configuration (Alternative)

**build.gradle**:
```groovy
plugins {
    id 'java-library'
    id 'maven-publish'
    id 'signing'
}

group = 'com.thunderpropagator'
version = '1.0.0'
sourceCompatibility = '17'

repositories {
    mavenCentral()
}

dependencies {
    implementation 'org.java-websocket:Java-WebSocket:1.5.5'
    implementation 'io.netty.incubator:netty-incubator-codec-http3:0.0.23.Final'
    implementation 'com.fasterxml.jackson.core:jackson-databind:2.16.1'
    implementation 'org.slf4j:slf4j-api:2.0.11'
    
    testImplementation 'org.junit.jupiter:junit-jupiter:5.10.1'
    testImplementation 'org.mockito:mockito-core:5.10.0'
}

java {
    withJavadocJar()
    withSourcesJar()
}

publishing {
    publications {
        mavenJava(MavenPublication) {
            from components.java
            
            pom {
                name = 'ThunderPropagator Java Client'
                description = 'Real-time data streaming client for Java'
                url = 'https://github.com/yourusername/thunderpropagator-java'
                
                licenses {
                    license {
                        name = 'MIT License'
                        url = 'https://opensource.org/licenses/MIT'
                    }
                }
                
                developers {
                    developer {
                        id = 'yourusername'
                        name = 'Your Name'
                        email = 'your.email@example.com'
                    }
                }
                
                scm {
                    connection = 'scm:git:git://github.com/yourusername/thunderpropagator-java.git'
                    developerConnection = 'scm:git:ssh://github.com:yourusername/thunderpropagator-java.git'
                    url = 'https://github.com/yourusername/thunderpropagator-java'
                }
            }
        }
    }
    
    repositories {
        maven {
            name = "OSSRH"
            def releasesRepoUrl = "https://s01.oss.sonatype.org/service/local/staging/deploy/maven2/"
            def snapshotsRepoUrl = "https://s01.oss.sonatype.org/content/repositories/snapshots/"
            url = version.endsWith('SNAPSHOT') ? snapshotsRepoUrl : releasesRepoUrl
            credentials {
                username = project.findProperty('ossrhUsername') ?: System.getenv('OSSRH_USERNAME')
                password = project.findProperty('ossrhPassword') ?: System.getenv('OSSRH_PASSWORD')
            }
        }
    }
}

signing {
    sign publishing.publications.mavenJava
}

tasks.named('test') {
    useJUnitPlatform()
}
```

**gradle.properties**:
```properties
ossrhUsername=your-jira-username
ossrhPassword=your-jira-password
signing.keyId=YOUR-GPG-KEY-ID
signing.password=your-gpg-passphrase
signing.secretKeyRingFile=/path/to/.gnupg/secring.gpg
```

### 8. Automated Publishing with GitHub Actions

**.github/workflows/publish.yml**:
```yaml
name: Publish to Maven Central

on:
  release:
    types: [published]

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Set up JDK 17
        uses: actions/setup-java@v4
        with:
          java-version: '17'
          distribution: 'temurin'
          cache: maven
      
      - name: Set up Maven settings
        uses: s4u/maven-settings-action@v3
        with:
          servers: |
            [{
              "id": "ossrh",
              "username": "${{ secrets.OSSRH_USERNAME }}",
              "password": "${{ secrets.OSSRH_PASSWORD }}"
            }]
      
      - name: Import GPG key
        uses: crazy-max/ghaction-import-gpg@v6
        with:
          gpg_private_key: ${{ secrets.GPG_PRIVATE_KEY }}
          passphrase: ${{ secrets.GPG_PASSPHRASE }}
      
      - name: Publish to Maven Central
        run: mvn clean deploy -P release --no-transfer-progress
        env:
          MAVEN_GPG_PASSPHRASE: ${{ secrets.GPG_PASSPHRASE }}
```

### 9. Version Management

**Maven**:
```bash
# Set version
mvn versions:set -DnewVersion=1.1.0

# Commit version
mvn versions:commit

# Deploy
mvn clean deploy -P release
```

**Gradle**:
```bash
# Update version in build.gradle
# Then publish
./gradlew publish
```

### 10. Creating JavaDoc

```bash
# Generate JavaDoc
mvn javadoc:javadoc

# View at target/site/apidocs/index.html

# Ensure all public APIs have JavaDoc
# Example:
```

```java
/**
 * WebSocket connection implementation for ThunderPropagator.
 * 
 * <p>This class provides WebSocket protocol support with automatic reconnection
 * and message routing capabilities.</p>
 * 
 * <p>Example usage:</p>
 * <pre>{@code
 * var config = WebSocketConfiguration.builder()
 *     .uri("wss://example.com/thunder")
 *     .build();
 * var connection = new WebSocketConnection(config);
 * connection.connectAsync().join();
 * }</pre>
 * 
 * @author Your Name
 * @since 1.0.0
 * @see AbstractConnection
 */
public class WebSocketConnection extends AbstractConnection<WebSocketConfiguration> {
    // ...
}
```

### 11. Essential Files

**README.md**:
```markdown
# ThunderPropagator Java Client

Real-time data streaming client for Java.

## Installation

### Maven
```xml
<dependency>
    <groupId>com.thunderpropagator</groupId>
    <artifactId>thunderpropagator-client</artifactId>
    <version>1.0.0</version>
</dependency>
```

### Gradle
```groovy
implementation 'com.thunderpropagator:thunderpropagator-client:1.0.0'
```

## Quick Start

```java
var config = WebSocketConfiguration.builder()
    .uri("wss://example.com/thunder")
    .build();

try (var client = new WebSocketClient(config)) {
    client.connectAsync().join();
    var channel = client.createChannelAsync("my-channel").join();
    
    channel.onMessageReceived(message -> {
        System.out.println("Received: " + message);
    });
    
    Thread.sleep(60000);
}
```

## Features

- Multiple protocol support (WebSocket, QUIC, custom)
- Full Java 17+ support with Records
- CompletableFuture-based async API
- Built-in encryption
- Auto-reconnection
- Channel-based communication

## Requirements

- Java 17 or higher

## License

MIT
```

**CHANGELOG.md**:
```markdown
# Changelog

## [1.0.0] - 2025-01-01

### Added
- Initial release
- WebSocket support
- QUIC support
- Channel-based communication
- Request/response pattern
```

### 12. Testing Before Release

```bash
# Run all tests
mvn test

# Check package contents
jar -tf target/thunderpropagator-client-1.0.0.jar

# Verify signatures
gpg --verify target/thunderpropagator-client-1.0.0.jar.asc

# Test in local project
mvn install
# Then use in another project with version 1.0.0
```

### 13. Maven Central Badges

Add to README.md:

```markdown
[![Maven Central](https://maven-badges.herokuapp.com/maven-central/com.thunderpropagator/thunderpropagator-client/badge.svg)](https://maven-badges.herokuapp.com/maven-central/com.thunderpropagator/thunderpropagator-client)
[![javadoc](https://javadoc.io/badge2/com.thunderpropagator/thunderpropagator-client/javadoc.svg)](https://javadoc.io/doc/com.thunderpropagator/thunderpropagator-client)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
```

### 14. Post-Publishing Steps

1. **Verify on Maven Central**: Search for your artifact at https://search.maven.org
2. **Wait for sync**: Takes 2-4 hours to appear in search
3. **Update documentation**: Add installation instructions
4. **Create GitHub release**: Tag version and add release notes
5. **Announce**: Share on relevant forums, Twitter, Reddit

### 15. Troubleshooting

**Common Issues:**

```bash
# GPG signing fails
export GPG_TTY=$(tty)
echo "use-agent" >> ~/.gnupg/gpg.conf

# Missing required metadata
# Ensure POM has: name, description, url, licenses, developers, scm

# Staging repository failed
# Check all artifacts are present: jar, sources, javadoc, pom, all .asc files

# Release failed
# Log in to https://s01.oss.sonatype.org/ and check staging repositories
```

---

## Key Java-Specific Considerations

1. **CompletableFuture**: All async operations return `CompletableFuture<T>`
2. **Records**: Immutable data with Java 17+
3. **Builder Pattern**: For complex configuration objects
4. **AutoCloseable**: Support try-with-resources
5. **CopyOnWriteArrayList**: Thread-safe event listeners
6. **ConcurrentHashMap**: Thread-safe collections
7. **Virtual Threads** (Java 21+): Consider using for better scalability
8. **SLF4J**: Standard logging facade
9. **Maven Central**: Follow all requirements (POM, Javadoc, sources, GPG)

---

## Implementation Checklist

- [ ] Set up Maven/Gradle project
- [ ] Configure POM with all required metadata
- [ ] Implement `AbstractConnection` base class
- [ ] Implement `WebSocketConnection`
- [ ] Implement `QuicConnection` (HTTP/3)
- [ ] Implement `InfiniteDataStreamConnection`
- [ ] Implement `Channel` class
- [ ] Implement `ThunderPropagatorClient`
- [ ] Create protocol-specific clients
- [ ] Define all models with Records
- [ ] Implement builder pattern for configurations
- [ ] Implement encryption utilities
- [ ] Write comprehensive JavaDoc for all public APIs
- [ ] Write JUnit 5 tests
- [ ] Write integration tests
- [ ] Create usage examples
- [ ] Add LICENSE file
- [ ] Write comprehensive README
- [ ] Set up GPG signing
- [ ] Register with Sonatype OSSRH
- [ ] Configure Maven settings
- [ ] Test local deployment
- [ ] Set up CI/CD (GitHub Actions)
- [ ] Deploy to Maven Central staging
- [ ] Release to Maven Central
- [ ] Monitor package usage
