# ThunderPropagator C++ Client - Implementation Prompt

## Project Goal
Create a C++ client library for ThunderPropagator real-time data streaming framework supporting WebSocket, QUIC, and custom protocols.

## Target Environment
- **C++ Version**: C++20
- **Build System**: CMake
- **Package Distribution**: vcpkg, Conan
- **Testing**: Google Test, Catch2

---

## Project Structure

```
thunderpropagator-cpp/
├── CMakeLists.txt
├── README.md
├── LICENSE
├── CHANGELOG.md
├── .gitignore
├── conanfile.py
├── vcpkg.json
├── include/
│   └── thunderpropagator/
│       ├── client.hpp
│       ├── connection.hpp
│       ├── channel.hpp
│       ├── config.hpp
│       ├── errors.hpp
│       ├── connections/
│       │   ├── websocket_connection.hpp
│       │   ├── quic_connection.hpp
│       │   └── infinite_data_stream_connection.hpp
│       ├── models/
│       │   ├── enums.hpp
│       │   ├── messages.hpp
│       │   ├── metadata.hpp
│       │   └── subscriptions.hpp
│       └── crypto/
│           ├── aes.hpp
│           └── rsa.hpp
├── src/
│   ├── client.cpp
│   ├── connection.cpp
│   ├── channel.cpp
│   ├── connections/
│   │   ├── websocket_connection.cpp
│   │   ├── quic_connection.cpp
│   │   └── infinite_data_stream_connection.cpp
│   ├── crypto/
│   │   ├── aes.cpp
│   │   └── rsa.cpp
│   └── utils/
│       └── json_helpers.cpp
├── tests/
│   ├── CMakeLists.txt
│   ├── unit/
│   │   ├── connection_test.cpp
│   │   └── channel_test.cpp
│   └── integration/
│       └── integration_test.cpp
├── examples/
│   ├── websocket_example.cpp
│   ├── quic_example.cpp
│   └── advanced_example.cpp
└── cmake/
    ├── FindWebSocketPP.cmake
    └── ThunderPropagatorConfig.cmake.in
```

---

## Build Configuration

**CMakeLists.txt**:
```cmake
cmake_minimum_required(VERSION 3.20)
project(ThunderPropagator VERSION 1.0.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# Options
option(BUILD_SHARED_LIBS "Build shared libraries" ON)
option(BUILD_TESTS "Build tests" ON)
option(BUILD_EXAMPLES "Build examples" ON)

# Dependencies
find_package(Boost 1.80 REQUIRED COMPONENTS system thread)
find_package(OpenSSL REQUIRED)
find_package(nlohmann_json 3.11 REQUIRED)
find_package(websocketpp REQUIRED)
find_package(spdlog REQUIRED)

# Library
add_library(thunderpropagator
    src/client.cpp
    src/connection.cpp
    src/channel.cpp
    src/connections/websocket_connection.cpp
    src/connections/quic_connection.cpp
    src/connections/infinite_data_stream_connection.cpp
    src/crypto/aes.cpp
    src/crypto/rsa.cpp
    src/utils/json_helpers.cpp
)

target_include_directories(thunderpropagator
    PUBLIC
        $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
        $<INSTALL_INTERFACE:include>
    PRIVATE
        ${CMAKE_CURRENT_SOURCE_DIR}/src
)

target_link_libraries(thunderpropagator
    PUBLIC
        Boost::system
        Boost::thread
        OpenSSL::SSL
        OpenSSL::Crypto
        nlohmann_json::nlohmann_json
        websocketpp::websocketpp
        spdlog::spdlog
)

# Compiler warnings
if(MSVC)
    target_compile_options(thunderpropagator PRIVATE /W4 /WX)
else()
    target_compile_options(thunderpropagator PRIVATE -Wall -Wextra -Wpedantic -Werror)
endif()

# Tests
if(BUILD_TESTS)
    enable_testing()
    add_subdirectory(tests)
endif()

# Examples
if(BUILD_EXAMPLES)
    add_subdirectory(examples)
endif()

# Installation
include(GNUInstallDirs)
install(TARGETS thunderpropagator
    EXPORT ThunderPropagatorTargets
    LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR}
    ARCHIVE DESTINATION ${CMAKE_INSTALL_LIBDIR}
    RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR}
)

install(DIRECTORY include/
    DESTINATION ${CMAKE_INSTALL_INCLUDEDIR}
)

install(EXPORT ThunderPropagatorTargets
    FILE ThunderPropagatorTargets.cmake
    NAMESPACE ThunderPropagator::
    DESTINATION ${CMAKE_INSTALL_LIBDIR}/cmake/ThunderPropagator
)

include(CMakePackageConfigHelpers)
configure_package_config_file(
    ${CMAKE_CURRENT_SOURCE_DIR}/cmake/ThunderPropagatorConfig.cmake.in
    ${CMAKE_CURRENT_BINARY_DIR}/ThunderPropagatorConfig.cmake
    INSTALL_DESTINATION ${CMAKE_INSTALL_LIBDIR}/cmake/ThunderPropagator
)

write_basic_package_version_file(
    ${CMAKE_CURRENT_BINARY_DIR}/ThunderPropagatorConfigVersion.cmake
    VERSION ${PROJECT_VERSION}
    COMPATIBILITY SameMajorVersion
)

install(FILES
    ${CMAKE_CURRENT_BINARY_DIR}/ThunderPropagatorConfig.cmake
    ${CMAKE_CURRENT_BINARY_DIR}/ThunderPropagatorConfigVersion.cmake
    DESTINATION ${CMAKE_INSTALL_LIBDIR}/cmake/ThunderPropagator
)
```

