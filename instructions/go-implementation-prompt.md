# ThunderPropagator Go Client - Implementation Prompt

## Project Goal
Create a Go client library for ThunderPropagator real-time data streaming framework supporting WebSocket, QUIC, and custom protocols.

## Target Environment
- **Go Version**: 1.21+
- **Module System**: Go modules
- **Package Distribution**: pkg.go.dev
- **Testing**: Built-in testing package

---

## Project Structure

```
thunderpropagator-go/
├── go.mod
├── go.sum
├── README.md
├── LICENSE
├── CHANGELOG.md
├── .gitignore
├── client.go
├── connection.go
├── channel.go
├── config.go
├── errors.go
├── connections/
│   ├── websocket.go
│   ├── quic.go
│   └── infinite_data_stream.go
├── models/
│   ├── enums.go
│   ├── messages.go
│   ├── metadata.go
│   └── subscriptions.go
├── crypto/
│   ├── aes.go
│   └── rsa.go
├── internal/
│   ├── utils/
│   │   └── helpers.go
│   └── constants/
│       └── constants.go
├── examples/
│   ├── websocket/
│   │   └── main.go
│   ├── quic/
│   │   └── main.go
│   └── advanced/
│       └── main.go
└── tests/
    ├── integration_test.go
    └── unit_test.go
```

---

## Core Dependencies

**go.mod**:
```go
module github.com/yourusername/thunderpropagator-go

go 1.21

require (
    github.com/gorilla/websocket v1.5.1
    github.com/quic-go/quic-go v0.40.1
    github.com/google/uuid v1.5.0
    golang.org/x/crypto v0.17.0
)

require (
    github.com/go-task/slim-sprig v0.0.0-20230315185526-52ccab3ef572 // indirect
    github.com/google/pprof v0.0.0-20231212022811-ec68065c825e // indirect
    github.com/onsi/ginkgo/v2 v2.13.2 // indirect
    github.com/quic-go/qpack v0.4.0 // indirect
    golang.org/x/exp v0.0.0-20231219180239-dc181d75b848 // indirect
    golang.org/x/mod v0.14.0 // indirect
    golang.org/x/net v0.19.0 // indirect
    golang.org/x/sync v0.5.0 // indirect
    golang.org/x/sys v0.15.0 // indirect
    golang.org/x/text v0.14.0 // indirect
    golang.org/x/tools v0.16.1 // indirect
)
```

---

## Design Patterns & Idioms

### 1. Interface-Based Design

**connection.go**:
```go
package thunderpropagator

import (
    "context"
    "sync"
    "time"
)

// Connection defines the interface for protocol-specific connections
type Connection interface {
    // Connect establishes connection to server
    Connect(ctx context.Context) error
    
    // Disconnect closes connection
    Disconnect(ctx context.Context) error
    
    // Send sends a message
    Send(ctx context.Context, message string) error
    
    // IsConnected checks connection status
    IsConnected() bool
    
    // GetConnectionID returns connection identifier
    GetConnectionID() string
    
    // OnMessageReceived sets message handler
    OnMessageReceived(handler MessageHandler)
    
    // OnStateChanged sets state change handler
    OnStateChanged(handler StateChangeHandler)
}

// MessageHandler is called when message received
type MessageHandler func(message string)

// StateChangeHandler is called when connection state changes
type StateChangeHandler func(oldState, newState ConnectionState)

// AbstractConnection provides base functionality for all connections
type AbstractConnection struct {
    mu                sync.RWMutex
    state             ConnectionState
    connectionID      string
    messageHandlers   []MessageHandler
    stateHandlers     []StateChangeHandler
    receiveTimeout    time.Duration
    keepAliveInterval time.Duration
}

// NewAbstractConnection creates a new abstract connection
func NewAbstractConnection() *AbstractConnection {
    return &AbstractConnection{
        state:             ConnectionStateReady,
        messageHandlers:   make([]MessageHandler, 0),
        stateHandlers:     make([]StateChangeHandler, 0),
        receiveTimeout:    24 * time.Hour,
        keepAliveInterval: 20 * time.Second,
    }
}

// GetState returns current connection state
func (c *AbstractConnection) GetState() ConnectionState {
    c.mu.RLock()
    defer c.mu.RUnlock()
    return c.state
}

// SetState updates connection state
func (c *AbstractConnection) SetState(newState ConnectionState) {
    c.mu.Lock()
    oldState := c.state
    c.state = newState
    handlers := make([]StateChangeHandler, len(c.stateHandlers))
    copy(handlers, c.stateHandlers)
    c.mu.Unlock()
    
    for _, handler := range handlers {
        handler(oldState, newState)
    }
}

// OnMessageReceived registers message handler
func (c *AbstractConnection) OnMessageReceived(handler MessageHandler) {
    c.mu.Lock()
    defer c.mu.Unlock()
    c.messageHandlers = append(c.messageHandlers, handler)
}

// OnStateChanged registers state change handler
func (c *AbstractConnection) OnStateChanged(handler StateChangeHandler) {
    c.mu.Lock()
    defer c.mu.Unlock()
    c.stateHandlers = append(c.stateHandlers, handler)
}

// EmitMessage dispatches message to handlers
func (c *AbstractConnection) EmitMessage(message string) {
    c.mu.RLock()
    handlers := make([]MessageHandler, len(c.messageHandlers))
    copy(handlers, c.messageHandlers)
    c.mu.RUnlock()
    
    for _, handler := range handlers {
        handler(message)
    }
}
```

