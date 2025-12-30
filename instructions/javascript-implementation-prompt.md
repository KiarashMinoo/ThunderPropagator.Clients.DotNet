# ThunderPropagator JavaScript/TypeScript Client - Implementation Prompt

## Project Goal
Create a JavaScript/TypeScript client library for ThunderPropagator real-time data streaming framework supporting WebSocket, QUIC (WebTransport), and custom protocols.

## Target Environment
- **TypeScript Version**: 5.0+
- **Target Runtimes**: Node.js 18+, Browser (ES2020+)
- **Build Tool**: Vite or esbuild
- **Package Manager**: npm/pnpm
- **Testing**: Vitest or Jest

---

## Project Structure

```
thunderpropagator-js/
├── package.json
├── tsconfig.json
├── vite.config.ts
├── README.md
├── LICENSE
├── .gitignore
├── src/
│   ├── index.ts
│   ├── client.ts
│   ├── connections/
│   │   ├── index.ts
│   │   ├── AbstractConnection.ts
│   │   ├── WebSocketConnection.ts
│   │   ├── QuicConnection.ts
│   │   └── InfiniteDataStreamConnection.ts
│   ├── channels/
│   │   ├── index.ts
│   │   ├── AbstractChannel.ts
│   │   └── Channel.ts
│   ├── models/
│   │   ├── index.ts
│   │   ├── Configuration.ts
│   │   ├── Enums.ts
│   │   ├── Messages.ts
│   │   ├── Metadata.ts
│   │   └── Subscriptions.ts
│   ├── crypto/
│   │   ├── index.ts
│   │   └── encryption.ts
│   └── types/
│       ├── index.ts
│       └── events.ts
├── tests/
│   ├── connections.test.ts
│   ├── channels.test.ts
│   └── integration.test.ts
└── examples/
    ├── websocket.ts
    ├── quic.ts
    └── node-example.ts
```

---

## Core Dependencies

```json
{
  "name": "@thunderpropagator/client",
  "version": "1.0.0",
  "type": "module",
  "main": "./dist/index.cjs",
  "module": "./dist/index.js",
  "types": "./dist/index.d.ts",
  "exports": {
    ".": {
      "import": "./dist/index.js",
      "require": "./dist/index.cjs",
      "types": "./dist/index.d.ts"
    }
  },
  "dependencies": {
    "eventemitter3": "^5.0.0",
    "ws": "^8.16.0"
  },
  "devDependencies": {
    "@types/node": "^20.10.0",
    "@types/ws": "^8.5.0",
    "typescript": "^5.3.0",
    "vite": "^5.0.0",
    "vitest": "^1.0.0",
    "tsx": "^4.7.0"
  },
  "peerDependencies": {
    "@fails-components/webtransport": "^0.2.0"
  },
  "peerDependenciesMeta": {
    "@fails-components/webtransport": {
      "optional": true
    }
  }
}
```

---

## TypeScript Configuration

```json
{
  "compilerOptions": {
    "target": "ES2020",
    "module": "ESNext",
    "lib": ["ES2020", "DOM"],
    "moduleResolution": "bundler",
    "strict": true,
    "esModuleInterop": true,
    "skipLibCheck": true,
    "declaration": true,
    "declarationMap": true,
    "sourceMap": true,
    "outDir": "dist",
    "rootDir": "src",
    "resolveJsonModule": true,
    "allowSyntheticDefaultImports": true,
    "forceConsistentCasingInFileNames": true
  },
  "include": ["src/**/*"],
  "exclude": ["node_modules", "dist", "tests"]
}
```

---

## Design Patterns & Idioms

### 1. Abstract Base Classes with Generics

```typescript
export abstract class AbstractConnection<TConfig extends BaseConfiguration> {
  protected config: TConfig;
  protected state: ConnectionState = ConnectionState.Ready;
  protected connectionId: string | null = null;
  protected emitter = new EventEmitter();

  constructor(config: TConfig) {
    this.config = config;
  }

  abstract connectAsync(): Promise<void>;
  abstract disconnectAsync(): Promise<void>;
  abstract sendAsync(message: string): Promise<void>;

  protected abstract startReceiveLoop(): void;

  get isConnected(): boolean {
    return this.state === ConnectionState.Open;
  }

  on(event: 'stateChanged', listener: (old: ConnectionState, new: ConnectionState) => void): this;
  on(event: 'messageReceived', listener: (message: string) => void): this;
  on(event: 'error', listener: (error: Error) => void): this;
  on(event: string, listener: (...args: any[]) => void): this {
    this.emitter.on(event, listener);
    return this;
  }

  protected setState(newState: ConnectionState): void {
    const oldState = this.state;
    this.state = newState;
    this.emitter.emit('stateChanged', oldState, newState);
  }

  protected notifyMessageReceived(message: string): void {
    // Filter PROBE messages
    if (message.trim() === 'PROBE') return;
    this.emitter.emit('messageReceived', message);
  }
}
```

