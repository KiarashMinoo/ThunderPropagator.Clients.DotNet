# ThunderPropagator Python Client - Implementation Prompt

## Project Goal
Create a Python client library for ThunderPropagator real-time data streaming framework supporting WebSocket, QUIC, and InfiniteDataStream protocols.

## Target Environment
- **Python Version**: 3.10+ (for modern type hints and pattern matching)
- **Async Framework**: asyncio
- **Package Distribution**: PyPI
- **Testing**: pytest with pytest-asyncio

---

## Project Structure

```
thunderpropagator-python/
├── pyproject.toml
├── README.md
├── LICENSE
├── .gitignore
├── src/
│   └── thunderpropagator/
│       ├── __init__.py
│       ├── client.py
│       ├── connections/
│       │   ├── __init__.py
│       │   ├── base.py
│       │   ├── websocket.py
│       │   ├── quic.py
│       │   └── infinite_data_stream.py
│       ├── channels/
│       │   ├── __init__.py
│       │   ├── base.py
│       │   └── channel.py
│       ├── models/
│       │   ├── __init__.py
│       │   ├── configuration.py
│       │   ├── enums.py
│       │   ├── messages.py
│       │   ├── metadata.py
│       │   └── subscriptions.py
│       ├── crypto/
│       │   ├── __init__.py
│       │   └── encryption.py
│       └── logging/
│           ├── __init__.py
│           └── logger.py
├── tests/
│   ├── __init__.py
│   ├── test_connections.py
│   ├── test_channels.py
│   └── test_integration.py
└── examples/
    ├── websocket_example.py
    ├── quic_example.py
    └── infinite_stream_example.py
```

---

## Core Dependencies

```toml
[project]
dependencies = [
    "websockets>=12.0",           # WebSocket support
    "aioquic>=0.9.0",            # QUIC protocol
    "aiohttp>=3.9.0",            # HTTP client
    "cryptography>=41.0",        # Encryption
    "pydantic>=2.5",             # Data validation
    "typing-extensions>=4.8",    # Backports for type hints
]

[project.optional-dependencies]
dev = [
    "pytest>=7.4",
    "pytest-asyncio>=0.21",
    "pytest-cov>=4.1",
    "black>=23.0",
    "mypy>=1.7",
    "ruff>=0.1",
]
```

---

## Design Patterns & Idioms

### 1. Abstract Base Classes
Use `abc.ABC` and `@abstractmethod` for base classes:

```python
from abc import ABC, abstractmethod
from typing import Generic, TypeVar

TConfig = TypeVar('TConfig', bound='BaseConfiguration')

class AbstractConnection(ABC, Generic[TConfig]):
    def __init__(self, config: TConfig):
        self.config = config
        self._state = ConnectionState.READY
        self._connection_id: str | None = None
    
    @abstractmethod
    async def connect_async(self) -> None:
        """Establish connection to server."""
        pass
    
    @abstractmethod
    async def disconnect_async(self) -> None:
        """Close connection gracefully."""
        pass
    
    @abstractmethod
    async def send_async(self, message: str) -> None:
        """Send raw message."""
        pass
```

### 2. Dataclasses with Pydantic
Use Pydantic for validated models:

```python
from pydantic import BaseModel, Field, field_validator

class WebSocketConfiguration(BaseModel):
    uri: str = Field(..., description="WebSocket URI")
    headers: dict[str, str] = Field(default_factory=dict)
    connection_timeout: float = Field(default=30.0, gt=0)
    keep_alive_interval: float = Field(default=20.0, gt=0)
    
    @field_validator('uri')
    @classmethod
    def validate_uri(cls, v: str) -> str:
        if not v.startswith(('ws://', 'wss://')):
            raise ValueError("URI must start with ws:// or wss://")
        return v
```

### 3. Async Context Managers
Implement `__aenter__` and `__aexit__`:

```python
class ThunderPropagatorClient:
    async def __aenter__(self):
        await self.connect_async()
        return self
    
    async def __aexit__(self, exc_type, exc_val, exc_tb):
        await self.disconnect_async()
        return False
```

### 4. Observable Pattern with AsyncIO Events
Use `asyncio.Event` and callbacks:

```python
from typing import Callable, Awaitable
from collections.abc import Coroutine

MessageCallback = Callable[[str], Awaitable[None]]

class AbstractConnection:
    def __init__(self):
        self._message_callbacks: list[MessageCallback] = []
        self._state_changed_callbacks: list[Callable] = []
    
    def on_message_received(self, callback: MessageCallback):
        """Register callback for received messages."""
        self._message_callbacks.append(callback)
    
    async def _notify_message_received(self, message: str):
        """Notify all registered callbacks."""
        await asyncio.gather(*[cb(message) for cb in self._message_callbacks])
```

