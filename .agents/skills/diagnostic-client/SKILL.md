---
name: diagnostic-client
description: 'Play and inspect UnicornHack from an MCP-driven console client. Use when: you need to drive the running UnicornHack.Web game server programmatically (test a feature end-to-end, reproduce a bug, smoke-test a change, or have an agent play a turn); read live game state without launching the Unity client; submit player actions and observe the resulting change sets; query static descriptions or dialog data. The client is a headless .NET console app in test/UnicornHack.DiagnosticClient that connects to the SignalR GameHub exactly like the Unity client and exposes every hub method plus token-efficient state read tools over an MCP stdio server.'
argument-hint: 'Describe the play/inspection task (e.g. "spawn a fresh diag-bot and move east twice", "verify abilities list after equipping a sword").'
---

# UnicornHack Diagnostic Client

A headless MCP server that connects to a running `UnicornHack.Web` instance, mirrors the player's game state, and exposes:

- **Read tools** that return compact text snapshots of the current state (status line, ASCII map, actor/item/ability tables, log-since-tick).
- **Action tools** that submit every `ActorActionType` to the server and wait for the resulting per-turn change sets before returning.
- **Dialog/description tools** that wrap `ShowDialog`, `QueryStaticDescription`, and `SendMessage`.

The client maintains the canonical state itself by applying the initial `GetState` snapshot and subsequent `ReceiveChangeSets` messages, so every tool reads from one consistent mirror.

Source: [test/UnicornHack.DiagnosticClient/](../../../UnicornHack/test/UnicornHack.DiagnosticClient/)

## Prerequisites

1. **SQL Server LocalDB** must be installed and reachable at `(localdb)\mssqllocaldb`. The connection string is in [src/UnicornHack.Web/appsettings.Development.json](../../../UnicornHack/src/UnicornHack.Web/appsettings.Development.json).
2. The ASP.NET Core HTTPS dev certificate must be trusted (or pass `--insecure` to the diagnostic client). One-time setup:
   ```
   dotnet dev-certs https --trust
   ```
3. **The `UnicornHack.Web` server must be running** locally (default `https://localhost:7251`) — see *Running the Web server* below.

## Running the Web server

The Web project targets a release `Microsoft.NETCore.App` 10.0 framework. Use the **system .NET install** (`C:\Program Files\dotnet` on Windows)

In a clean PowerShell session (NOT a Developer Command Prompt rooted in another repo):

```powershell
cd D:\repos\UnicornHack\UnicornHack
$env:DOTNET_ROOT = 'C:\Program Files\dotnet'
dotnet run --project src/UnicornHack.Web/UnicornHack.Web.csproj --launch-profile UnicornHack.Web
```

The `--launch-profile` defaults are in [src/UnicornHack.Web/Properties/launchSettings.json](../../../UnicornHack/src/UnicornHack.Web/Properties/launchSettings.json) — HTTPS on 7251, HTTP on 5251, `ASPNETCORE_ENVIRONMENT=Development`.