### 2. Enums with Type Safety

**models/enums.go**:
```go
package models

// ConnectionState represents connection state
type ConnectionState int

const (
    ConnectionStateReady ConnectionState = iota
    ConnectionStateConnecting
    ConnectionStateOpen
    ConnectionStateClosing
    ConnectionStateClosed
    ConnectionStateHasError
)

func (s ConnectionState) String() string {
    switch s {
    case ConnectionStateReady:
        return "Ready"
    case ConnectionStateConnecting:
        return "Connecting"
    case ConnectionStateOpen:
        return "Open"
    case ConnectionStateClosing:
        return "Closing"
    case ConnectionStateClosed:
        return "Closed"
    case ConnectionStateHasError:
        return "HasError"
    default:
        return "Unknown"
    }
}

// ChannelState represents channel state
type ChannelState int

const (
    ChannelStateReady ChannelState = iota
    ChannelStateOpening
    ChannelStateOpen
    ChannelStateClosing
    ChannelStateClosed
)

func (s ChannelState) String() string {
    switch s {
    case ChannelStateReady:
        return "Ready"
    case ChannelStateOpening:
        return "Opening"
    case ChannelStateOpen:
        return "Open"
    case ChannelStateClosing:
        return "Closing"
    case ChannelStateClosed:
        return "Closed"
    default:
        return "Unknown"
    }
}

// ProtocolType represents protocol type
type ProtocolType int

const (
    ProtocolTypeWebSocket ProtocolType = iota
    ProtocolTypeQuic
    ProtocolTypeInfiniteDataStream
)

func (p ProtocolType) String() string {
    switch p {
    case ProtocolTypeWebSocket:
        return "WebSocket"
    case ProtocolTypeQuic:
        return "QUIC"
    case ProtocolTypeInfiniteDataStream:
        return "InfiniteDataStream"
    default:
        return "Unknown"
    }
}
```

### 3. Configuration with Functional Options

**config.go**:
```go
package thunderpropagator

import (
    "net/http"
    "time"
)

// WebSocketConfig holds WebSocket configuration
type WebSocketConfig struct {
    URI               string
    Headers           http.Header
    ConnectionTimeout time.Duration
    KeepAliveInterval time.Duration
    MaxRetries        int
}

// WebSocketOption is a functional option for WebSocket configuration
type WebSocketOption func(*WebSocketConfig)

// NewWebSocketConfig creates configuration with functional options
func NewWebSocketConfig(uri string, opts ...WebSocketOption) *WebSocketConfig {
    config := &WebSocketConfig{
        URI:               uri,
        Headers:           make(http.Header),
        ConnectionTimeout: 30 * time.Second,
        KeepAliveInterval: 20 * time.Second,
        MaxRetries:        3,
    }
    
    for _, opt := range opts {
        opt(config)
    }
    
    return config
}

// WithHeader adds a header to the configuration
func WithHeader(key, value string) WebSocketOption {
    return func(c *WebSocketConfig) {
        c.Headers.Add(key, value)
    }
}

// WithConnectionTimeout sets connection timeout
func WithConnectionTimeout(timeout time.Duration) WebSocketOption {
    return func(c *WebSocketConfig) {
        c.ConnectionTimeout = timeout
    }
}

// WithKeepAliveInterval sets keep-alive interval
func WithKeepAliveInterval(interval time.Duration) WebSocketOption {
    return func(c *WebSocketConfig) {
        c.KeepAliveInterval = interval
    }
}

// WithMaxRetries sets maximum retry attempts
func WithMaxRetries(retries int) WebSocketOption {
    return func(c *WebSocketConfig) {
        c.MaxRetries = retries
    }
}
```