**conanfile.py**:
```python
from conan import ConanFile
from conan.tools.cmake import CMakeToolchain, CMake, cmake_layout

class ThunderPropagatorConan(ConanFile):
    name = "thunderpropagator"
    version = "1.0.0"
    license = "MIT"
    author = "Your Name <your.email@example.com>"
    url = "https://github.com/yourusername/thunderpropagator-cpp"
    description = "Real-time data streaming client for C++"
    topics = ("websocket", "quic", "streaming", "real-time")
    settings = "os", "compiler", "build_type", "arch"
    options = {
        "shared": [True, False],
        "fPIC": [True, False]
    }
    default_options = {
        "shared": False,
        "fPIC": True
    }
    exports_sources = "CMakeLists.txt", "src/*", "include/*", "cmake/*"
    
    def requirements(self):
        self.requires("boost/1.82.0")
        self.requires("openssl/3.1.0")
        self.requires("nlohmann_json/3.11.2")
        self.requires("websocketpp/0.8.2")
        self.requires("spdlog/1.12.0")
    
    def build_requirements(self):
        self.test_requires("gtest/1.13.0")
    
    def layout(self):
        cmake_layout(self)
    
    def generate(self):
        tc = CMakeToolchain(self)
        tc.generate()
    
    def build(self):
        cmake = CMake(self)
        cmake.configure()
        cmake.build()
    
    def package(self):
        cmake = CMake(self)
        cmake.install()
    
    def package_info(self):
        self.cpp_info.libs = ["thunderpropagator"]
```

**vcpkg.json**:
```json
{
  "name": "thunderpropagator",
  "version": "1.0.0",
  "description": "Real-time data streaming client for C++",
  "homepage": "https://github.com/yourusername/thunderpropagator-cpp",
  "license": "MIT",
  "dependencies": [
    "boost-system",
    "boost-thread",
    "boost-asio",
    "openssl",
    "nlohmann-json",
    "websocketpp",
    "spdlog"
  ],
  "dev-dependencies": [
    "gtest"
  ]
}
```

---

## Design Patterns & Idioms

### 1. Abstract Base Classes with Virtual Methods

**include/thunderpropagator/connection.hpp**:
```cpp
#pragma once

#include <string>
#include <memory>
#include <functional>
#include <atomic>
#include <mutex>

namespace thunderpropagator {

enum class ConnectionState {
    Ready,
    Connecting,
    Open,
    Closing,
    Closed,
    HasError
};

using MessageHandler = std::function<void(const std::string&)>;
using StateChangeHandler = std::function<void(ConnectionState, ConnectionState)>;

class IConnection {
public:
    virtual ~IConnection() = default;
    
    virtual void connect() = 0;
    virtual void disconnect() = 0;
    virtual void send(const std::string& message) = 0;
    
    virtual bool is_connected() const = 0;
    virtual std::string connection_id() const = 0;
    virtual ConnectionState state() const = 0;
    
    virtual void on_message_received(MessageHandler handler) = 0;
    virtual void on_state_changed(StateChangeHandler handler) = 0;
};

class AbstractConnection : public IConnection {
public:
    AbstractConnection();
    virtual ~AbstractConnection();
    
    // IConnection implementation
    bool is_connected() const override;
    std::string connection_id() const override;
    ConnectionState state() const override;
    
    void on_message_received(MessageHandler handler) override;
    void on_state_changed(StateChangeHandler handler) override;
    
protected:
    void set_state(ConnectionState new_state);
    void emit_message(const std::string& message);
    void set_connection_id(const std::string& id);
    
private:
    mutable std::mutex mutex_;
    std::atomic<ConnectionState> state_;
    std::string connection_id_;
    std::vector<MessageHandler> message_handlers_;
    std::vector<StateChangeHandler> state_handlers_;
};

} // namespace thunderpropagator
```