### 2. Interfaces for Configuration

```typescript
export interface BaseConfiguration {
  connectionTimeout?: number;
  reconnectEnabled?: boolean;
  reconnectMaxAttempts?: number;
}

export interface WebSocketConfiguration extends BaseConfiguration {
  uri: string;
  headers?: Record<string, string>;
  keepAliveInterval?: number;
  protocols?: string[];
}

export interface QuicConfiguration extends BaseConfiguration {
  url: string;
  serverCertificateHashes?: Uint8Array[];
}

export interface InfiniteDataStreamConfiguration extends BaseConfiguration {
  host: string;
  port: number;
  compressionEnabled?: boolean;
  chunkSize?: number;
}
```

### 3. Enums

```typescript
export enum ConnectionState {
  Ready = 'ready',
  Connecting = 'connecting',
  Open = 'open',
  Closing = 'closing',
  Closed = 'closed',
  HasError = 'hasError',
}

export enum ChannelState {
  Ready = 'ready',
  Opening = 'opening',
  Open = 'open',
  Closing = 'closing',
  Closed = 'closed',
}

export enum ProtocolType {
  WebSocket = 'websocket',
  Quic = 'quic',
  InfiniteDataStream = 'infiniteDataStream',
}
```

### 4. Type-Safe Events with EventEmitter

```typescript
import EventEmitter from 'eventemitter3';

interface ConnectionEvents {
  stateChanged: (oldState: ConnectionState, newState: ConnectionState) => void;
  messageReceived: (message: string) => void;
  error: (error: Error) => void;
}

export class TypedEventEmitter<T extends Record<string, any>> {
  private emitter = new EventEmitter();

  on<K extends keyof T>(event: K, listener: T[K]): this {
    this.emitter.on(event as string, listener);
    return this;
  }

  off<K extends keyof T>(event: K, listener: T[K]): this {
    this.emitter.off(event as string, listener);
    return this;
  }

  emit<K extends keyof T>(event: K, ...args: Parameters<T[K]>): boolean {
    return this.emitter.emit(event as string, ...args);
  }
}
```

---

## Implementation Details

### Connection Layer

#### Base Connection (`connections/AbstractConnection.ts`)

```typescript
import EventEmitter from 'eventemitter3';
import { ConnectionState } from '../models/Enums';
import { BaseConfiguration } from '../models/Configuration';
import { ConnectionResponse } from '../models/Messages';

export abstract class AbstractConnection<TConfig extends BaseConfiguration> {
  protected config: TConfig;
  protected state: ConnectionState = ConnectionState.Ready;
  protected connectionId: string | null = null;
  protected connectionInfo: ConnectionResponse | null = null;
  protected emitter = new EventEmitter();
  protected receiveLoopController: AbortController | null = null;

  constructor(config: TConfig) {
    this.config = config;
  }

  // Abstract methods for protocol-specific implementations
  protected abstract connectImpl(): Promise<void>;
  protected abstract disconnectImpl(): Promise<void>;
  protected abstract sendImpl(message: string): Promise<void>;
  protected abstract receiveLoop(signal: AbortSignal): Promise<void>;

  async connectAsync(): Promise<void> {
    if (this.state !== ConnectionState.Ready) {
      throw new Error(`Cannot connect from state ${this.state}`);
    }

    this.setState(ConnectionState.Connecting);
    try {
      await this.connectImpl();
      
      // Start receive loop
      this.receiveLoopController = new AbortController();
      this.receiveLoop(this.receiveLoopController.signal).catch((err) => {
        if (err.name !== 'AbortError') {
          this.handleError(err);
        }
      });

      this.setState(ConnectionState.Open);
    } catch (error) {
      this.setState(ConnectionState.HasError);
      this.handleError(error as Error);
      throw error;
    }
  }

  async disconnectAsync(): Promise<void> {
    if (this.state === ConnectionState.Closed || this.state === ConnectionState.Closing) {
      return;
    }

    this.setState(ConnectionState.Closing);

    // Stop receive loop
    if (this.receiveLoopController) {
      this.receiveLoopController.abort();
      this.receiveLoopController = null;
    }

    await this.disconnectImpl();
    this.setState(ConnectionState.Closed);
  }

  async sendAsync(message: string): Promise<void> {
    if (!this.isConnected) {
      throw new Error('Connection is not open');
    }
    await this.sendImpl(message);
  }

  get isConnected(): boolean {
    return this.state === ConnectionState.Open;
  }

  protected setState(newState: ConnectionState): void {
    const oldState = this.state;
    this.state = newState;
    this.emitter.emit('stateChanged', oldState, newState);
  }

  protected notifyMessageReceived(message: string): void {
    // Filter PROBE messages
    if (message.trim() === 'PROBE') return;
    this.emitter.emit('messageReceived', message);
  }

  protected handleError(error: Error): void {
    this.emitter.emit('error', error);
  }

  // Event registration
  on(event: 'stateChanged', listener: (old: ConnectionState, new: ConnectionState) => void): this;
  on(event: 'messageReceived', listener: (message: string) => void): this;
  on(event: 'error', listener: (error: Error) => void): this;
  on(event: string, listener: (...args: any[]) => void): this {
    this.emitter.on(event, listener);
    return this;
  }

  off(event: string, listener: (...args: any[]) => void): this {
    this.emitter.off(event, listener);
    return this;
  }
}
```

