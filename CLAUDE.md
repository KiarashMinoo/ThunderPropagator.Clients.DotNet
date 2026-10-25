# CLAUDE.md

If a more general agent-instructions doc exists at the repo root, read it first as primary source — this file adds Claude-specific notes on top.

## Commands

```powershell
dotnet restore    # always run before build/test; fetches shared build config on first use
dotnet build -c Release -p:Platform=AnyCPU
dotnet build -c Release                       # all configured platforms
dotnet test <UnitTestProject>
dotnet test <ArchTestProject>
dotnet clean      # also clears the downloaded shared build cache; restore again afterward
```

## Architecture

Four strict layers, never skipped or crossed:

```
Application
  ↓
Client        # protocol selection, channel factory, payload routing
  ↓
Channel       # request/response tracking, auth encryption/decryption, subscriptions
  ↓
Connection    # transport lifecycle, receive loop
```

Protocol implementations live under connection/channel/client-named folders; abstractions live exclusively in an infrastructure folder.

- **Payload routing (client layer)**: structured payloads → response handler; unstructured payloads split on a channel/message delimiter → message handler. First non-probe frame received is always treated as connection-info, before normal dispatch.
- **Multiple protocol implementations** side by side (WebSocket, a runtime-gated modern-transport, a raw streaming impl) behind the same connection abstraction.

## Conventions

- **Logging** — this repo's own logger/logger-provider abstractions, never a generic logging framework directly.
- **JSON** — this repo's own serialize/deserialize helpers, no ad-hoc serializers.
- **Configuration** — get/set property pattern from the shared configuration base, no private backing fields.
- **Sealing** — concrete protocol/client/channel classes: sealed in Release, non-sealed in Debug (`#if !DEBUG sealed #endif`) — never add `sealed` without that guard.
- **Naming** — public types carry the product's root-namespace prefix; narrow suffix-based exceptions exist (helper/descriptor/metadata types). Architecture-test enforced.

## Server compatibility

The wire contract the client implements (JSON control plane, proprietary push format) has drifted from the current server (`Key=Value` control plane, server-selected push format, 403 on bad credentials). Details, known client defects and draft issues: `AGENTS.md` ("Server compatibility status") and `.github/issue-drafts/`. Re-check the server before changing wire parsing or request building.

## Adding a Protocol Implementation

New connection type (implements connection abstraction) → channel implementation wiring auth/subscriptions on it → client-side registration for the factory → tests (connection lifecycle, payload routing, failure/reconnect paths).

## Build & Versioning

Version/TFMs centralized; CI bumps automatically. Restore fetches shared build config into a local, gitignored cache — if missing, build fails fast with cascading type errors; always restore first.