### 2. RAII and Smart Pointers

**include/thunderpropagator/channel.hpp**:
```cpp
#pragma once

#include "connection.hpp"
#include "models/metadata.hpp"
#include <nlohmann/json.hpp>
#include <optional>
#include <unordered_map>
#include <future>

namespace thunderpropagator {

enum class ChannelState {
    Ready,
    Opening,
    Open,
    Closing,
    Closed
};

class Channel : public std::enable_shared_from_this<Channel> {
public:
    Channel(std::string name, std::shared_ptr<IConnection> connection);
    ~Channel();
    
    // Delete copy constructor and assignment
    Channel(const Channel&) = delete;
    Channel& operator=(const Channel&) = delete;
    
    // Allow move
    Channel(Channel&&) noexcept = default;
    Channel& operator=(Channel&&) noexcept = default;
    
    void open();
    void close();
    
    nlohmann::json subscribe(const nlohmann::json& subscription);
    void handle_message(const nlohmann::json& message);
    
    ChannelState state() const;
    std::string name() const { return name_; }
    
private:
    void wait_for_metadata();
    
    std::string name_;
    std::atomic<ChannelState> state_;
    std::shared_ptr<IConnection> connection_;
    std::optional<ChannelMetadata> metadata_;
    
    mutable std::mutex mutex_;
    std::unordered_map<std::string, std::promise<nlohmann::json>> pending_requests_;
};

} // namespace thunderpropagator
```

### 3. Modern C++20 Features

**include/thunderpropagator/models/enums.hpp**:
```cpp
#pragma once

#include <string>
#include <string_view>

namespace thunderpropagator {

enum class ConnectionState {
    Ready,
    Connecting,
    Open,
    Closing,
    Closed,
    HasError
};

constexpr std::string_view to_string(ConnectionState state) {
    using enum ConnectionState;
    switch (state) {
        case Ready: return "Ready";
        case Connecting: return "Connecting";
        case Open: return "Open";
        case Closing: return "Closing";
        case Closed: return "Closed";
        case HasError: return "HasError";
    }
    return "Unknown";
}

enum class ChannelState {
    Ready,
    Opening,
    Open,
    Closing,
    Closed
};

constexpr std::string_view to_string(ChannelState state) {
    using enum ChannelState;
    switch (state) {
        case Ready: return "Ready";
        case Opening: return "Opening";
        case Open: return "Open";
        case Closing: return "Closing";
        case Closed: return "Closed";
    }
    return "Unknown";
}

enum class ProtocolType {
    WebSocket,
    Quic,
    InfiniteDataStream
};

constexpr std::string_view to_string(ProtocolType protocol) {
    using enum ProtocolType;
    switch (protocol) {
        case WebSocket: return "WebSocket";
        case Quic: return "QUIC";
        case InfiniteDataStream: return "InfiniteDataStream";
    }
    return "Unknown";
}

} // namespace thunderpropagator
```

### 4. Exception Hierarchy

**include/thunderpropagator/errors.hpp**:
```cpp
#pragma once

#include <stdexcept>
#include <string>

namespace thunderpropagator {

class ThunderPropagatorException : public std::runtime_error {
public:
    explicit ThunderPropagatorException(const std::string& message)
        : std::runtime_error(message) {}
};

class ConnectionException : public ThunderPropagatorException {
public:
    explicit ConnectionException(const std::string& message)
        : ThunderPropagatorException("Connection error: " + message) {}
};

class AlreadyConnectedException : public ConnectionException {
public:
    AlreadyConnectedException()
        : ConnectionException("Already connected") {}
};

class NotConnectedException : public ConnectionException {
public:
    NotConnectedException()
        : ConnectionException("Not connected") {}
};

class SendFailedException : public ConnectionException {
public:
    explicit SendFailedException(const std::string& reason)
        : ConnectionException("Send failed: " + reason) {}
};

class ChannelException : public ThunderPropagatorException {
public:
    explicit ChannelException(const std::string& channel, const std::string& message)
        : ThunderPropagatorException("Channel '" + channel + "': " + message),
          channel_(channel) {}
    
    const std::string& channel() const { return channel_; }
    
private:
    std::string channel_;
};

class TimeoutException : public ThunderPropagatorException {
public:
    TimeoutException()
        : ThunderPropagatorException("Operation timed out") {}
};

} // namespace thunderpropagator
```