#### WebSocket Connection (`connections/WebSocketConnection.ts`)

```typescript
import WebSocket from 'ws';
import { AbstractConnection } from './AbstractConnection';
import { WebSocketConfiguration } from '../models/Configuration';

export class WebSocketConnection extends AbstractConnection<WebSocketConfiguration> {
  private ws: WebSocket | null = null;

  protected async connectImpl(): Promise<void> {
    return new Promise((resolve, reject) => {
      const { uri, headers, protocols } = this.config;
      
      this.ws = new WebSocket(uri, protocols, {
        headers,
        handshakeTimeout: this.config.connectionTimeout || 30000,
      });

      this.ws.on('open', () => {
        console.log(`WebSocket connected to ${uri}`);
        resolve();
      });

      this.ws.on('error', (error) => {
        reject(error);
      });
    });
  }

  protected async disconnectImpl(): Promise<void> {
    if (this.ws) {
      this.ws.close();
      this.ws = null;
    }
  }

  protected async sendImpl(message: string): Promise<void> {
    if (!this.ws) {
      throw new Error('WebSocket not connected');
    }
    
    return new Promise((resolve, reject) => {
      this.ws!.send(message, (error) => {
        if (error) reject(error);
        else resolve();
      });
    });
  }

  protected async receiveLoop(signal: AbortSignal): Promise<void> {
    if (!this.ws) return;

    this.ws.on('message', (data) => {
      if (signal.aborted) return;
      const message = data.toString();
      this.notifyMessageReceived(message);
    });

    this.ws.on('close', () => {
      if (!signal.aborted) {
        this.setState(ConnectionState.Closed);
      }
    });

    this.ws.on('error', (error) => {
      if (!signal.aborted) {
        this.handleError(error);
      }
    });

    // Wait for abort signal
    return new Promise((resolve) => {
      signal.addEventListener('abort', () => resolve());
    });
  }
}
```

#### QUIC/WebTransport Connection (`connections/QuicConnection.ts`)

```typescript
import { AbstractConnection } from './AbstractConnection';
import { QuicConfiguration } from '../models/Configuration';

// WebTransport types (requires browser or polyfill)
declare global {
  interface WebTransport {
    ready: Promise<void>;
    closed: Promise<WebTransportCloseInfo>;
    datagrams: {
      writable: WritableStream<Uint8Array>;
      readable: ReadableStream<Uint8Array>;
    };
    createBidirectionalStream(): Promise<WebTransportBidirectionalStream>;
    close(closeInfo?: WebTransportCloseInfo): void;
  }

  interface WebTransportBidirectionalStream {
    readable: ReadableStream<Uint8Array>;
    writable: WritableStream<Uint8Array>;
  }

  var WebTransport: {
    prototype: WebTransport;
    new (url: string, options?: any): WebTransport;
  };
}

export class QuicConnection extends AbstractConnection<QuicConfiguration> {
  private transport: WebTransport | null = null;
  private stream: WebTransportBidirectionalStream | null = null;
  private writer: WritableStreamDefaultWriter<Uint8Array> | null = null;

  protected async connectImpl(): Promise<void> {
    const { url, serverCertificateHashes } = this.config;

    this.transport = new WebTransport(url, {
      serverCertificateHashes,
    });

    await this.transport.ready;
    console.log(`QUIC/WebTransport connected to ${url}`);

    // Create bidirectional stream
    this.stream = await this.transport.createBidirectionalStream();
    this.writer = this.stream.writable.getWriter();
  }

  protected async disconnectImpl(): Promise<void> {
    if (this.writer) {
      await this.writer.close();
      this.writer = null;
    }

    if (this.transport) {
      this.transport.close();
      await this.transport.closed;
      this.transport = null;
    }
  }

  protected async sendImpl(message: string): Promise<void> {
    if (!this.writer) {
      throw new Error('QUIC connection not established');
    }

    const encoder = new TextEncoder();
    const data = encoder.encode(message);
    await this.writer.write(data);
  }

  protected async receiveLoop(signal: AbortSignal): Promise<void> {
    if (!this.stream) return;

    const reader = this.stream.readable.getReader();
    const decoder = new TextDecoder();

    try {
      while (!signal.aborted) {
        const { value, done } = await reader.read();
        
        if (done || signal.aborted) break;
        
        const message = decoder.decode(value, { stream: true });
        this.notifyMessageReceived(message);
      }
    } finally {
      reader.releaseLock();
    }
  }
}
```