### 5. Enums
Use Python enums with auto():

```python
from enum import Enum, auto

class ConnectionState(Enum):
    READY = auto()
    CONNECTING = auto()
    OPEN = auto()
    CLOSING = auto()
    CLOSED = auto()
    HAS_ERROR = auto()

class ProtocolType(Enum):
    WEBSOCKET = "websocket"
    QUIC = "quic"
    INFINITE_DATA_STREAM = "infinite_data_stream"
```

---

## Implementation Details

### Connection Layer

#### Base Connection (`connections/base.py`)

```python
from abc import ABC, abstractmethod
from typing import Generic, TypeVar
import asyncio
import logging
from datetime import datetime

from ..models.enums import ConnectionState
from ..models.messages import ConnectionResponse

TConfig = TypeVar('TConfig')

class AbstractConnection(ABC, Generic[TConfig]):
    """Base class for all protocol connections."""
    
    def __init__(self, config: TConfig, logger: logging.Logger):
        self.config = config
        self.logger = logger
        self._state = ConnectionState.READY
        self._connection_id: str | None = None
        self._connection_info: ConnectionResponse | None = None
        
        # Event callbacks
        self._state_changed_callbacks = []
        self._message_received_callbacks = []
        self._error_occurred_callbacks = []
        
        # Internal
        self._receive_task: asyncio.Task | None = None
        self._disconnect_event = asyncio.Event()
    
    @property
    def connection_id(self) -> str | None:
        return self._connection_id
    
    @property
    def state(self) -> ConnectionState:
        return self._state
    
    @property
    def is_connected(self) -> bool:
        return self._state == ConnectionState.OPEN
    
    @abstractmethod
    async def _connect_impl(self) -> None:
        """Protocol-specific connection implementation."""
        pass
    
    @abstractmethod
    async def _disconnect_impl(self) -> None:
        """Protocol-specific disconnection implementation."""
        pass
    
    @abstractmethod
    async def _send_impl(self, message: str) -> None:
        """Protocol-specific send implementation."""
        pass
    
    @abstractmethod
    async def _receive_loop(self) -> None:
        """Protocol-specific receive loop."""
        pass
    
    async def connect_async(self) -> None:
        """Establish connection."""
        if self._state != ConnectionState.READY:
            raise RuntimeError(f"Cannot connect from state {self._state}")
        
        self._set_state(ConnectionState.CONNECTING)
        try:
            await self._connect_impl()
            self._receive_task = asyncio.create_task(self._receive_loop())
            self._set_state(ConnectionState.OPEN)
        except Exception as e:
            self._set_state(ConnectionState.HAS_ERROR)
            await self._notify_error(e)
            raise
    
    async def disconnect_async(self) -> None:
        """Disconnect gracefully."""
        if self._state in (ConnectionState.CLOSED, ConnectionState.CLOSING):
            return
        
        self._set_state(ConnectionState.CLOSING)
        
        # Cancel receive task
        if self._receive_task:
            self._receive_task.cancel()
            try:
                await self._receive_task
            except asyncio.CancelledError:
                pass
        
        await self._disconnect_impl()
        self._set_state(ConnectionState.CLOSED)
    
    async def send_async(self, message: str) -> None:
        """Send message."""
        if not self.is_connected:
            raise RuntimeError("Connection is not open")
        await self._send_impl(message)
    
    def _set_state(self, new_state: ConnectionState):
        """Update state and notify listeners."""
        old_state = self._state
        self._state = new_state
        asyncio.create_task(self._notify_state_changed(old_state, new_state))
    
    async def _notify_state_changed(self, old: ConnectionState, new: ConnectionState):
        await asyncio.gather(*[cb(old, new) for cb in self._state_changed_callbacks])
    
    async def _notify_message_received(self, message: str):
        # Filter PROBE messages
        if message.strip() == "PROBE":
            return
        await asyncio.gather(*[cb(message) for cb in self._message_received_callbacks])
    
    async def _notify_error(self, error: Exception):
        await asyncio.gather(*[cb(error) for cb in self._error_occurred_callbacks])
    
    # Event registration
    def on_state_changed(self, callback):
        self._state_changed_callbacks.append(callback)
    
    def on_message_received(self, callback):
        self._message_received_callbacks.append(callback)
    
    def on_error_occurred(self, callback):
        self._error_occurred_callbacks.append(callback)
```

#### WebSocket Connection (`connections/websocket.py`)