---

## Implementation Details

### WebSocket Connection

**include/thunderpropagator/connections/websocket_connection.hpp**:
```cpp
#pragma once

#include "../connection.hpp"
#include "../config.hpp"
#include <websocketpp/config/asio_client.hpp>
#include <websocketpp/client.hpp>
#include <thread>

namespace thunderpropagator {

using WebSocketClient = websocketpp::client<websocketpp::config::asio_tls_client>;
using WebSocketHandle = websocketpp::connection_hdl;

class WebSocketConnection : public AbstractConnection {
public:
    explicit WebSocketConnection(const WebSocketConfig& config);
    ~WebSocketConnection() override;
    
    void connect() override;
    void disconnect() override;
    void send(const std::string& message) override;
    
private:
    void on_open(WebSocketHandle hdl);
    void on_close(WebSocketHandle hdl);
    void on_message(WebSocketHandle hdl, WebSocketClient::message_ptr msg);
    void on_fail(WebSocketHandle hdl);
    
    void run_io_service();
    
    WebSocketConfig config_;
    WebSocketClient client_;
    WebSocketHandle connection_handle_;
    std::thread io_thread_;
    std::atomic<bool> running_;
};

} // namespace thunderpropagator
```

**src/connections/websocket_connection.cpp**:
```cpp
#include "thunderpropagator/connections/websocket_connection.hpp"
#include "thunderpropagator/errors.hpp"
#include <spdlog/spdlog.h>

namespace thunderpropagator {

WebSocketConnection::WebSocketConnection(const WebSocketConfig& config)
    : config_(config), running_(false) {
    
    client_.clear_access_channels(websocketpp::log::alevel::all);
    client_.clear_error_channels(websocketpp::log::elevel::all);
    
    client_.init_asio();
    client_.start_perpetual();
    
    client_.set_open_handler([this](WebSocketHandle hdl) {
        on_open(hdl);
    });
    
    client_.set_close_handler([this](WebSocketHandle hdl) {
        on_close(hdl);
    });
    
    client_.set_message_handler([this](WebSocketHandle hdl, WebSocketClient::message_ptr msg) {
        on_message(hdl, msg);
    });
    
    client_.set_fail_handler([this](WebSocketHandle hdl) {
        on_fail(hdl);
    });
    
    // TLS initialization
    client_.set_tls_init_handler([](WebSocketHandle) {
        auto ctx = std::make_shared<boost::asio::ssl::context>(
            boost::asio::ssl::context::tlsv12_client
        );
        ctx->set_default_verify_paths();
        return ctx;
    });
}

WebSocketConnection::~WebSocketConnection() {
    if (running_) {
        disconnect();
    }
}

void WebSocketConnection::connect() {
    if (state() != ConnectionState::Ready) {
        throw AlreadyConnectedException();
    }
    
    set_state(ConnectionState::Connecting);
    
    spdlog::info("Connecting to {}", config_.uri);
    
    websocketpp::lib::error_code ec;
    auto con = client_.get_connection(config_.uri, ec);
    
    if (ec) {
        set_state(ConnectionState::HasError);
        throw ConnectionException("Get connection failed: " + ec.message());
    }
    
    // Add headers
    for (const auto& [key, value] : config_.headers) {
        con->append_header(key, value);
    }
    
    connection_handle_ = con->get_handle();
    client_.connect(con);
    
    running_ = true;
    io_thread_ = std::thread([this] { run_io_service(); });
}

void WebSocketConnection::disconnect() {
    if (state() == ConnectionState::Closed) {
        return;
    }
    
    set_state(ConnectionState::Closing);
    
    websocketpp::lib::error_code ec;
    client_.close(connection_handle_, websocketpp::close::status::normal, "Closing", ec);
    
    if (ec) {
        spdlog::error("Close failed: {}", ec.message());
    }
    
    running_ = false;
    client_.stop_perpetual();
    
    if (io_thread_.joinable()) {
        io_thread_.join();
    }
    
    set_state(ConnectionState::Closed);
}

void WebSocketConnection::send(const std::string& message) {
    if (state() != ConnectionState::Open) {
        throw NotConnectedException();
    }
    
    websocketpp::lib::error_code ec;
    client_.send(connection_handle_, message, websocketpp::frame::opcode::text, ec);
    
    if (ec) {
        throw SendFailedException(ec.message());
    }
}

void WebSocketConnection::on_open(WebSocketHandle hdl) {
    spdlog::info("WebSocket connection opened");
    set_state(ConnectionState::Open);
}

void WebSocketConnection::on_close(WebSocketHandle hdl) {
    spdlog::info("WebSocket connection closed");
    set_state(ConnectionState::Closed);
}

void WebSocketConnection::on_message(WebSocketHandle hdl, WebSocketClient::message_ptr msg) {
    const auto& payload = msg->get_payload();
    
    if (payload != "PROBE") {
        emit_message(payload);
    }
}

void WebSocketConnection::on_fail(WebSocketHandle hdl) {
    auto con = client_.get_con_from_hdl(hdl);
    spdlog::error("WebSocket connection failed: {}", con->get_ec().message());
    set_state(ConnectionState::HasError);
}

void WebSocketConnection::run_io_service() {
    try {
        client_.run();
    } catch (const std::exception& e) {
        spdlog::error("IO service error: {}", e.what());
        set_state(ConnectionState::HasError);
    }
}

} // namespace thunderpropagator
```