### Channel Layer

#### Abstract Channel (`channels/AbstractChannel.ts`)

```typescript
import EventEmitter from 'eventemitter3';
import { v4 as uuidv4 } from 'uuid';
import { AbstractConnection } from '../connections/AbstractConnection';
import { ChannelState } from '../models/Enums';
import { ChannelMetadata } from '../models/Metadata';
import { FeederMessage } from '../models/Messages';
import { SubscriptionRequest, SubscriptionResponse } from '../models/Subscriptions';

export class AbstractChannel {
  readonly name: string;
  protected connection: AbstractConnection<any>;
  protected state: ChannelState = ChannelState.Ready;
  protected channelId: string | null = null;
  protected metadata: ChannelMetadata | null = null;
  protected emitter = new EventEmitter();
  protected pendingRequests = new Map<string, {
    resolve: (value: any) => void;
    reject: (error: Error) => void;
    timeout: NodeJS.Timeout;
  }>();

  constructor(name: string, connection: AbstractConnection<any>) {
    this.name = name;
    this.connection = connection;
  }

  get isOpen(): boolean {
    return this.state === ChannelState.Open;
  }

  async openAsync(): Promise<void> {
    if (this.state !== ChannelState.Ready) {
      throw new Error(`Cannot open channel from state ${this.state}`);
    }

    this.setState(ChannelState.Opening);

    const openRequest = {
      route: { channel: this.name, endpoint: 'open' },
      timestamp: new Date().toISOString(),
    };

    await this.connection.sendAsync(JSON.stringify(openRequest));

    // Wait for metadata (with timeout)
    await this.waitForOpen(30000);
    this.setState(ChannelState.Open);
  }

  async closeAsync(): Promise<void> {
    if (this.state === ChannelState.Closed) return;

    this.setState(ChannelState.Closing);

    const closeRequest = {
      route: { channel: this.name, endpoint: 'close' },
    };

    await this.connection.sendAsync(JSON.stringify(closeRequest));
    this.setState(ChannelState.Closed);

    // Cancel all pending requests
    for (const [id, pending] of this.pendingRequests) {
      clearTimeout(pending.timeout);
      pending.reject(new Error('Channel closed'));
    }
    this.pendingRequests.clear();
  }

  async subscribeAsync(subscription: SubscriptionRequest): Promise<SubscriptionResponse> {
    const requestId = uuidv4();
    const request = {
      id: requestId,
      route: { channel: this.name, endpoint: 'subscribe' },
      payload: subscription,
    };

    const promise = new Promise<SubscriptionResponse>((resolve, reject) => {
      const timeout = setTimeout(() => {
        this.pendingRequests.delete(requestId);
        reject(new Error('Subscribe timeout'));
      }, 30000);

      this.pendingRequests.set(requestId, { resolve, reject, timeout });
    });

    await this.connection.sendAsync(JSON.stringify(request));
    return promise;
  }

  async handleReceivedMessage(message: any): Promise<void> {
    // Check if it's a response to pending request
    if (message.id && this.pendingRequests.has(message.id)) {
      const pending = this.pendingRequests.get(message.id)!;
      clearTimeout(pending.timeout);
      this.pendingRequests.delete(message.id);
      pending.resolve(message.payload);
      return;
    }

    // Check if it's metadata
    if (message.metadata) {
      this.metadata = message.metadata as ChannelMetadata;
      this.emitter.emit('metadataReceived', this.metadata);
      return;
    }

    // Regular message
    const feederMessage: FeederMessage = message;
    this.emitter.emit('messageReceived', feederMessage);
  }

  protected setState(newState: ChannelState): void {
    const oldState = this.state;
    this.state = newState;
    this.emitter.emit('stateChanged', oldState, newState);
  }

  protected async waitForOpen(timeout: number): Promise<void> {
    return new Promise((resolve, reject) => {
      const timer = setTimeout(() => {
        reject(new Error('Channel open timeout'));
      }, timeout);

      const check = setInterval(() => {
        if (this.metadata) {
          clearInterval(check);
          clearTimeout(timer);
          resolve();
        }
      }, 100);
    });
  }

  // Event registration
  on(event: 'stateChanged', listener: (old: ChannelState, new: ChannelState) => void): this;
  on(event: 'messageReceived', listener: (message: FeederMessage) => void): this;
  on(event: 'metadataReceived', listener: (metadata: ChannelMetadata) => void): this;
  on(event: string, listener: (...args: any[]) => void): this {
    this.emitter.on(event, listener);
    return this;
  }
}
```