### 4. Error Handling

**errors.go**:
```go
package thunderpropagator

import (
    "errors"
    "fmt"
)

var (
    // ErrAlreadyConnected indicates connection already established
    ErrAlreadyConnected = errors.New("already connected")
    
    // ErrNotConnected indicates no active connection
    ErrNotConnected = errors.New("not connected")
    
    // ErrConnectionFailed indicates connection attempt failed
    ErrConnectionFailed = errors.New("connection failed")
    
    // ErrSendFailed indicates send operation failed
    ErrSendFailed = errors.New("send failed")
    
    // ErrTimeout indicates operation timed out
    ErrTimeout = errors.New("operation timed out")
    
    // ErrInvalidState indicates invalid state for operation
    ErrInvalidState = errors.New("invalid state")
    
    // ErrChannelNotFound indicates channel does not exist
    ErrChannelNotFound = errors.New("channel not found")
)

// ConnectionError wraps connection-related errors
type ConnectionError struct {
    Op  string
    Err error
}

func (e *ConnectionError) Error() string {
    if e.Err != nil {
        return fmt.Sprintf("connection error during %s: %v", e.Op, e.Err)
    }
    return fmt.Sprintf("connection error during %s", e.Op)
}

func (e *ConnectionError) Unwrap() error {
    return e.Err
}

// ChannelError wraps channel-related errors
type ChannelError struct {
    Channel string
    Op      string
    Err     error
}

func (e *ChannelError) Error() string {
    if e.Err != nil {
        return fmt.Sprintf("channel '%s' error during %s: %v", e.Channel, e.Op, e.Err)
    }
    return fmt.Sprintf("channel '%s' error during %s", e.Channel, e.Op)
}

func (e *ChannelError) Unwrap() error {
    return e.Err
}
```

---

## Implementation Details

### WebSocket Connection