### Channel Implementation

**src/channel.cpp**:
```cpp
#include "thunderpropagator/channel.hpp"
#include "thunderpropagator/errors.hpp"
#include <spdlog/spdlog.h>
#include <chrono>

using namespace std::chrono_literals;
using json = nlohmann::json;

namespace thunderpropagator {

Channel::Channel(std::string name, std::shared_ptr<IConnection> connection)
    : name_(std::move(name)),
      state_(ChannelState::Ready),
      connection_(std::move(connection)) {
}

Channel::~Channel() {
    if (state_ != ChannelState::Closed) {
        try {
            close();
        } catch (...) {
            // Ignore exceptions in destructor
        }
    }
}

void Channel::open() {
    ChannelState expected = ChannelState::Ready;
    if (!state_.compare_exchange_strong(expected, ChannelState::Opening)) {
        throw ChannelException(name_, "Invalid state for open");
    }
    
    json request = {
        {"route", {
            {"channel", name_},
            {"endpoint", "open"}
        }},
        {"timestamp", std::chrono::system_clock::now().time_since_epoch().count()}
    };
    
    connection_->send(request.dump());
    
    wait_for_metadata();
    
    state_ = ChannelState::Open;
}

void Channel::close() {
    ChannelState expected = ChannelState::Open;
    if (state_.compare_exchange_strong(expected, ChannelState::Closing)) {
        json request = {
            {"route", {
                {"channel", name_},
                {"endpoint", "close"}
            }}
        };
        
        connection_->send(request.dump());
        state_ = ChannelState::Closed;
    }
}

json Channel::subscribe(const json& subscription) {
    if (state_ != ChannelState::Open) {
        throw ChannelException(name_, "Channel not open");
    }
    
    std::string request_id = generate_uuid();
    
    json request = {
        {"id", request_id},
        {"route", {
            {"channel", name_},
            {"endpoint", "subscribe"}
        }},
        {"payload", subscription}
    };
    
    std::promise<json> response_promise;
    auto response_future = response_promise.get_future();
    
    {
        std::lock_guard<std::mutex> lock(mutex_);
        pending_requests_[request_id] = std::move(response_promise);
    }
    
    connection_->send(request.dump());
    
    auto status = response_future.wait_for(30s);
    
    if (status == std::future_status::timeout) {
        std::lock_guard<std::mutex> lock(mutex_);
        pending_requests_.erase(request_id);
        throw TimeoutException();
    }
    
    return response_future.get();
}

void Channel::handle_message(const json& message) {
    // Check if response to pending request
    if (message.contains("id")) {
        std::string id = message["id"];
        
        std::lock_guard<std::mutex> lock(mutex_);
        auto it = pending_requests_.find(id);
        if (it != pending_requests_.end()) {
            it->second.set_value(message["payload"]);
            pending_requests_.erase(it);
            return;
        }
    }
    
    // Check if metadata
    if (message.contains("metadata")) {
        std::lock_guard<std::mutex> lock(mutex_);
        metadata_ = message["metadata"].get<ChannelMetadata>();
        return;
    }
    
    // Regular message
}

void Channel::wait_for_metadata() {
    auto start = std::chrono::steady_clock::now();
    
    while (std::chrono::steady_clock::now() - start < 30s) {
        {
            std::lock_guard<std::mutex> lock(mutex_);
            if (metadata_.has_value()) {
                return;
            }
        }
        std::this_thread::sleep_for(100ms);
    }
    
    throw TimeoutException();
}

ChannelState Channel::state() const {
    return state_.load();
}

} // namespace thunderpropagator
```