### Client Layer

#### ThunderPropagator Client (`client.ts`)

```typescript
import { AbstractConnection } from './connections/AbstractConnection';
import { WebSocketConnection } from './connections/WebSocketConnection';
import { AbstractChannel } from './channels/AbstractChannel';
import { ProtocolType } from './models/Enums';
import { WebSocketConfiguration } from './models/Configuration';

export class ThunderPropagatorClient {
  protected connection: AbstractConnection<any>;
  protected channels = new Map<string, AbstractChannel>();

  constructor(connection: AbstractConnection<any>) {
    this.connection = connection;
    this.connection.on('messageReceived', (message) => this.routeMessage(message));
  }

  async connectAsync(): Promise<void> {
    await this.connection.connectAsync();
  }

  async disconnectAsync(): Promise<void> {
    // Close all channels
    for (const channel of this.channels.values()) {
      await channel.closeAsync();
    }
    await this.connection.disconnectAsync();
  }

  async createChannelAsync(name: string): Promise<AbstractChannel> {
    if (this.channels.has(name)) {
      return this.channels.get(name)!;
    }

    const channel = new AbstractChannel(name, this.connection);
    this.channels.set(name, channel);
    await channel.openAsync();
    return channel;
  }

  getChannel(name: string): AbstractChannel | undefined {
    return this.channels.get(name);
  }

  protected async routeMessage(message: string): Promise<void> {
    try {
      // Try JSON format
      const data = JSON.parse(message);
      if (data.route?.channel) {
        const channel = this.channels.get(data.route.channel);
        if (channel) {
          await channel.handleReceivedMessage(data);
        }
        return;
      }
    } catch {
      // Not JSON, try CSV
      const parts = message.split(',', 2);
      if (parts.length >= 2) {
        const channelName = parts[0];
        const channel = this.channels.get(channelName);
        if (channel) {
          await channel.handleReceivedMessage({ payload: parts[1] });
        }
      }
    }
  }
}

export class WebSocketClient extends ThunderPropagatorClient {
  constructor(config: WebSocketConfiguration) {
    const connection = new WebSocketConnection(config);
    super(connection);
  }
}
```

---

## Usage Example

```typescript
import { WebSocketClient } from '@thunderpropagator/client';

async function main() {
  const client = new WebSocketClient({
    uri: 'wss://example.com/thunder',
    headers: { 'Authorization': 'Bearer token123' },
    keepAliveInterval: 20000,
  });

  await client.connectAsync();

  const channel = await client.createChannelAsync('market-data');

  channel.on('messageReceived', (message) => {
    console.log('Received:', message);
  });

  const response = await channel.subscribeAsync({
    dataType: 'quotes',
    symbols: ['AAPL', 'GOOGL'],
  });

  console.log('Subscribed:', response.subscriptionId);

  // Keep running
  await new Promise(resolve => setTimeout(resolve, 60000));

  await client.disconnectAsync();
}

main().catch(console.error);
```

---

## Testing

```typescript
// tests/connections.test.ts
import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { WebSocketConnection } from '../src/connections/WebSocketConnection';

describe('WebSocketConnection', () => {
  let connection: WebSocketConnection;

  beforeEach(() => {
    connection = new WebSocketConnection({
      uri: 'wss://echo.websocket.org',
    });
  });

  afterEach(async () => {
    if (connection.isConnected) {
      await connection.disconnectAsync();
    }
  });

  it('should connect successfully', async () => {
    await connection.connectAsync();
    expect(connection.isConnected).toBe(true);
  });

  it('should send and receive messages', async () => {
    await connection.connectAsync();
    
    const received: string[] = [];
    connection.on('messageReceived', (msg) => received.push(msg));

    await connection.sendAsync('test');
    
    // Wait for echo
    await new Promise(resolve => setTimeout(resolve, 1000));
    expect(received).toContain('test');
  });
});
```

---

## Build & Distribution

```bash
# Install dependencies
npm install

# Run tests
npm test

# Type check
npm run type-check

# Build for distribution
npm run build

# Publish to npm
npm publish
```