```python
import websockets
from websockets.client import WebSocketClientProtocol

from .base import AbstractConnection
from ..models.configuration import WebSocketConfiguration

class WebSocketConnection(AbstractConnection[WebSocketConfiguration]):
    """WebSocket protocol connection."""
    
    def __init__(self, config: WebSocketConfiguration, logger):
        super().__init__(config, logger)
        self._websocket: WebSocketClientProtocol | None = None
    
    async def _connect_impl(self) -> None:
        extra_headers = self.config.headers or {}
        self._websocket = await websockets.connect(
            self.config.uri,
            extra_headers=extra_headers,
            open_timeout=self.config.connection_timeout,
            ping_interval=self.config.keep_alive_interval,
        )
        self.logger.info(f"WebSocket connected to {self.config.uri}")
    
    async def _disconnect_impl(self) -> None:
        if self._websocket:
            await self._websocket.close()
            self._websocket = None
    
    async def _send_impl(self, message: str) -> None:
        if not self._websocket:
            raise RuntimeError("WebSocket not connected")
        await self._websocket.send(message)
    
    async def _receive_loop(self) -> None:
        """Receive messages continuously."""
        try:
            async for message in self._websocket:
                await self._notify_message_received(message)
        except asyncio.CancelledError:
            pass
        except Exception as e:
            self.logger.error(f"Receive loop error: {e}")
            await self._notify_error(e)
```

### Channel Layer

#### Base Channel (`channels/base.py`)

```python
from abc import ABC
from typing import TYPE_CHECKING
import asyncio
import re
from datetime import datetime, timedelta

from ..models.enums import ChannelState
from ..models.metadata import ChannelMetadata
from ..models.messages import FeederMessage
from ..models.subscriptions import SubscriptionRequest, SubscriptionResponse

if TYPE_CHECKING:
    from ..connections.base import AbstractConnection

class AbstractChannel:
    """Base channel implementation."""
    
    def __init__(self, name: str, connection: 'AbstractConnection'):
        self.name = name
        self.connection = connection
        self._state = ChannelState.READY
        self._channel_id: str | None = None
        self._metadata: ChannelMetadata | None = None
        
        # Callbacks
        self._state_changed_callbacks = []
        self._message_received_callbacks = []
        self._metadata_received_callbacks = []
        
        # Pending requests
        self._pending_requests: dict[str, asyncio.Future] = {}
    
    @property
    def state(self) -> ChannelState:
        return self._state
    
    @property
    def is_open(self) -> bool:
        return self._state == ChannelState.OPEN
    
    async def open_async(self) -> None:
        """Open the channel."""
        if self._state != ChannelState.READY:
            raise RuntimeError(f"Cannot open channel from state {self._state}")
        
        self._set_state(ChannelState.OPENING)
        
        # Send channel open request
        open_request = {
            "route": {"channel": self.name, "endpoint": "open"},
            "timestamp": datetime.utcnow().isoformat(),
        }
        await self.connection.send_async(json.dumps(open_request))
        
        # Wait for metadata (with timeout)
        try:
            await asyncio.wait_for(self._wait_for_open(), timeout=30.0)
            self._set_state(ChannelState.OPEN)
        except asyncio.TimeoutError:
            self._set_state(ChannelState.CLOSED)
            raise RuntimeError("Channel open timeout")
    
    async def close_async(self) -> None:
        """Close the channel."""
        if self._state == ChannelState.CLOSED:
            return
        
        self._set_state(ChannelState.CLOSING)
        
        close_request = {
            "route": {"channel": self.name, "endpoint": "close"},
        }
        await self.connection.send_async(json.dumps(close_request))
        self._set_state(ChannelState.CLOSED)
    
    async def subscribe_async(self, subscription: SubscriptionRequest) -> SubscriptionResponse:
        """Subscribe to data stream."""
        request_id = str(uuid.uuid4())
        request = {
            "id": request_id,
            "route": {"channel": self.name, "endpoint": "subscribe"},
            "payload": subscription.dict(),
        }
        
        future = asyncio.Future()
        self._pending_requests[request_id] = future
        
        await self.connection.send_async(json.dumps(request))
        
        try:
            response = await asyncio.wait_for(future, timeout=30.0)
            return SubscriptionResponse(**response)
        except asyncio.TimeoutError:
            del self._pending_requests[request_id]
            raise RuntimeError("Subscribe timeout")
    
    async def handle_received_message(self, message: dict):
        """Handle message routed to this channel."""
        # Check if it's a response to pending request
        if "id" in message and message["id"] in self._pending_requests:
            future = self._pending_requests.pop(message["id"])
            future.set_result(message.get("payload", {}))
            return
        
        # Check if it's metadata
        if "metadata" in message:
            self._metadata = ChannelMetadata(**message["metadata"])
            await self._notify_metadata_received(self._metadata)
            return
        
        # Regular message
        feeder_message = FeederMessage(**message)
        await self._notify_message_received(feeder_message)
    
    def _set_state(self, new_state: ChannelState):
        old_state = self._state
        self._state = new_state
        asyncio.create_task(self._notify_state_changed(old_state, new_state))
    
    async def _wait_for_open(self):
        """Wait for channel to be opened (metadata received)."""
        while not self._metadata:
            await asyncio.sleep(0.1)
    
    async def _notify_state_changed(self, old, new):
        await asyncio.gather(*[cb(old, new) for cb in self._state_changed_callbacks])
    
    async def _notify_message_received(self, message):
        await asyncio.gather(*[cb(message) for cb in self._message_received_callbacks])
    
    async def _notify_metadata_received(self, metadata):
        await asyncio.gather(*[cb(metadata) for cb in self._metadata_received_callbacks])
    
    def on_state_changed(self, callback):
        self._state_changed_callbacks.append(callback)
    
    def on_message_received(self, callback):
        self._message_received_callbacks.append(callback)
    
    def on_metadata_received(self, callback):
        self._metadata_received_callbacks.append(callback)
```

