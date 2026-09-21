# Pterodactyl Pavlov Server Controller

A Blazor Server web application for managing [Pavlov VR](https://pavlov-vr.com/) game servers hosted on [Pterodactyl](https://pterodactyl.io/). Real-time RCON control, persistent player tracking, server automation, map rotation editing and public statistics pages.

[![.NET](https://img.shields.io/badge/.NET-7.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-GPL--3.0-blue.svg)](LICENSE)

> **Work in progress.** Expect bugs. Primarily tested against SND and EFP servers.

**Live stats example:** <https://pavlov.bloodisgood.org/stats/hearth>

## Contents

- [Features](#features)
  - [RCON control](#rcon-control)
  - [Player management](#player-management)
  - [Server automation](#server-automation)
  - [Map rotation editor](#map-rotation-editor)
  - [Statistics](#statistics)
  - [Audit log](#audit-log)
  - [Users and access](#users-and-access)
- [Installation](#installation)
  - [Requirements](#requirements)
  - [Databases](#databases)
  - [Configuration](#configuration)
  - [Pterodactyl API accounts](#pterodactyl-api-accounts)
  - [Statistics prerequisite](#statistics-prerequisite)
  - [Running](#running)
- [Architecture notes](#architecture-notes)
- [License](#license)

## Features

### RCON control

PPSC maintains a single persistent RCON connection per server regardless of how many users are logged in. This keeps Pavlov from being overloaded by concurrent connections and allows continuous player monitoring.

<details>
<summary><b>Player actions</b></summary>

- Kick, ban (permanent, 1h, 1d, 1w, 1mo or custom) and unban
- Switch team
- Add and remove moderator
- Slap with configurable damage
- Set vitals: HP, armour, helmet
- Revive
- Godmode, visibility and no-clip toggles
- Ignite and detonate
- Gag (mute)
- Warp to target player
- Movement speed multiplier and per-player gravity

</details>

<details>
<summary><b>Inventory and economy</b></summary>

- Give item (full item enum)
- Give, set and clear cash
- Give vehicle
- Give team cash
- Supply a full loadout
- Drop items
- Disable item pickup
- Enable and disable the buy menu

</details>

<details>
<summary><b>Server-wide settings</b></summary>

- Limited ammo type
- Global gravity and game speed multipliers
- Prone and fall damage toggles
- Attachment mode, kill feedback and utility trails
- Voting and nametag toggles
- Server PIN for reserved slots

</details>

<details>
<summary><b>Map, match and NPCs</b></summary>

- Switch map (map label plus game mode), rotate map, reset SND
- Pause match for a given number of seconds
- Clear empty vehicles
- Add and remove bots, with optional team
- Spawn loot crates, chickens and zombies

</details>

<details>
<summary><b>TTT and RCON Plus</b></summary>

- TTT: set and flush karma, end round, pause and resume the timer, always-enable skin menu
- RCON Plus tab: raw command input, in-game menu items, per-player notifications

</details>

### Player management

Every player that connects to a server is tracked persistently.

- Steam avatar, name, profile link and all previous aliases used on the server
- Total playtime, last seen timestamp, country flag and region
- Steam VAC ban count, game ban count and days since the last ban
- Per-server notes and ban status, with reason and scheduled unban date
- Bans in preset durations (permanent, 1 hour, 1 day, 1 week, 1 month) or a custom number of hours, with automatic unban at the scheduled time
- Paginated offline player list, 25 per page, sorted by last seen, with the same actions as online players

### Server automation

Configured per server through the UI and persisted to the database.

**Warmup rounds** *(SND only)* — triggers a warmup round after each map change, drawing a random loadout from 11 presets and cycling through all of them before repeating:

| Loadout | Description |
| --- | --- |
| Grenade Only | Utility trails on, 0.75× gravity |
| Newton Launcher | Force launcher with low gravity |
| Speedy 50 Cal | Anti-materiel rifle at 2× game speed |
| Knife Only | Melee only |
| John Wick | Pistols and three flashbangs, 2× movement speed |
| Ballistic Shield | Shield and sidearm |
| Flare Gun | Flare pistol |
| Golden Gun | One-shot golden pistol |
| M1 Garand | Semi-auto rifle, mines and smoke |
| RPG | Explosive launcher |
| Tranquilizer | Dart gun |

Players are put in godmode during setup and equipped on spawn. Godmode lifts four seconds after the round starts, and suicide detection removes it from dead players immediately.

**Reserved slots** — configurable slot count and 4-digit PIN. Locks the server once the player count reaches the reserved threshold; players with the PIN can still join beyond the visible cap.

**Ping kick** — configurable threshold in milliseconds and measurement window in seconds. Players over the threshold are warned and kicked after a five-second grace period. The exemption list takes comma-separated SteamID64s.

**Pause SND on inactivity** *(SND only)* — pauses the match below two players and unpauses when they return, so rounds do not time out on an empty server.

**Kill skin** — assigns a configurable skin to the player with the most kills once they pass a kill threshold. Applied once per round, during active rounds only.

**MOTD** — shows a configurable message to each player on join, for 20 seconds.

**Half-time announcement** *(SND only)* — sends a configurable message at 9–9, with a minimum one-minute cooldown.

**League mode** — hides non-competitive UI and disables the cash display, cheat commands, limited ammo controls, voting controls, raw RCON input and nametag controls.

### Map rotation editor

- Load, save and apply named rotation presets
- Drag-and-drop reordering
- Add maps by workshop UGC ID and game mode, or remove them
- Apply to the live server, optionally restarting it in the same action
- Export the current rotation as a list of workshop URLs
- Map names and thumbnails resolved through mod.io and cached locally
- Game modes: SND, DM, TDM, EFP, CS, TTT and Custom

### Statistics

Generated from Pavlov log files every 8 hours and written out as a static HTML page served at `/stats/{serverId}`, always public and requiring no login. Generation takes anywhere from a few seconds to tens of minutes depending on how much traffic the server sees. Requires `bVerboseLogging=true`, see [Statistics prerequisite](#statistics-prerequisite).

**Per player** — kills, deaths, assists, headshots, teamkills, suicides, bombs planted, defused and exploded, chickens killed, experience, cash earned (EFP), best weapon, best map and mode, rounds played, win/loss ratio, time on server and last seen.

**Per server** — total kills, headshots, assists and teamkills, total bomb plants, defuses and explosions, unique players, unique maps, matches and rounds.

**Per map and weapon** — aggregate kills and player stats, with 52+ weapons tracked individually.

Scoring:

| Event | Points |
| --- | ---: |
| Kill | +10 |
| Headshot bonus | +5 |
| Assist | +2 |
| Death | −1 |
| Teamkill | −40 |
| Bomb plant | +50 |
| Bomb defuse | +50 |

Stats pages also carry Steam VAC and ban status per player, total time on the server and demo download links where demos are stored.

### Audit log

Every action taken through PPSC is logged with the server ID, the username (or `SYSTEM` / `UNKNOWN`), a UTC timestamp and a description. The most recent 100 entries per server are shown in the UI.

### Users and access

- Registration takes a Pterodactyl username and API key
- Only the username and API key are stored — passwords are never copied
- Login validates the password against the live Pterodactyl database, so password changes there apply immediately
- Users only see the servers they are assigned to in Pterodactyl, as user or owner
- Passwords can be changed from the account page

## Installation

### Requirements

- .NET 7 runtime
- MySQL or MariaDB
- A running Pterodactyl panel with Wings
- Pavlov VR dedicated server eggs managed by Wings

### Databases

PPSC uses four databases:

| Connection string | Purpose |
| --- | --- |
| `PavlovStats` | Parsed log data for statistics |
| `PavlovServers` | Persistent players, audit log, map rotations |
| `Pterodactyl` | Read-only access to your existing Pterodactyl database |
| `PPSC` | PPSC user accounts and API keys |

Migrations for `PavlovStats`, `PavlovServers` and `PPSC` run automatically on startup. `Pterodactyl` is read-only and must already exist.

### Configuration

`appsettings.json`:

```jsonc
{
  "ConnectionStrings": {
    "PavlovStats": "server=...;database=pavlovstats;...",
    "PavlovServers": "server=...;database=pavlovservers;...",
    "Pterodactyl": "server=...;database=pterodactyl;...",
    "PPSC": "server=...;database=ppsc;..."
  },

  // Pterodactyl panel base URL, no trailing slash
  "pterodactyl_baseurl": "https://panel.example.com",

  // API key used for RCON access
  "pterodactyl_apikey": "ptla_...",

  // Separate API key used for statistics generation
  "pterodactyl_stats_apikey": "ptla_...",

  // Paths inside the Pavlov egg container
  "pavlov_gameinipath": "/Pavlov/Saved/Config/LinuxServer/Game.ini",
  "pavlov_demospath": "/Pavlov/Saved/Demos",

  // Steam Web API key, for avatars, names and VAC/game ban lookups
  "steam_apikey": "...",

  // mod.io API key, for workshop map names and thumbnails
  "modio_apikey": "...",

  // Local JSON cache files
  "mapscache": "mapscache.json",
  "steam_summarycache": "steam_summarycache.json",
  "steam_bancache": "steam_bancache.json",

  // Base path when running behind a reverse proxy at a subpath, e.g. "/ppsc"
  "basePath": "",

  // Write all RCON traffic to a log file
  "logRconToFile": false,

  // Minimum gap between RCON commands sent to the same server, in
  // milliseconds. Pavlov drops commands that arrive back to back.
  "rcon_command_interval_ms": 60
}
```

### Pterodactyl API accounts

Create two dedicated system accounts in Pterodactyl:

| Account | Assign to | Required permissions |
| --- | --- | --- |
| RCON | Servers that should appear in the PPSC server list | File Read, File Read-Content, File Update, Startup Read |
| Stats | Servers that should have statistics generated | File Create, File Read, File Read-Content, File Delete |

Keeping them separate lets you decide independently which servers are controllable through RCON and which produce statistics.

### Statistics prerequisite

Add this to `Game.ini` on every server you want statistics for:

```ini
[/Script/Pavlov.PavlovGameState]
bVerboseLogging=true
```

Without it the log files do not contain the detail the stats engine needs.

### Running

```bash
cd PterodactylPavlovServerController
dotnet run
```

For production:

```bash
dotnet publish -c Release
```

## Architecture notes

**RCON connections.** Exactly one connection is opened per server at startup, no matter how many users are browsing. It polls for server info and the player list once per second, which keeps player tracking current and avoids overloading Pavlov with concurrent sessions. Commands are paced and serialised per server, because Pavlov silently drops commands that arrive too quickly.

**Statistics generation.** `PavlovStatsReader` reads log files through the Pterodactyl file API and parses them into the `PavlovStats` database. The result is a self-contained static HTML file, written back through Pterodactyl and served at `/stats/{serverId}`, regenerated every 8 hours.

**Caching.** Steam profiles, VAC ban records and mod.io map metadata are cached as local JSON files. mod.io requests are rate-limited to one per second.

**Reverse proxy.** Set `basePath` when mounting PPSC at a subpath, for example behind nginx at `/ppsc`. Leave it empty when serving from the root.

## License

[GPL-3.0](LICENSE)