**connections/websocket.go**:
```go
package connections

import (
    "context"
    "fmt"
    "net/http"
    "sync"
    "time"
    
    "github.com/gorilla/websocket"
    "github.com/yourusername/thunderpropagator-go"
)

// WebSocketConnection implements Connection for WebSocket protocol
type WebSocketConnection struct {
    *thunderpropagator.AbstractConnection
    config *thunderpropagator.WebSocketConfig
    conn   *websocket.Conn
    mu     sync.RWMutex
    stopCh chan struct{}
}

// NewWebSocketConnection creates a new WebSocket connection
func NewWebSocketConnection(config *thunderpropagator.WebSocketConfig) *WebSocketConnection {
    return &WebSocketConnection{
        AbstractConnection: thunderpropagator.NewAbstractConnection(),
        config:            config,
        stopCh:            make(chan struct{}),
    }
}

// Connect establishes WebSocket connection
func (c *WebSocketConnection) Connect(ctx context.Context) error {
    c.mu.Lock()
    defer c.mu.Unlock()
    
    if c.GetState() != thunderpropagator.ConnectionStateReady {
        return thunderpropagator.ErrAlreadyConnected
    }
    
    c.SetState(thunderpropagator.ConnectionStateConnecting)
    
    dialer := websocket.Dialer{
        HandshakeTimeout: c.config.ConnectionTimeout,
    }
    
    conn, resp, err := dialer.DialContext(ctx, c.config.URI, c.config.Headers)
    if err != nil {
        c.SetState(thunderpropagator.ConnectionStateHasError)
        return &thunderpropagator.ConnectionError{
            Op:  "dial",
            Err: err,
        }
    }
    defer resp.Body.Close()
    
    c.conn = conn
    c.SetState(thunderpropagator.ConnectionStateOpen)
    
    // Start receive loop
    go c.receiveLoop()
    
    // Start keep-alive loop
    go c.keepAliveLoop()
    
    return nil
}

// Disconnect closes WebSocket connection
func (c *WebSocketConnection) Disconnect(ctx context.Context) error {
    c.mu.Lock()
    defer c.mu.Unlock()
    
    if c.conn == nil {
        return thunderpropagator.ErrNotConnected
    }
    
    c.SetState(thunderpropagator.ConnectionStateClosing)
    
    close(c.stopCh)
    
    err := c.conn.WriteMessage(
        websocket.CloseMessage,
        websocket.FormatCloseMessage(websocket.CloseNormalClosure, ""),
    )
    if err != nil {
        return &thunderpropagator.ConnectionError{
            Op:  "close",
            Err: err,
        }
    }
    
    err = c.conn.Close()
    c.conn = nil
    c.SetState(thunderpropagator.ConnectionStateClosed)
    
    return err
}

// Send sends a message through WebSocket
func (c *WebSocketConnection) Send(ctx context.Context, message string) error {
    c.mu.RLock()
    defer c.mu.RUnlock()
    
    if c.conn == nil || c.GetState() != thunderpropagator.ConnectionStateOpen {
        return thunderpropagator.ErrNotConnected
    }
    
    err := c.conn.WriteMessage(websocket.TextMessage, []byte(message))
    if err != nil {
        return &thunderpropagator.ConnectionError{
            Op:  "send",
            Err: err,
        }
    }
    
    return nil
}

// IsConnected checks if WebSocket is connected
func (c *WebSocketConnection) IsConnected() bool {
    return c.GetState() == thunderpropagator.ConnectionStateOpen
}

// GetConnectionID returns connection identifier
func (c *WebSocketConnection) GetConnectionID() string {
    c.mu.RLock()
    defer c.mu.RUnlock()
    return c.connectionID
}

// receiveLoop continuously receives messages
func (c *WebSocketConnection) receiveLoop() {
    defer func() {
        if r := recover(); r != nil {
            c.SetState(thunderpropagator.ConnectionStateHasError)
        }
    }()
    
    for {
        select {
        case <-c.stopCh:
            return
        default:
            c.mu.RLock()
            conn := c.conn
            c.mu.RUnlock()
            
            if conn == nil {
                return
            }
            
            conn.SetReadDeadline(time.Now().Add(24 * time.Hour))
            
            _, message, err := conn.ReadMessage()
            if err != nil {
                if websocket.IsUnexpectedCloseError(err, websocket.CloseGoingAway, websocket.CloseNormalClosure) {
                    c.SetState(thunderpropagator.ConnectionStateHasError)
                }
                return
            }
            
            messageStr := string(message)
            if messageStr != "PROBE" {
                c.EmitMessage(messageStr)
            }
        }
    }
}

// keepAliveLoop sends periodic keep-alive messages
func (c *WebSocketConnection) keepAliveLoop() {
    ticker := time.NewTicker(c.config.KeepAliveInterval)
    defer ticker.Stop()
    
    for {
        select {
        case <-c.stopCh:
            return
        case <-ticker.C:
            if c.IsConnected() {
                err := c.Send(context.Background(), "PING")
                if err != nil {
                    c.SetState(thunderpropagator.ConnectionStateHasError)
                    return
                }
            }
        }
    }
}
```

### Channel Implementation