**vite.config.ts**:
```typescript
import { defineConfig } from 'vite';
import { resolve } from 'path';

export default defineConfig({
  build: {
    lib: {
      entry: resolve(__dirname, 'src/index.ts'),
      name: 'ThunderPropagator',
      formats: ['es', 'cjs'],
      fileName: (format) => `index.${format === 'es' ? 'js' : 'cjs'}`,
    },
    rollupOptions: {
      external: ['ws', 'eventemitter3'],
    },
  },
  test: {
    globals: true,
    environment: 'node',
  },
});
```

---

## NPM Module Creation & Publishing Guide

### 1. Initialize npm Package

```bash
# Create project directory
mkdir thunderpropagator-js
cd thunderpropagator-js

# Initialize npm package
npm init -y

# Install development dependencies
npm install -D typescript @types/node vite vitest
npm install -D @types/ws eventemitter3 ws

# Initialize TypeScript
npx tsc --init
```

### 2. Configure package.json

```json
{
  "name": "@thunderpropagator/client",
  "version": "1.0.0",
  "description": "Real-time data streaming client for JavaScript/TypeScript",
  "author": "Your Name <your.email@example.com>",
  "license": "MIT",
  "homepage": "https://github.com/yourusername/thunderpropagator-js#readme",
  "repository": {
    "type": "git",
    "url": "git+https://github.com/yourusername/thunderpropagator-js.git"
  },
  "bugs": {
    "url": "https://github.com/yourusername/thunderpropagator-js/issues"
  },
  "keywords": [
    "websocket",
    "quic",
    "streaming",
    "real-time",
    "thunderpropagator",
    "client",
    "typescript"
  ],
  "type": "module",
  "main": "./dist/index.cjs",
  "module": "./dist/index.js",
  "types": "./dist/index.d.ts",
  "exports": {
    ".": {
      "import": {
        "types": "./dist/index.d.ts",
        "default": "./dist/index.js"
      },
      "require": {
        "types": "./dist/index.d.cts",
        "default": "./dist/index.cjs"
      }
    },
    "./package.json": "./package.json"
  },
  "files": [
    "dist",
    "src",
    "README.md",
    "LICENSE"
  ],
  "scripts": {
    "build": "vite build && npm run build:types",
    "build:types": "tsc --emitDeclarationOnly --outDir dist",
    "dev": "vite build --watch",
    "test": "vitest run",
    "test:watch": "vitest",
    "test:coverage": "vitest run --coverage",
    "lint": "eslint src/**/*.ts",
    "format": "prettier --write \"src/**/*.ts\"",
    "typecheck": "tsc --noEmit",
    "prepublishOnly": "npm run build && npm run test",
    "prepare": "npm run build"
  },
  "dependencies": {
    "eventemitter3": "^5.0.1",
    "ws": "^8.16.0"
  },
  "devDependencies": {
    "@types/node": "^20.10.0",
    "@types/ws": "^8.5.0",
    "@typescript-eslint/eslint-plugin": "^6.15.0",
    "@typescript-eslint/parser": "^6.15.0",
    "eslint": "^8.56.0",
    "prettier": "^3.1.1",
    "typescript": "^5.3.3",
    "vite": "^5.0.10",
    "vitest": "^1.1.0",
    "@vitest/coverage-v8": "^1.1.0"
  },
  "peerDependencies": {
    "@fails-components/webtransport": "^0.2.0"
  },
  "peerDependenciesMeta": {
    "@fails-components/webtransport": {
      "optional": true
    }
  },
  "engines": {
    "node": ">=18.0.0"
  }
}
```

### 3. Configure TypeScript Compilation

**tsconfig.json**:
```json
{
  "compilerOptions": {
    "target": "ES2020",
    "module": "ESNext",
    "lib": ["ES2020", "DOM"],
    "moduleResolution": "bundler",
    "resolveJsonModule": true,
    "allowSyntheticDefaultImports": true,
    "esModuleInterop": true,
    "forceConsistentCasingInFileNames": true,
    "strict": true,
    "skipLibCheck": true,
    "declaration": true,
    "declarationMap": true,
    "sourceMap": true,
    "outDir": "dist",
    "rootDir": "src",
    "incremental": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noImplicitReturns": true,
    "noFallthroughCasesInSwitch": true
  },
  "include": ["src/**/*"],
  "exclude": ["node_modules", "dist", "tests", "**/*.test.ts"]
}
```

### 4. Configure Vite for Library Build