### Client Layer

#### ThunderPropagator Client (`client.py`)

```python
import re
import json
from typing import Dict

from .connections.base import AbstractConnection
from .connections.websocket import WebSocketConnection
from .channels.base import AbstractChannel
from .models.enums import ProtocolType

class ThunderPropagatorClient:
    """Protocol-agnostic ThunderPropagator client."""
    
    def __init__(self, connection: AbstractConnection):
        self.connection = connection
        self._channels: Dict[str, AbstractChannel] = {}
        
        # Register for connection messages
        self.connection.on_message_received(self._route_message)
    
    async def connect_async(self) -> None:
        """Connect to server."""
        await self.connection.connect_async()
    
    async def disconnect_async(self) -> None:
        """Disconnect from server."""
        # Close all channels first
        for channel in list(self._channels.values()):
            await channel.close_async()
        
        await self.connection.disconnect_async()
    
    async def create_channel_async(self, name: str) -> AbstractChannel:
        """Create and open a channel."""
        if name in self._channels:
            return self._channels[name]
        
        channel = AbstractChannel(name, self.connection)
        self._channels[name] = channel
        await channel.open_async()
        return channel
    
    def get_channel(self, name: str) -> AbstractChannel | None:
        """Get existing channel."""
        return self._channels.get(name)
    
    async def _route_message(self, message: str):
        """Route received message to appropriate channel."""
        try:
            # Try JSON format first
            data = json.loads(message)
            if "route" in data and "channel" in data["route"]:
                channel_name = data["route"]["channel"]
                if channel_name in self._channels:
                    await self._channels[channel_name].handle_received_message(data)
                return
        except json.JSONDecodeError:
            pass
        
        # Try CSV format
        parts = message.split(',', 1)
        if len(parts) >= 2:
            channel_name = parts[0]
            if channel_name in self._channels:
                # Parse as simple message
                await self._channels[channel_name].handle_received_message({
                    "payload": parts[1]
                })
    
    async def __aenter__(self):
        await self.connect_async()
        return self
    
    async def __aexit__(self, exc_type, exc_val, exc_tb):
        await self.disconnect_async()
        return False


class WebSocketClient(ThunderPropagatorClient):
    """WebSocket-specific client."""
    
    def __init__(self, config, logger):
        connection = WebSocketConnection(config, logger)
        super().__init__(connection)
```

---

## Models

### Configuration (`models/configuration.py`)

```python
from pydantic import BaseModel, Field

class BaseConfiguration(BaseModel):
    """Base configuration for all connections."""
    connection_timeout: float = Field(default=30.0, gt=0)
    reconnect_enabled: bool = True
    reconnect_max_attempts: int = 5

class WebSocketConfiguration(BaseConfiguration):
    uri: str
    headers: dict[str, str] = Field(default_factory=dict)
    keep_alive_interval: float = 20.0

class QuicConfiguration(BaseConfiguration):
    host: str
    port: int
    max_bidirectional_streams: int = 100
    max_unidirectional_streams: int = 100

class InfiniteDataStreamConfiguration(BaseConfiguration):
    host: str
    port: int
    compression_enabled: bool = False
    chunk_size: int = 8192
```

### Messages (`models/messages.py`)

```python
from pydantic import BaseModel
from datetime import datetime
from typing import Any

class FeederMessage(BaseModel):
    id: str | None = None
    route: dict[str, str] | None = None
    payload: Any = None
    timestamp: datetime | None = None

class ConnectionResponse(BaseModel):
    connection_id: str
    server_version: str
    capabilities: list[str] = []
```

---

## Usage Example

