# ThunderPropagator.Clients.DotNet Copilot Instructions

Use `AGENTS.md` at repo root as the primary instruction source. This file stays intentionally short and only adds high-priority reminders.

## Quick start
- Run `dotnet restore` before build/test.
- Build with `dotnet build -c Release -p:Platform=AnyCPU` (or `dotnet build -c Release` for all configured platforms).
- Run tests explicitly:
  - `dotnet test Tests/UnitTests/UnitTests.csproj`
  - `dotnet test Tests/ArchTests/ArchTests.csproj`

## Critical gotchas
- Restore downloads required shared props into `.shared-props/` via `Directory.Build.props`; if this fails, builds can produce cascading missing-type errors.
- `dotnet clean` removes `.shared-props/`; run `dotnet restore` again afterward.
- Do not use `Microsoft.Extensions.Logging` in library code. Use `Infrastructure/Loggers/ILogger.cs` and `Infrastructure/Loggers/ILoggerProvider.cs`.
- Keep configuration classes on the `Get<T>()` / `Set()` property pattern from `AbstractThunderPropagatorConfiguration`.
- Preserve first non-`PROBE` frame behavior: treat it as `ThunderPropagatorConnectionResponse` before normal channel routing.

## Architecture and conventions
- Follow the 3-layer flow: Client -> Channel -> Connection.
- Keep abstractions in `Infrastructure/*`; keep protocol-specific concrete types in `Clients/*`, `Channels/*`, `Connections/*`.
- Keep conditional sealing pattern for concrete protocol classes: `#if !DEBUG sealed #endif`.
- Respect architecture/naming rules enforced by `Tests/ArchTests/ArchitectureTests.cs`.

## Link-first references
- `AGENTS.md`
- `docs/README.md`
- `docs/Clients/README.md`
- `docs/Channels/README.md`
- `docs/Connections/README.md`
- `docs/Infrastructure/README.md`
- `docs/Models/README.md`