**vite.config.ts**:
```typescript
import { defineConfig } from 'vite';
import { resolve } from 'path';
import dts from 'vite-plugin-dts';

export default defineConfig({
  plugins: [
    dts({
      insertTypesEntry: true,
      rollupTypes: true,
    }),
  ],
  build: {
    lib: {
      entry: resolve(__dirname, 'src/index.ts'),
      name: 'ThunderPropagator',
      formats: ['es', 'cjs'],
      fileName: (format) => {
        if (format === 'es') return 'index.js';
        if (format === 'cjs') return 'index.cjs';
        return `index.${format}.js`;
      },
    },
    rollupOptions: {
      // Externalize dependencies that shouldn't be bundled
      external: ['ws', 'eventemitter3', /^node:.*/],
      output: {
        // Preserve module structure for better tree-shaking
        preserveModules: false,
        exports: 'named',
        globals: {
          ws: 'WebSocket',
          eventemitter3: 'EventEmitter3',
        },
      },
    },
    sourcemap: true,
    minify: false, // Keep code readable, minification at consumer's choice
  },
  test: {
    globals: true,
    environment: 'node',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: [
        'node_modules/',
        'dist/',
        'tests/',
        '**/*.test.ts',
        '**/*.spec.ts',
      ],
    },
  },
});
```

### 5. Create Essential Files

**.npmignore**:
```
# Source files (already in dist)
src/
tests/
examples/

# Config files
tsconfig.json
vite.config.ts
.eslintrc.json
.prettierrc

# Development
node_modules/
coverage/
.vscode/
.idea/

# Git
.git/
.gitignore
.github/

# Testing
*.test.ts
*.spec.ts
__tests__/

# Documentation source
docs/
```

**.gitignore**:
```
# Dependencies
node_modules/
package-lock.json
yarn.lock
pnpm-lock.yaml

# Build output
dist/
*.tsbuildinfo

# Testing
coverage/
.nyc_output/

# IDE
.vscode/
.idea/
*.swp
*.swo
*~

# OS
.DS_Store
Thumbs.db

# Environment
.env
.env.local
.env.*.local

# Logs
logs/
*.log
npm-debug.log*
```

**LICENSE** (MIT example):
```
MIT License

Copyright (c) 2025 Your Name

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

**README.md**:
```markdown
# @thunderpropagator/client

Real-time data streaming client for JavaScript/TypeScript supporting WebSocket, QUIC (WebTransport), and custom protocols.

## Installation

```bash
npm install @thunderpropagator/client
```

## Quick Start

```typescript
import { WebSocketClient } from '@thunderpropagator/client';

const client = new WebSocketClient({
  uri: 'wss://example.com/thunder',
});

await client.connectAsync();
const channel = await client.createChannelAsync('my-channel');

channel.on('messageReceived', (message) => {
  console.log('Received:', message);
});
```

## Features

- 🚀 Multiple protocol support (WebSocket, QUIC, custom)
- 📦 TypeScript support with full type definitions
- 🔒 Built-in encryption support
- 🔄 Auto-reconnection
- 📡 Channel-based communication
- 🎯 Request/response pattern
- 🌐 Browser and Node.js compatible

## Documentation