```python
import asyncio
import logging
from thunderpropagator import WebSocketClient
from thunderpropagator.models import WebSocketConfiguration

async def main():
    # Configure
    config = WebSocketConfiguration(
        uri="wss://example.com/thunder",
        headers={"Authorization": "Bearer token123"}
    )
    
    logger = logging.getLogger("thunderpropagator")
    
    # Create client
    async with WebSocketClient(config, logger) as client:
        # Create channel
        channel = await client.create_channel_async("market-data")
        
        # Subscribe to messages
        async def on_message(msg):
            print(f"Received: {msg}")
        
        channel.on_message_received(on_message)
        
        # Subscribe to data
        from thunderpropagator.models import SubscriptionRequest
        sub = SubscriptionRequest(
            data_type="quotes",
            symbols=["AAPL", "GOOGL"]
        )
        response = await channel.subscribe_async(sub)
        print(f"Subscribed: {response.subscription_id}")
        
        # Keep running
        await asyncio.sleep(60)

if __name__ == "__main__":
    asyncio.run(main())
```

---

## Testing

```python
# tests/test_connections.py
import pytest
from thunderpropagator.connections import WebSocketConnection
from thunderpropagator.models import WebSocketConfiguration

@pytest.mark.asyncio
async def test_websocket_connect():
    config = WebSocketConfiguration(uri="wss://echo.websocket.org")
    conn = WebSocketConnection(config, logging.getLogger())
    
    await conn.connect_async()
    assert conn.is_connected
    
    await conn.disconnect_async()
    assert not conn.is_connected
```

---

## Build & Distribution

```bash
# Install dependencies
pip install -e ".[dev]"

# Run tests
pytest tests/ -v --cov=thunderpropagator

# Type checking
mypy src/thunderpropagator

# Linting
ruff check src/ tests/
black src/ tests/

# Build package
python -m build

# Publish to PyPI
twine upload dist/*
```

---

## PyPI Package Creation & Publishing Guide

### 1. Initialize Python Package

```bash
# Create project directory
mkdir thunderpropagator-python
cd thunderpropagator-python

# Create virtual environment
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate

# Install build tools
pip install --upgrade pip setuptools wheel build twine

# Create project structure
mkdir -p src/thunderpropagator tests examples
```

### 2. Configure pyproject.toml

```toml
[build-system]
requires = ["setuptools>=68.0", "wheel"]
build-backend = "setuptools.build_meta"

[project]
name = "thunderpropagator-client"
version = "1.0.0"
description = "Real-time data streaming client for Python"
readme = "README.md"
requires-python = ">=3.10"
license = {text = "MIT"}
authors = [
    {name = "Your Name", email = "your.email@example.com"}
]
maintainers = [
    {name = "Your Name", email = "your.email@example.com"}
]
keywords = ["websocket", "quic", "streaming", "real-time", "thunderpropagator", "client"]
classifiers = [
    "Development Status :: 4 - Beta",
    "Intended Audience :: Developers",
    "License :: OSI Approved :: MIT License",
    "Programming Language :: Python :: 3",
    "Programming Language :: Python :: 3.10",
    "Programming Language :: Python :: 3.11",
    "Programming Language :: Python :: 3.12",
    "Programming Language :: Python :: 3.13",
    "Topic :: Software Development :: Libraries :: Python Modules",
    "Topic :: System :: Networking",
    "Framework :: AsyncIO",
    "Typing :: Typed",
]

dependencies = [
    "websockets>=12.0",
    "aioquic>=0.9.0",
    "aiohttp>=3.9.0",
    "cryptography>=41.0",
    "pydantic>=2.5",
    "typing-extensions>=4.8",
]

[project.optional-dependencies]
dev = [
    "pytest>=7.4",
    "pytest-asyncio>=0.21",
    "pytest-cov>=4.1",
    "pytest-timeout>=2.2",
    "black>=23.0",
    "mypy>=1.7",
    "ruff>=0.1",
    "isort>=5.12",
]
docs = [
    "sphinx>=7.0",
    "sphinx-rtd-theme>=1.3",
    "sphinx-autodoc-typehints>=1.24",
]

[project.urls]
Homepage = "https://github.com/yourusername/thunderpropagator-python"
Documentation = "https://thunderpropagator-python.readthedocs.io"
Repository = "https://github.com/yourusername/thunderpropagator-python"
"Bug Tracker" = "https://github.com/yourusername/thunderpropagator-python/issues"
Changelog = "https://github.com/yourusername/thunderpropagator-python/blob/main/CHANGELOG.md"

[tool.setuptools]
package-dir = {"" = "src"}

[tool.setuptools.packages.find]
where = ["src"]
include = ["thunderpropagator*"]
namespaces = false

[tool.setuptools.package-data]
thunderpropagator = ["py.typed"]

# Black configuration
[tool.black]
line-length = 100
target-version = ['py310']
include = '\.pyi?$'

# isort configuration
[tool.isort]
profile = "black"
line_length = 100

# mypy configuration
[tool.mypy]
python_version = "3.10"
strict = true
warn_return_any = true
warn_unused_configs = true
disallow_untyped_defs = true

# pytest configuration
[tool.pytest.ini_options]
asyncio_mode = "auto"
testpaths = ["tests"]
python_files = ["test_*.py"]
python_classes = ["Test*"]
python_functions = ["test_*"]
addopts = [
    "-ra",
    "--strict-markers",
    "--strict-config",
    "--showlocals",
    "--cov=thunderpropagator",
    "--cov-report=term-missing",
    "--cov-report=html",
]

# Coverage configuration
[tool.coverage.run]
source = ["src"]
omit = ["tests/*", "examples/*"]

[tool.coverage.report]
exclude_lines = [
    "pragma: no cover",
    "def __repr__",
    "raise AssertionError",
    "raise NotImplementedError",
    "if __name__ == .__main__.:",
    "if TYPE_CHECKING:",
    "@abstractmethod",
]

# Ruff configuration
[tool.ruff]
line-length = 100
target-version = "py310"
select = ["E", "F", "W", "I", "N", "UP", "ANN", "B", "A", "C4", "PT"]
ignore = ["ANN101", "ANN102"]

[tool.ruff.per-file-ignores]
"tests/*" = ["ANN"]
```