### Client Implementation

**include/thunderpropagator/client.hpp**:
```cpp
#pragma once

#include "connection.hpp"
#include "channel.hpp"
#include <unordered_map>
#include <memory>
#include <mutex>

namespace thunderpropagator {

class Client {
public:
    explicit Client(std::shared_ptr<IConnection> connection);
    ~Client();
    
    void connect();
    void disconnect();
    
    std::shared_ptr<Channel> create_channel(const std::string& name);
    std::shared_ptr<Channel> get_channel(const std::string& name);
    
private:
    std::shared_ptr<IConnection> connection_;
    std::unordered_map<std::string, std::shared_ptr<Channel>> channels_;
    mutable std::mutex mutex_;
};

class WebSocketClient {
public:
    explicit WebSocketClient(const WebSocketConfig& config);
    
    void connect();
    void disconnect();
    
    std::shared_ptr<Channel> create_channel(const std::string& name);
    
private:
    std::unique_ptr<Client> client_;
    std::shared_ptr<IConnection> connection_;
};

} // namespace thunderpropagator
```

**src/client.cpp**:
```cpp
#include "thunderpropagator/client.hpp"
#include "thunderpropagator/connections/websocket_connection.hpp"
#include "thunderpropagator/errors.hpp"

namespace thunderpropagator {

Client::Client(std::shared_ptr<IConnection> connection)
    : connection_(std::move(connection)) {
}

Client::~Client() {
    disconnect();
}

void Client::connect() {
    connection_->connect();
}

void Client::disconnect() {
    // Close all channels
    {
        std::lock_guard<std::mutex> lock(mutex_);
        for (auto& [name, channel] : channels_) {
            channel->close();
        }
        channels_.clear();
    }
    
    connection_->disconnect();
}

std::shared_ptr<Channel> Client::create_channel(const std::string& name) {
    {
        std::lock_guard<std::mutex> lock(mutex_);
        auto it = channels_.find(name);
        if (it != channels_.end()) {
            return it->second;
        }
    }
    
    auto channel = std::make_shared<Channel>(name, connection_);
    channel->open();
    
    {
        std::lock_guard<std::mutex> lock(mutex_);
        channels_[name] = channel;
    }
    
    return channel;
}

std::shared_ptr<Channel> Client::get_channel(const std::string& name) {
    std::lock_guard<std::mutex> lock(mutex_);
    auto it = channels_.find(name);
    if (it != channels_.end()) {
        return it->second;
    }
    return nullptr;
}

WebSocketClient::WebSocketClient(const WebSocketConfig& config) {
    connection_ = std::make_shared<WebSocketConnection>(config);
    client_ = std::make_unique<Client>(connection_);
}

void WebSocketClient::connect() {
    client_->connect();
}

void WebSocketClient::disconnect() {
    client_->disconnect();
}

std::shared_ptr<Channel> WebSocketClient::create_channel(const std::string& name) {
    return client_->create_channel(name);
}

} // namespace thunderpropagator
```

---

## Usage Example