See [full documentation](https://github.com/yourusername/thunderpropagator-js#readme)

## License

MIT
```

### 6. Build and Test Before Publishing

```bash
# Type check
npm run typecheck

# Run tests
npm test

# Build the package
npm run build

# Verify build output
ls -la dist/

# Test the package locally
npm pack
npm install -g ./thunderpropagator-client-1.0.0.tgz
```

### 7. Publishing to npm

#### First-time Setup

```bash
# Login to npm (if not already logged in)
npm login

# Verify you're logged in
npm whoami
```

#### Option A: Public Package

```bash
# Publish publicly
npm publish --access public

# For scoped packages (@org/package)
npm publish --access public
```

#### Option B: Private Package (requires paid npm account)

```bash
npm publish --access restricted
```

#### Publishing with Tags

```bash
# Publish as beta
npm publish --tag beta

# Publish as next
npm publish --tag next

# Users install with: npm install @thunderpropagator/client@beta
```

### 8. Version Management

```bash
# Patch version (1.0.0 -> 1.0.1)
npm version patch

# Minor version (1.0.0 -> 1.1.0)
npm version minor

# Major version (1.0.0 -> 2.0.0)
npm version major

# Pre-release version (1.0.0 -> 1.0.1-beta.0)
npm version prerelease --preid=beta

# Then publish
npm publish
```

### 9. Automated Publishing with GitHub Actions

**.github/workflows/publish.yml**:
```yaml
name: Publish to npm

on:
  release:
    types: [published]

jobs:
  publish:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      id-token: write
    steps:
      - uses: actions/checkout@v4
      
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
          registry-url: 'https://registry.npmjs.org'
      
      - name: Install dependencies
        run: npm ci
      
      - name: Run tests
        run: npm test
      
      - name: Build
        run: npm run build
      
      - name: Publish to npm
        run: npm publish --provenance --access public
        env:
          NODE_AUTH_TOKEN: ${{ secrets.NPM_TOKEN }}
```

### 10. Package Testing Strategy

**Test local package before publishing**:

```bash
# 1. Build the package
npm run build

# 2. Create a tarball
npm pack

# 3. Test in another project
cd /tmp/test-project
npm init -y
npm install /path/to/thunderpropagator-client-1.0.0.tgz

# 4. Test import
node -e "const client = require('@thunderpropagator/client'); console.log(client);"
```

### 11. npm Scripts Breakdown

```json
{
  "scripts": {
    // Development
    "dev": "vite build --watch",
    "start": "npm run dev",
    
    // Building
    "build": "vite build && npm run build:types",
    "build:types": "tsc --emitDeclarationOnly --outDir dist",
    "clean": "rm -rf dist coverage",
    
    // Testing
    "test": "vitest run",
    "test:watch": "vitest",
    "test:coverage": "vitest run --coverage",
    "test:ui": "vitest --ui",
    
    // Code Quality
    "lint": "eslint src/**/*.ts",
    "lint:fix": "eslint src/**/*.ts --fix",
    "format": "prettier --write \"src/**/*.ts\"",
    "format:check": "prettier --check \"src/**/*.ts\"",
    "typecheck": "tsc --noEmit",
    
    // Pre-publish checks
    "prepublishOnly": "npm run clean && npm run build && npm run test",
    "prepack": "npm run build",
    
    // Version management
    "version": "npm run format && git add -A src",
    "postversion": "git push && git push --tags",
    
    // Release
    "release:patch": "npm version patch && npm publish",
    "release:minor": "npm version minor && npm publish",
    "release:major": "npm version major && npm publish"
  }
}
```

### 12. Package Size Optimization

```bash
# Check package size
npm pack --dry-run

# Analyze bundle size
npx vite-bundle-visualizer

# Install size checker
npx package-size @thunderpropagator/client

# Keep package under 100KB (compressed)
```

### 13. Documentation Best Practices

- Include TypeDoc comments for all public APIs
- Provide comprehensive README with examples
- Create CHANGELOG.md for version history
- Add CONTRIBUTING.md for contributors
- Include API documentation in `/docs`

### 14. npm Package Badges

Add to README.md:

```markdown
[![npm version](https://badge.fury.io/js/@thunderpropagator%2Fclient.svg)](https://www.npmjs.com/package/@thunderpropagator/client)
[![npm downloads](https://img.shields.io/npm/dm/@thunderpropagator/client.svg)](https://www.npmjs.com/package/@thunderpropagator/client)
[![license](https://img.shields.io/npm/l/@thunderpropagator/client.svg)](https://github.com/yourusername/thunderpropagator-js/blob/main/LICENSE)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.0+-blue.svg)](https://www.typescriptlang.org/)
```

### 15. Post-Publishing Checklist

- [ ] Verify package appears on npmjs.com
- [ ] Test installation: `npm install @thunderpropagator/client`
- [ ] Check bundle size on bundlephobia.com
- [ ] Update GitHub release notes
- [ ] Announce on social media/forums
- [ ] Update documentation site
- [ ] Monitor npm download stats
- [ ] Watch for issues/bug reports

---

## Key JavaScript/TypeScript Considerations

1. **Dual Package**: Support both ESM and CommonJS
2. **Browser + Node**: Use conditional exports/imports
3. **WebTransport**: QUIC support in modern browsers
4. **AbortController**: For cancellation patterns
5. **Promises**: All async operations return promises
6. **EventEmitter**: Use `eventemitter3` for events
7. **Type Safety**: Leverage TypeScript strict mode
8. **Tree Shaking**: Ensure side-effect free modules

---

## Implementation Checklist

- [ ] Set up TypeScript project with Vite
- [ ] Configure package.json with proper exports
- [ ] Implement `AbstractConnection` base class
- [ ] Implement `WebSocketConnection` (Node.js + Browser)
- [ ] Implement `QuicConnection` using WebTransport
- [ ] Implement `InfiniteDataStreamConnection`
- [ ] Implement `AbstractChannel`
- [ ] Implement `ThunderPropagatorClient`
- [ ] Create protocol-specific clients
- [ ] Define all TypeScript interfaces
- [ ] Implement encryption utilities (Web Crypto API)
- [ ] Write unit tests (Vitest)
- [ ] Write integration tests
- [ ] Add JSDoc/TypeDoc comments
- [ ] Create usage examples
- [ ] Set up dual package (ESM/CJS)
- [ ] Configure CI/CD for automated publishing
- [ ] Create comprehensive README
- [ ] Add LICENSE file
- [ ] Publish to npm registry
- [ ] Monitor package analytics