### 3. Create Essential Files

**src/thunderpropagator/py.typed**:
```
# Marker file for PEP 561 - indicates package supports type hints
```

**src/thunderpropagator/__init__.py**:
```python
"""ThunderPropagator Python Client Library."""

from .client import (
    ThunderPropagatorClient,
    WebSocketClient,
    QuicClient,
    InfiniteDataStreamClient,
)
from .connections import (
    AbstractConnection,
    WebSocketConnection,
    QuicConnection,
    InfiniteDataStreamConnection,
)
from .channels import AbstractChannel, Channel
from .models import (
    BaseConfiguration,
    WebSocketConfiguration,
    QuicConfiguration,
    InfiniteDataStreamConfiguration,
    ConnectionState,
    ChannelState,
    ProtocolType,
    FeederMessage,
    ConnectionResponse,
    ChannelMetadata,
)

__version__ = "1.0.0"
__all__ = [
    # Clients
    "ThunderPropagatorClient",
    "WebSocketClient",
    "QuicClient",
    "InfiniteDataStreamClient",
    # Connections
    "AbstractConnection",
    "WebSocketConnection",
    "QuicConnection",
    "InfiniteDataStreamConnection",
    # Channels
    "AbstractChannel",
    "Channel",
    # Configuration
    "BaseConfiguration",
    "WebSocketConfiguration",
    "QuicConfiguration",
    "InfiniteDataStreamConfiguration",
    # Enums
    "ConnectionState",
    "ChannelState",
    "ProtocolType",
    # Messages
    "FeederMessage",
    "ConnectionResponse",
    "ChannelMetadata",
]
```

**MANIFEST.in**:
```
include README.md
include LICENSE
include CHANGELOG.md
include pyproject.toml
recursive-include src/thunderpropagator *.py py.typed
recursive-include tests *.py
recursive-exclude * __pycache__
recursive-exclude * *.py[co]
```

**.gitignore**:
```
# Byte-compiled / optimized / DLL files
__pycache__/
*.py[cod]
*$py.class
*.so

# Distribution / packaging
.Python
build/
develop-eggs/
dist/
downloads/
eggs/
.eggs/
lib/
lib64/
parts/
sdist/
var/
wheels/
*.egg-info/
.installed.cfg
*.egg

# Virtual environments
venv/
env/
ENV/
.venv

# Testing
.pytest_cache/
.coverage
htmlcov/
.tox/
.hypothesis/

# Type checking
.mypy_cache/
.dmypy.json
dmypy.json

# IDE
.vscode/
.idea/
*.swp
*.swo

# OS
.DS_Store
Thumbs.db
```

**LICENSE** (MIT):
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
# ThunderPropagator Python Client

Real-time data streaming client for Python supporting WebSocket, QUIC, and custom protocols.

## Installation

```bash
pip install thunderpropagator-client
```

## Quick Start

```python
import asyncio
from thunderpropagator import WebSocketClient, WebSocketConfiguration

async def main():
    config = WebSocketConfiguration(uri="wss://example.com/thunder")
    
    async with WebSocketClient(config) as client:
        channel = await client.create_channel_async("my-channel")
        
        async def on_message(msg):
            print(f"Received: {msg}")
        
        channel.on_message_received(on_message)
        await asyncio.sleep(60)

asyncio.run(main())
```