**channel.go**:
```go
package thunderpropagator

import (
    "context"
    "encoding/json"
    "fmt"
    "sync"
    "time"
    
    "github.com/google/uuid"
    "github.com/yourusername/thunderpropagator-go/models"
)

// Channel represents a logical communication channel
type Channel struct {
    mu              sync.RWMutex
    name            string
    state           models.ChannelState
    connection      Connection
    metadata        *models.ChannelMetadata
    pendingRequests map[uuid.UUID]chan json.RawMessage
}

// NewChannel creates a new channel
func NewChannel(name string, connection Connection) *Channel {
    return &Channel{
        name:            name,
        state:           models.ChannelStateReady,
        connection:      connection,
        pendingRequests: make(map[uuid.UUID]chan json.RawMessage),
    }
}

// Open opens the channel
func (c *Channel) Open(ctx context.Context) error {
    c.mu.Lock()
    if c.state != models.ChannelStateReady {
        c.mu.Unlock()
        return &ChannelError{
            Channel: c.name,
            Op:      "open",
            Err:     ErrInvalidState,
        }
    }
    c.state = models.ChannelStateOpening
    c.mu.Unlock()
    
    request := map[string]interface{}{
        "route": map[string]string{
            "channel":  c.name,
            "endpoint": "open",
        },
        "timestamp": time.Now().Format(time.RFC3339),
    }
    
    requestJSON, err := json.Marshal(request)
    if err != nil {
        return &ChannelError{
            Channel: c.name,
            Op:      "open",
            Err:     err,
        }
    }
    
    if err := c.connection.Send(ctx, string(requestJSON)); err != nil {
        c.mu.Lock()
        c.state = models.ChannelStateReady
        c.mu.Unlock()
        return &ChannelError{
            Channel: c.name,
            Op:      "open",
            Err:     err,
        }
    }
    
    // Wait for metadata
    if err := c.waitForMetadata(ctx); err != nil {
        return &ChannelError{
            Channel: c.name,
            Op:      "open",
            Err:     err,
        }
    }
    
    c.mu.Lock()
    c.state = models.ChannelStateOpen
    c.mu.Unlock()
    
    return nil
}

// Close closes the channel
func (c *Channel) Close(ctx context.Context) error {
    c.mu.Lock()
    if c.state == models.ChannelStateClosed {
        c.mu.Unlock()
        return nil
    }
    c.state = models.ChannelStateClosing
    c.mu.Unlock()
    
    request := map[string]interface{}{
        "route": map[string]string{
            "channel":  c.name,
            "endpoint": "close",
        },
    }
    
    requestJSON, err := json.Marshal(request)
    if err != nil {
        return &ChannelError{
            Channel: c.name,
            Op:      "close",
            Err:     err,
        }
    }
    
    if err := c.connection.Send(ctx, string(requestJSON)); err != nil {
        return &ChannelError{
            Channel: c.name,
            Op:      "close",
            Err:     err,
        }
    }
    
    c.mu.Lock()
    c.state = models.ChannelStateClosed
    c.mu.Unlock()
    
    return nil
}

// Subscribe creates a subscription on the channel
func (c *Channel) Subscribe(ctx context.Context, subscription *models.SubscriptionRequest) (*models.SubscriptionResponse, error) {
    requestID := uuid.New()
    responseCh := make(chan json.RawMessage, 1)
    
    c.mu.Lock()
    c.pendingRequests[requestID] = responseCh
    c.mu.Unlock()
    
    defer func() {
        c.mu.Lock()
        delete(c.pendingRequests, requestID)
        c.mu.Unlock()
    }()
    
    request := map[string]interface{}{
        "id": requestID.String(),
        "route": map[string]string{
            "channel":  c.name,
            "endpoint": "subscribe",
        },
        "payload": subscription,
    }
    
    requestJSON, err := json.Marshal(request)
    if err != nil {
        return nil, &ChannelError{
            Channel: c.name,
            Op:      "subscribe",
            Err:     err,
        }
    }
    
    if err := c.connection.Send(ctx, string(requestJSON)); err != nil {
        return nil, &ChannelError{
            Channel: c.name,
            Op:      "subscribe",
            Err:     err,
        }
    }
    
    select {
    case <-ctx.Done():
        return nil, &ChannelError{
            Channel: c.name,
            Op:      "subscribe",
            Err:     ctx.Err(),
        }
    case responseData := <-responseCh:
        var response models.SubscriptionResponse
        if err := json.Unmarshal(responseData, &response); err != nil {
            return nil, &ChannelError{
                Channel: c.name,
                Op:      "subscribe",
                Err:     err,
            }
        }
        return &response, nil
    case <-time.After(30 * time.Second):
        return nil, &ChannelError{
            Channel: c.name,
            Op:      "subscribe",
            Err:     ErrTimeout,
        }
    }
}

// HandleMessage processes received message
func (c *Channel) HandleMessage(message json.RawMessage) error {
    var envelope struct {
        ID       string          `json:"id"`
        Metadata json.RawMessage `json:"metadata"`
        Payload  json.RawMessage `json:"payload"`
    }
    
    if err := json.Unmarshal(message, &envelope); err != nil {
        return fmt.Errorf("failed to unmarshal envelope: %w", err)
    }
    
    // Check if it's a response to pending request
    if envelope.ID != "" {
        if requestID, err := uuid.Parse(envelope.ID); err == nil {
            c.mu.RLock()
            if ch, exists := c.pendingRequests[requestID]; exists {
                c.mu.RUnlock()
                select {
                case ch <- envelope.Payload:
                default:
                }
                return nil
            }
            c.mu.RUnlock()
        }
    }
    
    // Check if it's metadata
    if len(envelope.Metadata) > 0 {
        var metadata models.ChannelMetadata
        if err := json.Unmarshal(envelope.Metadata, &metadata); err == nil {
            c.mu.Lock()
            c.metadata = &metadata
            c.mu.Unlock()
            return nil
        }
    }
    
    // Regular message
    return nil
}

// waitForMetadata waits for channel metadata
func (c *Channel) waitForMetadata(ctx context.Context) error {
    timeout := time.After(30 * time.Second)
    ticker := time.NewTicker(100 * time.Millisecond)
    defer ticker.Stop()
    
    for {
        select {
        case <-ctx.Done():
            return ctx.Err()
        case <-timeout:
            return ErrTimeout
        case <-ticker.C:
            c.mu.RLock()
            hasMetadata := c.metadata != nil
            c.mu.RUnlock()
            if hasMetadata {
                return nil
            }
        }
    }
}

// GetState returns current channel state
func (c *Channel) GetState() models.ChannelState {
    c.mu.RLock()
    defer c.mu.RUnlock()
    return c.state
}

// GetName returns channel name
func (c *Channel) GetName() string {
    return c.name
}
```

