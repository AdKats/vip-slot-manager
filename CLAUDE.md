# VipSlotManager — Procon v2 Plugin

## Project Overview

VipSlotManager is a Procon v2 plugin that manages VIP reserved slots for Battlefield servers. It provides database-backed VIP management with in-game commands, automatic sync, and a web API for external tools.

- **Language:** C#
- **License:** GPLv3
- **Supported games:** BF3, BF4, BFH, BFBC2
- **Original author:** maxdralle
- **Maintainer:** Prophet731
- **Dependencies:** MySqlConnector, Dapper, Procon v2 (runtime only)

## Architecture

| File | Responsibility |
|------|---------------|
| `src/VipSlotManager.cs` | Main entry point, plugin metadata, lifecycle, core state, helper methods |
| `src/VipSlotManager/Settings.cs` | Plugin variables UI and persistence |
| `src/VipSlotManager/Events.cs` | Procon event handlers |
| `src/VipSlotManager/Commands.cs` | In-game chat command handling |
| `src/VipSlotManager/Database.cs` | MySQL operations via MySqlConnector |

## Code Style

See the master `CLAUDE.md` at the procon_plugins root for shared conventions.

## Build & CI

- `VipSlotManager.csproj` at root is a **CI-only artifact** for `dotnet format`.
- **CI workflow** (`.github/workflows/ci.yml`): `dotnet format` checks on push/PR.
- **Release workflow** (`.github/workflows/release.yml`): tag-triggered release packaging.

### Running style checks locally

```bash
dotnet restore
dotnet format whitespace --verify-no-changes
dotnet format style --verify-no-changes --severity warn --exclude-diagnostics IDE1007
```

## Threading Model

Uses ThreadPool.QueueUserWorkItem for async database operations and periodic sync via Timer callbacks.

## Event Registrations

```
OnListPlayers, OnPlayerJoin, OnPlayerLeft, OnPlayerSpawned,
OnGlobalChat, OnTeamChat, OnSquadChat,
OnServerInfo, OnLevelLoaded, OnRoundOver,
OnReservedSlotsList, OnReservedSlotsListAggressiveJoin
```

## Database

Key tables:
- VIP player list with expiration dates
- Server-specific VIP assignments
- Sync state tracking

## Supported Games

- BF3, BF4, BFH, BFBC2

## Branch Structure

- `master` — current development, Procon v2 only
- `legacy` — archived pre-refactor code