**examples/websocket_example.cpp**:
```cpp
#include <thunderpropagator/client.hpp>
#include <thunderpropagator/config.hpp>
#include <spdlog/spdlog.h>
#include <nlohmann/json.hpp>
#include <chrono>
#include <thread>

using namespace thunderpropagator;
using namespace std::chrono_literals;
using json = nlohmann::json;

int main() {
    try {
        // Configure logging
        spdlog::set_level(spdlog::level::info);
        
        // Create configuration
        WebSocketConfig config;
        config.uri = "wss://example.com/thunder";
        config.headers["Authorization"] = "Bearer token123";
        config.connection_timeout = 30s;
        config.keep_alive_interval = 20s;
        
        // Create client
        WebSocketClient client(config);
        
        // Connect
        client.connect();
        spdlog::info("Connected to ThunderPropagator");
        
        // Create channel
        auto channel = client.create_channel("market-data");
        spdlog::info("Channel '{}' opened", channel->name());
        
        // Subscribe
        json subscription = {
            {"dataType", "quotes"},
            {"symbols", {"AAPL", "GOOGL"}}
        };
        
        auto response = channel->subscribe(subscription);
        spdlog::info("Subscribed: {}", response["subscriptionId"].get<std::string>());
        
        // Keep running
        std::this_thread::sleep_for(60s);
        
        // Cleanup
        client.disconnect();
        
    } catch (const ThunderPropagatorException& e) {
        spdlog::error("ThunderPropagator error: {}", e.what());
        return 1;
    } catch (const std::exception& e) {
        spdlog::error("Error: {}", e.what());
        return 1;
    }
    
    return 0;
}
```

---

## Testing

**tests/unit/connection_test.cpp**:
```cpp
#include <gtest/gtest.h>
#include <thunderpropagator/connections/websocket_connection.hpp>

using namespace thunderpropagator;

class WebSocketConnectionTest : public ::testing::Test {
protected:
    void SetUp() override {
        WebSocketConfig config;
        config.uri = "wss://echo.websocket.org";
        connection = std::make_unique<WebSocketConnection>(config);
    }
    
    std::unique_ptr<WebSocketConnection> connection;
};

TEST_F(WebSocketConnectionTest, ConnectAndDisconnect) {
    EXPECT_EQ(connection->state(), ConnectionState::Ready);
    
    connection->connect();
    std::this_thread::sleep_for(std::chrono::seconds(2));
    
    EXPECT_TRUE(connection->is_connected());
    EXPECT_EQ(connection->state(), ConnectionState::Open);
    
    connection->disconnect();
    EXPECT_EQ(connection->state(), ConnectionState::Closed);
}

TEST_F(WebSocketConnectionTest, SendMessage) {
    connection->connect();
    std::this_thread::sleep_for(std::chrono::seconds(2));
    
    EXPECT_NO_THROW(connection->send("test"));
    
    connection->disconnect();
}
```

---

## vcpkg Publishing Guide

### 1. Prerequisites

```bash
# Install vcpkg
git clone https://github.com/Microsoft/vcpkg.git
cd vcpkg
./bootstrap-vcpkg.sh  # Linux/macOS
# or
.\bootstrap-vcpkg.bat  # Windows

# Install vcpkg globally
./vcpkg integrate install
```

### 2. Create Port Files

Create `ports/thunderpropagator/` directory:

**ports/thunderpropagator/portfile.cmake**:
```cmake
vcpkg_from_github(
    OUT_SOURCE_PATH SOURCE_PATH
    REPO yourusername/thunderpropagator-cpp
    REF v1.0.0
    SHA512 <calculated-sha512>
    HEAD_REF main
)

vcpkg_cmake_configure(
    SOURCE_PATH "${SOURCE_PATH}"
    OPTIONS
        -DBUILD_TESTS=OFF
        -DBUILD_EXAMPLES=OFF
)

vcpkg_cmake_install()
vcpkg_cmake_config_fixup(CONFIG_PATH lib/cmake/ThunderPropagator)

file(REMOVE_RECURSE "${CURRENT_PACKAGES_DIR}/debug/include")
file(INSTALL "${SOURCE_PATH}/LICENSE" DESTINATION "${CURRENT_PACKAGES_DIR}/share/${PORT}" RENAME copyright)

vcpkg_copy_pdbs()
```

**ports/thunderpropagator/vcpkg.json**:
```json
{
  "name": "thunderpropagator",
  "version": "1.0.0",
  "description": "Real-time data streaming client for C++",
  "homepage": "https://github.com/yourusername/thunderpropagator-cpp",
  "license": "MIT",
  "dependencies": [
    "boost-system",
    "boost-thread",
    "boost-asio",
    "openssl",
    "nlohmann-json",
    "websocketpp",
    "spdlog"
  ]
}
```

### 3. Test Port Locally

```bash
# Build and install locally
vcpkg install thunderpropagator

# Test in project
cmake -B build -S . -DCMAKE_TOOLCHAIN_FILE=[vcpkg root]/scripts/buildsystems/vcpkg.cmake
cmake --build build
```