### Client Implementation

**client.go**:
```go
package thunderpropagator

import (
    "context"
    "sync"
)

// Client provides high-level API for ThunderPropagator
type Client struct {
    mu         sync.RWMutex
    connection Connection
    channels   map[string]*Channel
}

// NewClient creates a new ThunderPropagator client
func NewClient(connection Connection) *Client {
    return &Client{
        connection: connection,
        channels:   make(map[string]*Channel),
    }
}

// Connect establishes connection
func (c *Client) Connect(ctx context.Context) error {
    return c.connection.Connect(ctx)
}

// Disconnect closes connection
func (c *Client) Disconnect(ctx context.Context) error {
    // Close all channels first
    c.mu.Lock()
    channels := make([]*Channel, 0, len(c.channels))
    for _, ch := range c.channels {
        channels = append(channels, ch)
    }
    c.mu.Unlock()
    
    for _, ch := range channels {
        _ = ch.Close(ctx)
    }
    
    return c.connection.Disconnect(ctx)
}

// CreateChannel creates or retrieves a channel
func (c *Client) CreateChannel(ctx context.Context, name string) (*Channel, error) {
    c.mu.Lock()
    if ch, exists := c.channels[name]; exists {
        c.mu.Unlock()
        return ch, nil
    }
    c.mu.Unlock()
    
    channel := NewChannel(name, c.connection)
    if err := channel.Open(ctx); err != nil {
        return nil, err
    }
    
    c.mu.Lock()
    c.channels[name] = channel
    c.mu.Unlock()
    
    return channel, nil
}

// GetChannel retrieves an existing channel
func (c *Client) GetChannel(name string) (*Channel, error) {
    c.mu.RLock()
    defer c.mu.RUnlock()
    
    if ch, exists := c.channels[name]; exists {
        return ch, nil
    }
    
    return nil, ErrChannelNotFound
}

// WebSocketClient is a WebSocket-specific client
type WebSocketClient struct {
    *Client
}

// NewWebSocketClient creates a new WebSocket client
func NewWebSocketClient(config *WebSocketConfig) *WebSocketClient {
    conn := connections.NewWebSocketConnection(config)
    client := NewClient(conn)
    
    return &WebSocketClient{
        Client: client,
    }
}
```