To run it detached (so subsequent shell commands aren't blocked):

```powershell
$env:DOTNET_ROOT = 'C:\Program Files\dotnet'
Start-Process -FilePath 'C:\Program Files\dotnet\dotnet.exe' `
  -ArgumentList @('run','--project','D:\repos\UnicornHack\UnicornHack\src\UnicornHack.Web\UnicornHack.Web.csproj','--launch-profile','UnicornHack.Web') `
  -WorkingDirectory 'D:\repos\UnicornHack\UnicornHack' `
  -RedirectStandardOutput 'D:\repos\UnicornHack\UnicornHack\web.out.log' `
  -RedirectStandardError  'D:\repos\UnicornHack\UnicornHack\web.err.log'
# Tail logs to see startup messages
Get-Content D:\repos\UnicornHack\UnicornHack\web.out.log -Wait
```

Wait for `Now listening on: https://localhost:7251` in the log before connecting clients. The Razor home page (`/`) currently throws `No service for type 'Microsoft.ApplicationInsights.AspNetCore.JavaScriptSnippet' has been registered` — this is harmless to the SignalR hub (`/gameHub`), which is what the diagnostic client uses.

### Debug / development server options

The server reads four optional configuration keys at startup. They are **CLI parameters** routed through ASP.NET Core's default `AddCommandLine` provider — there is no `appsettings.json` block for them. Three are bound to `UnicornHack.GameOptions` (which is then persisted with the saved game); the fourth, `ResetDatabase`, is a one-shot startup flag read directly by [Program.cs](../../../UnicornHack/src/UnicornHack.Web/Program.cs).

| Flag | Type | Effect |
|---|---|---|
| `--InitialSeed=<uint>` | uint | Pins the random seed for every newly-created character; same seed ⇒ identical map / items / monsters. Omit the flag entirely for a cryptographically-random seed per character (is specify seed 0). |
| `--RevealLevels=true` | bool | Fully reveals every level (only use to pinpoint found issues). |
| `--RevealRooms=true` | bool | Fully reveals each room (only use to pinpoint found issues). |
| `--ResetDatabase=true` | bool | Runs `DatabaseCleaner` at startup, dropping and recreating the schema. |

Examples:

```powershell
# Deterministic dev session that resets the DB:
dotnet run --project src/UnicornHack.Web -- --InitialSeed=1 --ResetDatabase=true

# Production-shaped run (random seed, no resets):
dotnet run --project src/UnicornHack.Web

# Same via environment variables (when CLI args are awkward, e.g. systemd):
$env:InitialSeed=1; dotnet run --project src/UnicornHack.Web
```

The repo's [launchSettings.json](../../../UnicornHack/src/UnicornHack.Web/Properties/launchSettings.json) ships with no `commandLineArgs` defaults, so the `UnicornHack.Web` launch profile gives you a production-like run (random seed, no reveal, no reset). Append the flags above on the CLI.

## Launch

### From VS Code (Copilot MCP)

The repo ships a workspace MCP server definition at [UnicornHack/.vscode/mcp.json](../../../UnicornHack/.vscode/mcp.json). VS Code auto-discovers it. To use it:

1. Make sure the `UnicornHack.Web` server is running (see *Prerequisites*).
2. Open the Chat panel → tools picker → enable **`unicornhack-diagnostic`** (or `MCP: List Servers` → Start).
3. VS Code will prompt for `playerName` and `serverUrl` the first time; defaults are `diag-bot` and `https://localhost:7251`.

The server starts via `dotnet run --project test/UnicornHack.DiagnosticClient` with `--insecure` and a log file under that project's `bin/` folder. Once started, all read/action/dialog tools listed below are available to Copilot.

### From the command line

```
dotnet run --project UnicornHack/test/UnicornHack.DiagnosticClient -- --name <character> [--server <url>] [--insecure] [--log-file <path>] [--verbose] [--action-timeout-ms 30000]
```

- `--name` (required): the in-game character name. A new character is auto-created on first connect.
- `--server`: base URL, default `https://localhost:7251`. The hub path `/gameHub` is appended.
- `--insecure`: trust any TLS cert. Local dev only — never against a remote server.
- `--log-file`: append diagnostic logs here (stdio is reserved for MCP JSON-RPC; without this flag, logs go to stderr).
- `--verbose`: enable Debug-level logging.
- `--action-timeout-ms`: per-action wait for change sets, default 30 000 ms.

The process speaks MCP over stdio. To register it with VS Code (or another MCP client), point that client at the `dotnet run` invocation above.

## Tool Inventory

### Read tools (cheap, no server round-trip)

| Tool | Returns |
|---|---|
| `get_status` | One-line summary: name, id, tick, can_act, hp/maxhp, ep/maxep, xp, branch, depth, room size. **Call first every interaction.** |
| `get_map(include_legend=false)` | ASCII grid of the current room overlaid with player (`@`), actors (uppercase visible, lowercase remembered), items (`$`), connections (`>`/`<`). |
| `get_visible_entities(include_remembered=true)` | TSV table of actors in the room with hp, heading, next-action tick. |
| `get_items` | TSV table of items in the room. |
| `get_abilities(only_slotted=false)` | TSV table of the player's abilities (id, name, slot, activation, cooldowns, targeting shape:size). |
| `get_races` | Player races/species components. |
| `get_connections` | Stairs/doors in the room. |
| `get_log(since_tick=-1)` | Log entries with `Ticks > since_tick`. **Use `since_tick` to bound output** — pass the tick recorded before your action. |
| `get_player_options` | Current choice/action and indexed option identifiers with enabled/disabled state. |
| `get_dialog` | Last `ReceiveUIRequest` payload (list-of-objects). `null` if none. |
| `get_chat` | Recent broadcast messages. |
| `get_diagnostics` | Hub state, last applied tick, collection counts. |

### Action tools (each submits to the hub and waits for the post-action change sets)

| Tool | Mapping |
|---|---|
| `wait` | `ActorActionType.Wait` |
| `move(direction)` | `MoveOneCell`. Direction enum: `0=E 1=NE 2=N 3=NW 4=W 5=SW 6=S 7=SE`. |
| `change_heading(direction)` | `ChangeHeading` (turn in place). |
| `move_to(x, y)` | `MoveToCell`. (x, y) are byte tile coords in the current room; the server pathfinds. |
| `pickup(item_id)` | `PickupItem` from the current tile. |
| `drop(item_id)` | `DropItem`. |
| `equip(item_id, slot)` | `EquipItem`; `slot` is the `EquipmentSlot` int. |
| `unequip(item_id)` | `UnequipItem`. |
| `set_ability_slot(slot, ability_id)` | `SetAbilitySlot`. Pass `ability_id=null` to clear. |
| `use_ability_slot(slot, target_id?, target2?)` | `UseAbilitySlot`. Optional targeting depends on the ability's shape. |
| `travel_to_room(room_id)` | `TravelToRoom`. |
| `select_player_option(option_index)` | Select an indexed entry from the current stage shown by `get_player_options`. |
| `perform_action(action_type, target?, target2?)` | Raw escape hatch for any `ActorActionType` int. |

Each action tool returns a short status string like:
```
action=MoveOneCell target=0 target2= tick=124 can_act=True hp=18/20 ep=10/10
```

### Dialog / description tools

| Tool | Mapping |
|---|---|
| `show_dialog(query_type, arguments=[])` | `ShowDialog`. Waits for its matching `ReceiveUIRequest` and returns the payload; `get_dialog` retains the last response. `query_type` values: `0=Clear 1=SlottableAbilities 2=PlayerAttributes 3=PlayerInventory 4=PlayerAdaptations 5=PlayerSkills 6=AbilityAttributes 7=ActorAttributes 8=ItemAttributes 9=StaticDescription 10=PostGameStatistics 11=LevelMap`. |
| `describe(topic_id, category)` | `QueryStaticDescription`. `category` is `DescriptionCategory`. |
| `send_message(text)` | `SendMessage`. **Currently fails server-side** because the server reads `Context.User.Identity.Name` and authentication is not wired up. |

## Recommended Interaction Loop

```
1. get_status                       # 1 line — knows whether you can act
2. get_map                          # ASCII room
3. get_visible_entities             # who else is here, where, how dangerous
4. <act>: move(direction) | use_ability_slot(slot, ...) | ...
5. tick_before = result.tick from step 1
6. get_log(since_tick=tick_before)  # what happened during your action
7. repeat from 1
```

## Token-Budget Tips

- Prefer `get_status` + `get_map` + `get_log(since_tick=...)` over a full state dump.
- `get_log` is append-only on the server, so passing a tight `since_tick` keeps output small.
- Avoid asking for full ability/item dumps every turn unless something changed.

## Wire Format Notes

- Movement direction follows `UnicornHack.Primitives.Direction`: `East=0 Northeast=1 North=2 Northwest=3 West=4 Southwest=5 South=6 Southeast=7`. `Up=8`/`Down=9` are reported as "unable to move" by the server in `MoveOneCell`; use `travel_to_room` or step onto a stairway tile and let the server handle level transitions via change sets.
- `move_to` packs the cell as `(x << 8) | y` server-side (`Point.ToInt32()`); the client tool does this for you.
- The diagnostic client reuses the server's own `*Change` DTOs (via `ProjectReference` to `UnicornHack.Web`) together with the server's `MessagePack.Resolvers.GeneratedResolver`. The hand-written formatters under `src/UnicornHack.Web/Hubs/MessagePack/` only invoke a property setter when its `ChangedProperties` bit is set on the wire, so the setter side effect that re-sets the same bit is always a no-op. The canonical state instance keeps `ChangedProperties = null` so writes against it are also no-ops via the `?.` guard.

## Troubleshooting

- **TLS / dev-cert errors**: pass `--insecure` (local only). Alternatively `dotnet dev-certs https --trust`.
- **`Hub is not connected` / `Disconnected`**: the bootstrap connection failed; check the log file and confirm the server URL/port match the `UnicornHack.Web` launch profile. After the first failed connect the diagnostic client retries forever in the background — call any action tool again and it will lazily reconnect via `EnsureReadyAsync`.
- **`You must install or update .NET to run this application`** when launching the Web server: the apphost is resolving the runtime from a side-by-side install (e.g. `D:\repos\EFCore\.dotnet`) whose preview build doesn't satisfy `Microsoft.NETCore.App 10.0.0`. Set `$env:DOTNET_ROOT = 'C:\Program Files\dotnet'` in a fresh shell before `dotnet run` — see *Running the Web server*.
- **Action returns but state looks unchanged**: action may have been rejected (e.g. moving into a wall). Check `get_log(since_tick=...)` for the explanatory entry.
- **`send_message` throws server-side NullReferenceException**: expected — the server requires authentication. Tracked as a known limitation.

## Adding a New Tool

1. Add a method to one of `Tools/ReadTools.cs`, `Tools/ActionTools.cs`, or `Tools/DialogTools.cs`.
2. Decorate with `[McpServerTool, Description("…")]`.
3. For action tools, route through `_conn.PerformActionAsync(...)` and `_turnTracker.WaitAsync(...)` so the response reflects post-turn state.
4. Tools are picked up automatically by `WithToolsFromAssembly()` in `Program.cs`.

## When the Wire Format Changes

If the server appends a `[Key(n)]` to a `*Change` DTO:
1. Add the new key on the server DTO and to its hand-written formatter under `src/UnicornHack.Web/Hubs/MessagePack/`. The diagnostic client picks up the new field automatically via the `ProjectReference`.
2. Add a `case n:` arm to the corresponding `MergeX` switch in `State/StateApplier.cs` so the new field is applied onto the canonical state.
3. Expose the new field through the relevant read tool if useful for agents.