## Features

- 🚀 Multiple protocol support (WebSocket, QUIC, custom)
- 📦 Full type hints for IDE support
- 🔒 Built-in encryption
- 🔄 Auto-reconnection
- 📡 Channel-based communication
- 🎯 Request/response pattern
- ⚡ Async/await throughout

## Documentation

See [full documentation](https://thunderpropagator-python.readthedocs.io)

## Requirements

- Python 3.10+

## License

MIT
```

**CHANGELOG.md**:
```markdown
# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-01-01

### Added
- Initial release
- WebSocket connection support
- QUIC connection support
- InfiniteDataStream connection support
- Channel-based communication
- Request/response pattern
- Encryption support
- Full type hints
```

### 4. Build and Test Locally

```bash
# Install in development mode
pip install -e ".[dev]"

# Run linting
ruff check src/ tests/
black --check src/ tests/
isort --check-only src/ tests/

# Run type checking
mypy src/

# Run tests
pytest tests/ -v --cov

# Build the package
python -m build

# Check the package
twine check dist/*

# Verify package contents
tar -tzf dist/thunderpropagator-client-1.0.0.tar.gz
```

### 5. Publishing to PyPI

#### First-time Setup

```bash
# Create PyPI account at https://pypi.org/account/register/

# Create API token at https://pypi.org/manage/account/token/

# Configure credentials
# Option 1: Using .pypirc
cat > ~/.pypirc << EOF
[pypi]
username = __token__
password = pypi-YOUR-API-TOKEN-HERE
EOF

# Option 2: Using environment variable
export TWINE_USERNAME=__token__
export TWINE_PASSWORD=pypi-YOUR-API-TOKEN-HERE
```

#### Publishing Steps

```bash
# Clean previous builds
rm -rf dist/ build/ *.egg-info

# Build distributions
python -m build

# Upload to TestPyPI first (recommended)
twine upload --repository testpypi dist/*

# Test installation from TestPyPI
pip install --index-url https://test.pypi.org/simple/ thunderpropagator-client

# If everything works, upload to PyPI
twine upload dist/*
```

### 6. Automated Publishing with GitHub Actions

**.github/workflows/publish.yml**:
```yaml
name: Publish to PyPI

on:
  release:
    types: [published]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Set up Python
        uses: actions/setup-python@v5
        with:
          python-version: '3.10'
      
      - name: Install dependencies
        run: |
          python -m pip install --upgrade pip
          pip install build twine
      
      - name: Build package
        run: python -m build
      
      - name: Check package
        run: twine check dist/*
      
      - name: Publish to PyPI
        env:
          TWINE_USERNAME: __token__
          TWINE_PASSWORD: ${{ secrets.PYPI_API_TOKEN }}
        run: twine upload dist/*
```

**.github/workflows/test.yml**:
```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
        python-version: ['3.10', '3.11', '3.12']
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Set up Python ${{ matrix.python-version }}
        uses: actions/setup-python@v5
        with:
          python-version: ${{ matrix.python-version }}
      
      - name: Install dependencies
        run: |
          python -m pip install --upgrade pip
          pip install -e ".[dev]"
      
      - name: Lint with ruff
        run: ruff check src/ tests/
      
      - name: Type check with mypy
        run: mypy src/
      
      - name: Test with pytest
        run: pytest tests/ -v --cov --cov-report=xml
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          file: ./coverage.xml
```

### 7. Version Management

```bash
# Update version in pyproject.toml
# Then tag and release

git tag v1.0.1
git push origin v1.0.1

# Semantic versioning
# Major: 1.0.0 -> 2.0.0 (breaking changes)
# Minor: 1.0.0 -> 1.1.0 (new features)
# Patch: 1.0.0 -> 1.0.1 (bug fixes)
```

### 8. Using Poetry (Alternative)

```bash
# Install Poetry
curl -sSL https://install.python-poetry.org | python3 -

# Initialize project
poetry init

# Add dependencies
poetry add websockets aioquic aiohttp cryptography pydantic

# Add dev dependencies
poetry add --group dev pytest pytest-asyncio pytest-cov black mypy ruff

# Build
poetry build

# Publish
poetry publish
```

**pyproject.toml for Poetry**:
```toml
[tool.poetry]
name = "thunderpropagator-client"
version = "1.0.0"
description = "Real-time data streaming client for Python"
authors = ["Your Name <your.email@example.com>"]
license = "MIT"
readme = "README.md"
homepage = "https://github.com/yourusername/thunderpropagator-python"
repository = "https://github.com/yourusername/thunderpropagator-python"
documentation = "https://thunderpropagator-python.readthedocs.io"
keywords = ["websocket", "quic", "streaming", "real-time"]
classifiers = [
    "Development Status :: 4 - Beta",
    "Intended Audience :: Developers",
    "Programming Language :: Python :: 3.10",
]

[tool.poetry.dependencies]
python = "^3.10"
websockets = "^12.0"
aioquic = "^0.9.0"
aiohttp = "^3.9.0"
cryptography = "^41.0"
pydantic = "^2.5"
typing-extensions = "^4.8"

[tool.poetry.group.dev.dependencies]
pytest = "^7.4"
pytest-asyncio = "^0.21"
pytest-cov = "^4.1"
black = "^23.0"
mypy = "^1.7"
ruff = "^0.1"

[build-system]
requires = ["poetry-core"]
build-backend = "poetry.core.masonry.api"
```

### 9. Package Testing Checklist

```bash
# Test import
python -c "import thunderpropagator; print(thunderpropagator.__version__)"

# Test installation from local build
pip install dist/thunderpropagator_client-1.0.0-py3-none-any.whl

# Test in clean environment
python -m venv test_env
source test_env/bin/activate
pip install thunderpropagator-client
python -c "from thunderpropagator import WebSocketClient"
deactivate
rm -rf test_env
```

### 10. Documentation with Sphinx

```bash
# Install Sphinx
pip install sphinx sphinx-rtd-theme sphinx-autodoc-typehints

# Initialize docs
mkdir docs
cd docs
sphinx-quickstart

# Build documentation
sphinx-build -b html . _build

# Host on ReadTheDocs
# Connect your GitHub repo at https://readthedocs.org
```

**docs/conf.py**:
```python
import os
import sys
sys.path.insert(0, os.path.abspath('../src'))

project = 'ThunderPropagator Python Client'
copyright = '2025, Your Name'
author = 'Your Name'
version = '1.0.0'

extensions = [
    'sphinx.ext.autodoc',
    'sphinx.ext.napoleon',
    'sphinx.ext.viewcode',
    'sphinx_autodoc_typehints',
]

html_theme = 'sphinx_rtd_theme'
```

### 11. PyPI Package Badges

Add to README.md:

```markdown
[![PyPI version](https://badge.fury.io/py/thunderpropagator-client.svg)](https://pypi.org/project/thunderpropagator-client/)
[![Python versions](https://img.shields.io/pypi/pyversions/thunderpropagator-client.svg)](https://pypi.org/project/thunderpropagator-client/)
[![License](https://img.shields.io/pypi/l/thunderpropagator-client.svg)](https://github.com/yourusername/thunderpropagator-python/blob/main/LICENSE)
[![Downloads](https://pepy.tech/badge/thunderpropagator-client)](https://pepy.tech/project/thunderpropagator-client)
```

### 12. Post-Publishing Checklist

- [ ] Verify package on PyPI.org
- [ ] Test installation: `pip install thunderpropagator-client`
- [ ] Check package on libraries.io
- [ ] Update GitHub release notes
- [ ] Update documentation site (ReadTheDocs)
- [ ] Announce on Python forums/Reddit
- [ ] Monitor PyPI download stats
- [ ] Respond to issues/PRs

---

## Key Python-Specific Considerations

1. **Type Hints**: Use modern type hints (PEP 604 union syntax `X | Y`)
2. **Async/Await**: All I/O operations use asyncio
3. **Context Managers**: Support `async with` for auto cleanup
4. **Pydantic**: Validation and serialization
5. **No GIL**: Consider using asyncio for true concurrency
6. **Logging**: Use standard `logging` module, don't create custom
7. **Packaging**: Use `pyproject.toml` with modern build system
8. **Type Stubs**: Include `py.typed` for PEP 561 compliance

---

## Implementation Checklist

- [ ] Set up project structure with `pyproject.toml`
- [ ] Configure build system (setuptools or Poetry)
- [ ] Implement `AbstractConnection` base class
- [ ] Implement `WebSocketConnection`
- [ ] Implement `QuicConnection` (using aioquic)
- [ ] Implement `InfiniteDataStreamConnection`
- [ ] Implement `AbstractChannel`
- [ ] Implement `ThunderPropagatorClient` base
- [ ] Create protocol-specific clients
- [ ] Define all Pydantic models
- [ ] Implement encryption utilities
- [ ] Add `py.typed` marker file
- [ ] Write unit tests (pytest)
- [ ] Write integration tests
- [ ] Add type hints and pass mypy strict mode
- [ ] Create usage examples
- [ ] Write comprehensive README
- [ ] Add LICENSE and CHANGELOG
- [ ] Write documentation (Sphinx)
- [ ] Set up CI/CD (GitHub Actions)
- [ ] Test package locally
- [ ] Upload to TestPyPI
- [ ] Publish to PyPI
- [ ] Monitor package analytics