### 4. Submit to vcpkg Registry

```bash
# Fork vcpkg repository
git clone https://github.com/yourusername/vcpkg.git
cd vcpkg

# Create branch
git checkout -b add-thunderpropagator

# Add port files
mkdir -p ports/thunderpropagator
cp /path/to/portfile.cmake ports/thunderpropagator/
cp /path/to/vcpkg.json ports/thunderpropagator/

# Update versions
./vcpkg x-add-version thunderpropagator

# Commit and push
git add .
git commit -m "Add thunderpropagator port"
git push origin add-thunderpropagator

# Create Pull Request to microsoft/vcpkg
```

---

## Conan Publishing Guide

### 1. Prerequisites

```bash
# Install Conan
pip install conan

# Configure profile
conan profile detect

# Login to Conan Center
conan remote add conancenter https://center.conan.io
```

### 2. Test Package Locally

```bash
# Create package
conan create . --build=missing

# Test in consumer project
# Add to consumer's conanfile.txt:
[requires]
thunderpropagator/1.0.0

[generators]
CMakeDeps
CMakeToolchain
```

### 3. Publish to Artifactory/Custom Remote

```bash
# Add remote
conan remote add myremote https://myartifactory.com/artifactory/api/conan/myrepo

# Upload
conan upload thunderpropagator/1.0.0 -r myremote --all
```

### 4. Submit to Conan Center

```bash
# Fork conan-center-index
git clone https://github.com/conan-io/conan-center-index.git
cd conan-center-index

# Create recipe
mkdir -p recipes/thunderpropagator/all
cp /path/to/conanfile.py recipes/thunderpropagator/all/
cp /path/to/conandata.yml recipes/thunderpropagator/all/

# Add config
cat > recipes/thunderpropagator/config.yml << EOF
versions:
  "1.0.0":
    folder: all
EOF

# Test recipe
conan create recipes/thunderpropagator/all/ thunderpropagator/1.0.0@

# Submit PR
git checkout -b thunderpropagator
git add recipes/thunderpropagator
git commit -m "Add thunderpropagator recipe"
git push origin thunderpropagator
```

---

## GitHub Actions CI/CD

**.github/workflows/build.yml**:
```yaml
name: Build

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  build:
    strategy:
      matrix:
        os: [ubuntu-latest, macos-latest, windows-latest]
        build_type: [Debug, Release]
    
    runs-on: ${{ matrix.os }}
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Install vcpkg
        uses: lukka/run-vcpkg@v11
        with:
          vcpkgGitCommitId: 'latest'
      
      - name: Configure CMake
        run: cmake -B build -DCMAKE_BUILD_TYPE=${{ matrix.build_type }} -DCMAKE_TOOLCHAIN_FILE=$VCPKG_ROOT/scripts/buildsystems/vcpkg.cmake
      
      - name: Build
        run: cmake --build build --config ${{ matrix.build_type }}
      
      - name: Test
        run: ctest --test-dir build --config ${{ matrix.build_type }} --output-on-failure
```

---

## Key C++-Specific Considerations

1. **Memory Management**: RAII, smart pointers (`std::shared_ptr`, `std::unique_ptr`)
2. **Thread Safety**: `std::mutex`, `std::atomic`, `std::lock_guard`
3. **Modern C++**: Concepts, ranges, coroutines (C++20)
4. **Build System**: CMake for cross-platform builds
5. **Dependencies**: vcpkg or Conan for package management
6. **Exception Safety**: RAII ensures cleanup on exceptions
7. **Performance**: Zero-cost abstractions, move semantics

---

## Implementation Checklist

- [ ] Set up CMake project with C++20
- [ ] Configure vcpkg/Conan dependencies
- [ ] Implement `IConnection` interface
- [ ] Implement `WebSocketConnection`
- [ ] Implement `QuicConnection`
- [ ] Implement `InfiniteDataStreamConnection`
- [ ] Implement `Channel` with RAII
- [ ] Implement `Client` and protocol-specific clients
- [ ] Define all models with JSON support
- [ ] Implement exception hierarchy
- [ ] Implement crypto utilities
- [ ] Write unit tests with GTest
- [ ] Write integration tests
- [ ] Add Doxygen documentation
- [ ] Create usage examples
- [ ] Build on all platforms
- [ ] Create vcpkg port
- [ ] Create Conan recipe
- [ ] Set up CI/CD
- [ ] Publish to vcpkg registry