---

## Usage Example

**examples/websocket/main.go**:
```go
package main

import (
    "context"
    "fmt"
    "log"
    "time"
    
    tp "github.com/yourusername/thunderpropagator-go"
    "github.com/yourusername/thunderpropagator-go/models"
)

func main() {
    // Create configuration
    config := tp.NewWebSocketConfig(
        "wss://example.com/thunder",
        tp.WithHeader("Authorization", "Bearer token123"),
        tp.WithConnectionTimeout(30*time.Second),
        tp.WithKeepAliveInterval(20*time.Second),
    )
    
    // Create client
    client := tp.NewWebSocketClient(config)
    
    // Connect
    ctx := context.Background()
    if err := client.Connect(ctx); err != nil {
        log.Fatalf("Failed to connect: %v", err)
    }
    defer client.Disconnect(ctx)
    
    fmt.Println("Connected to ThunderPropagator")
    
    // Create channel
    channel, err := client.CreateChannel(ctx, "market-data")
    if err != nil {
        log.Fatalf("Failed to create channel: %v", err)
    }
    
    fmt.Printf("Channel '%s' opened\n", channel.GetName())
    
    // Subscribe
    subscription := &models.SubscriptionRequest{
        DataType: "quotes",
        Symbols:  []string{"AAPL", "GOOGL"},
    }
    
    response, err := channel.Subscribe(ctx, subscription)
    if err != nil {
        log.Fatalf("Failed to subscribe: %v", err)
    }
    
    fmt.Printf("Subscribed with ID: %s\n", response.SubscriptionID)
    
    // Keep running
    time.Sleep(60 * time.Second)
}
```

---

## Testing

**tests/unit_test.go**:
```go
package tests

import (
    "context"
    "testing"
    "time"
    
    tp "github.com/yourusername/thunderpropagator-go"
)

func TestWebSocketConnection(t *testing.T) {
    config := tp.NewWebSocketConfig("wss://echo.websocket.org")
    conn := connections.NewWebSocketConnection(config)
    
    ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
    defer cancel()
    
    // Test connect
    if err := conn.Connect(ctx); err != nil {
        t.Fatalf("Failed to connect: %v", err)
    }
    
    if !conn.IsConnected() {
        t.Error("Expected connection to be established")
    }
    
    // Test send
    if err := conn.Send(ctx, "test"); err != nil {
        t.Errorf("Failed to send: %v", err)
    }
    
    // Test disconnect
    if err := conn.Disconnect(ctx); err != nil {
        t.Errorf("Failed to disconnect: %v", err)
    }
}
```

---

## Go Module Publishing Guide

### 1. Prerequisites

```bash
# Install Go
# Download from https://go.dev/dl/

# Verify installation
go version

# Initialize module
go mod init github.com/yourusername/thunderpropagator-go
```

### 2. Prepare go.mod

Ensure proper module path and Go version:
```go
module github.com/yourusername/thunderpropagator-go

go 1.21

require (
    // Dependencies listed here
)
```

### 3. Version Tagging

Go modules use Git tags for versioning:
```bash
# Commit all changes
git add .
git commit -m "Release v1.0.0"

# Create tag
git tag v1.0.0

# Push with tags
git push origin main
git push origin v1.0.0
```

### 4. Publishing to pkg.go.dev

**pkg.go.dev automatically indexes public Go modules from GitHub.**

No manual upload needed! Just push your tags.

```bash
# Trigger indexing (optional)
# Visit: https://pkg.go.dev/github.com/yourusername/thunderpropagator-go@v1.0.0

# Or use proxy
GOPROXY=proxy.golang.org go list -m github.com/yourusername/thunderpropagator-go@v1.0.0
```

### 5. Pre-Release Checks

```bash
# Format code
go fmt ./...

# Vet code
go vet ./...

# Run tests
go test ./... -v

# Check dependencies
go mod tidy
go mod verify

# Build
go build ./...

# Test installation
go install github.com/yourusername/thunderpropagator-go@v1.0.0
```

### 6. Version Management

**Semantic Versioning**:
- `v1.0.0` - Major release
- `v1.1.0` - Minor release (new features)
- `v1.0.1` - Patch release (bug fixes)

**Pre-releases**:
- `v1.0.0-alpha.1`
- `v1.0.0-beta.1`
- `v1.0.0-rc.1`

**Version 2+**:
```bash
# For major version 2+, update module path
module github.com/yourusername/thunderpropagator-go/v2

# Tag with v2
git tag v2.0.0
git push origin v2.0.0
```

### 7. Automated Publishing with GitHub Actions

**.github/workflows/release.yml**:
```yaml
name: Release

on:
  push:
    tags:
      - 'v*'

jobs:
  release:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout code
        uses: actions/checkout@v4
      
      - name: Set up Go
        uses: actions/setup-go@v4
        with:
          go-version: '1.21'
      
      - name: Run tests
        run: go test ./... -v
      
      - name: Build
        run: go build ./...
      
      - name: Verify module
        run: |
          go mod tidy
          go mod verify
      
      - name: Create GitHub Release
        uses: softprops/action-gh-release@v1
        with:
          generate_release_notes: true
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      
      - name: Trigger pkg.go.dev indexing
        run: |
          TAG=${GITHUB_REF#refs/tags/}
          curl "https://proxy.golang.org/github.com/yourusername/thunderpropagator-go/@v/${TAG}.info"
```

### 8. Documentation

Add comprehensive package documentation:
```go
// Package thunderpropagator provides a client library for ThunderPropagator
// real-time data streaming framework.
//
// Example usage:
//
//     config := thunderpropagator.NewWebSocketConfig("wss://example.com")
//     client := thunderpropagator.NewWebSocketClient(config)
//     
//     if err := client.Connect(context.Background()); err != nil {
//         log.Fatal(err)
//     }
//     defer client.Disconnect(context.Background())
package thunderpropagator
```

### 9. README Badges

```markdown
[![Go Reference](https://pkg.go.dev/badge/github.com/yourusername/thunderpropagator-go.svg)](https://pkg.go.dev/github.com/yourusername/thunderpropagator-go)
[![Go Report Card](https://goreportcard.com/badge/github.com/yourusername/thunderpropagator-go)](https://goreportcard.com/report/github.com/yourusername/thunderpropagator-go)
[![License](https://img.shields.io/github/license/yourusername/thunderpropagator-go)](LICENSE)
```

### 10. License

Add LICENSE file (MIT recommended):
```
MIT License

Copyright (c) 2025 Your Name

Permission is hereby granted, free of charge, to any person obtaining a copy...
```

---

## Key Go-Specific Considerations

1. **Interfaces**: Idiomatic Go uses small, focused interfaces
2. **Error Handling**: Explicit error returns, use `errors.Is()` and `errors.As()`
3. **Concurrency**: Goroutines and channels for async operations
4. **Context**: Use `context.Context` for cancellation and timeouts
5. **Sync**: `sync.Mutex`, `sync.RWMutex` for thread safety
6. **Testing**: Built-in testing package with table-driven tests
7. **Modules**: Go modules for dependency management

---

## Implementation Checklist

- [ ] Initialize Go module with proper path
- [ ] Implement `Connection` interface
- [ ] Implement `WebSocketConnection`
- [ ] Implement `QuicConnection`
- [ ] Implement `InfiniteDataStreamConnection`
- [ ] Implement `Channel` with proper synchronization
- [ ] Implement `Client` and protocol-specific clients
- [ ] Define all models with proper JSON tags
- [ ] Implement error types
- [ ] Implement crypto utilities
- [ ] Write unit tests
- [ ] Write integration tests
- [ ] Add package documentation
- [ ] Create usage examples
- [ ] Run `go fmt`, `go vet`
- [ ] Test with `go test -race`
- [ ] Create README with examples
- [ ] Add LICENSE file
- [ ] Tag first version
- [ ] Verify on pkg.go.dev
