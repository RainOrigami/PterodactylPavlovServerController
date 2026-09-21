**Pterodactyl Pavlov Server Controller (PPSC)**  
A Blazor Server web application for managing Pavlov VR game servers hosted on Pterodactyl. Provides real-time RCON control, persistent player tracking, automated server management, map rotation editing, and statistics generation.  
***Work in progress.*** * Expect bugs. Primarily tested with SND and EFP servers.*  
**Live stats example:** [https://pavlov.bloodisgood.org/stats/hearth](https://pavlov.bloodisgood.org/stats/hearth "https://pavlov.bloodisgood.org/stats/hearth")  
**Features**  
**RCON Control**  
PPSC maintains a single persistent RCON connection per server regardless of how many users are logged in. This prevents Pavlov from being overloaded by concurrent connections and enables continuous player monitoring.  
**Player actions**  
- Kick / ban (permanent, 1h, 1d, 1w, 1mo, or custom duration) / unban  
- Switch team  
- Add / remove moderator  
- Slap (with configurable damage)  
- Set vitals: HP, armor, helmet  
- Revive  
- Godmode (toggle)  
- Visibility (toggle)  
- Ignite / detonate  
- Gag (mute)  
- Warp to target player  
- No-clip (toggle)  
- Movement speed multiplier  
- Gravity per player  
**Inventory & economy**  
- Give item (full item enum)  
- Give / set / clear cash  
- Give vehicle  
- Give team cash  
- Supply (equip full loadout)  
- Drop items  
- Disable item pickup (toggle)  
- Enable / disable buy menu  
**Server-wide settings**  
- Set limited ammo type  
- Global gravity multiplier  
- Global game speed multiplier  
- Enable / disable prone  
- Enable / disable fall damage  
- Toggle attachment mode  
- Toggle kill feedback  
- Toggle utility trails  
- Toggle voting  
- Toggle nametags  
- Set server PIN (reserved slots)  
**Map & match**  
- Switch map (map label + game mode)  
- Rotate map  
- Reset SND  
- Pause match (duration in seconds)  
- Clear empty vehicles  
**Bot / NPC management**  
- Add / remove bots (amount, optional team)  
- Spawn loot crates  
- Spawn / remove chickens  
- Spawn / remove zombies  
**TTT-specific**  
- Set / flush karma  
- End round  
- Pause / resume timer  
- Toggle always-enable skin menu  
**RCON Plus** (extended commands exposed in a separate UI tab)  
- Custom raw RCON command input  
- Get / give / remove in-game menu items  
- Notify individual players  
**Player Management**  
PPSC tracks every player that connects to each server persistently.  
**Player profile**  
- Steam avatar, name, and profile link  
- All previous aliases used on the server  
- Total playtime and last seen timestamp  
- Country flag and region name  
- Steam VAC ban count, game ban count, and days since last ban  
- Per-server notes / comments  
- Per-server ban status, reason, and scheduled unban date  
**Ban system**  
- Preset ban durations: permanent, 1 hour, 1 day, 1 week, 1 month  
- Custom ban duration (hours)  
- Free-text ban reason  
- Automatic unban at scheduled time  
**Offline players**  
- Paginated list (25 per page) of all players ever seen on a server  
- Sorted by last seen descending  
- Same ban / comment / profile actions as online players  
**Server Automation**  
All automation features are configured per-server through the UI. Settings are persisted to the database.  
**Warmup rounds** *(SND only)*  
   
 Automatically triggers a warmup round after each map change. Selects a random loadout from 11 presets, cycling through all before repeating:  
| | |  
|-|-|  
| **Loadout** | **Description** |   
| Grenade Only | Utility trails on, 0.75× gravity |   
| Newton Launcher | Fire launcher |   
| Speedy 50 Cal | Barrett M99 sniper |   
| Knife Only | Melee only |   
| John Wick | Dual pistols + 3 flashbangs, 2× movement speed |   
| Ballistic Shield | Shield + sidearm |   
| Flare Gun | Flare pistol |   
| Golden Gun | Golden pistol variant |   
| M1 Garand | 8-round semi-auto rifle |   
| RPG | Explosive launcher |   
| Tranquilizer | Non-lethal dart gun |   
   
Players are put in godmode during setup and auto-equipped on spawn. Godmode lifts 4 seconds after the round starts; suicide detection removes godmode from dead players immediately.  
**Reserved slots**  
   
 Configurable slot count and 4-digit PIN. Automatically locks the server when the player count reaches the reserved threshold. Players with the PIN can join beyond the visible player cap.  
**Ping kick**  
   
 Configurable ping threshold (ms) and measurement window (seconds). Players exceeding the threshold receive a warning notification and are kicked after a 5-second grace period. Exemption list accepts comma-separated SteamID64s.  
**Pause SND on inactivity** *(SND only)*  
   
 Automatically pauses the match when fewer than 2 players are online. Unpauses when players return. Prevents rounds from timing out on empty servers.  
**Kill skin**  
   
 Assigns a configurable skin to the player with the most kills once they reach a configurable kill threshold. Applied once per round, only during active rounds.  
**MOTD (Message of the Day)**  
   
 Displays a configurable message to each player on join. Shown for 20 seconds.  
**Half-time announcement** *(SND only)*  
   
 Sends a configurable message when the score reaches 9–9. Minimum 1-minute cooldown between announcements.  
**League mode**  
   
 Hides non-competitive UI elements and disables: cash display, cheat commands, limited ammo type controls, voting disable, custom RCON command input, and nametag controls.  
**Map Rotation Editor**  
- Load and apply saved map rotations  
- Save the current rotation under a named preset  
- Drag-and-drop reordering  
- Add maps (workshop UGC ID + game mode) or remove maps  
- Apply rotation to the live server immediately  
- Apply and restart server in one action  
- Export current rotation as a list of workshop URLs  
- Workshop map names and thumbnails resolved via mod.io (cached locally)  
- Supported game modes: SND, DM, TDM, EFP, CS, TTT, Custom  
**Statistics**  
Stats are generated from Pavlov server log files every 8 hours. Generation time ranges from a few seconds to tens of minutes depending on server traffic. Output is a static HTML file served at /stats/{serverId}, always publicly accessible without login.  
Requires bVerboseLogging=true in Game.ini.  
**Tracked per player**  
- Kills, deaths, assists, headshots, teamkills, suicides  
- Bombs planted, defused, exploded  
- Chickens killed  
- Experience / score  
- Cash earned (EFP)  
- Best weapon, best map/mode combination  
- Rounds played, win/loss ratio  
- Time on server, last seen date  
**Tracked per server**  
- Total kills, headshots, assists, teamkills  
- Total bomb plants, defuses, explosions  
- Unique players, unique maps, total matches and rounds  
**Tracked per map and per weapon**  
- Aggregate kills and player stats  
- 52+ weapons tracked individually (see full list in source)  
**Scoring system**  
| | |  
|-|-|  
| **Event** | **Points** |   
| Kill | +10 |   
| Headshot bonus | +5 |   
| Assist | +2 |   
| Death | −1 |   
| Teamkill | −40 |   
| Bomb plant | +50 |   
| Bomb defuse | +50 |   
   
Stats pages also include:  
- Steam VAC / ban status per player  
- Total time spent on server  
- Demo download links (if demos are stored)  
**Audit Log**  
Every server action taken through PPSC is logged with server ID, username (or SYSTEM / UNKNOWN), UTC timestamp, and a description. The last 100 entries per server are shown in the UI.  
**User & Access Management**  
- Registration requires a Pterodactyl username and a Pterodactyl API key  
- Only the username and API key are stored in PPSC — passwords are never copied  
- Login always validates the password against the live Pterodactyl database, so password changes in Pterodactyl apply to PPSC automatically  
- Each user can only see servers they are assigned to (as user or owner) in Pterodactyl  
- Password change is available through the account page  
**Installation**  
**Requirements**  
- .NET 7 runtime  
- MySQL / MariaDB  
- A running Pterodactyl panel with Wings  
- Pavlov VR dedicated server eggs managed by Wings  
**Databases**  
PPSC requires four separate databases:  
| | |  
|-|-|  
| **Connection string key** | **Purpose** |   
| PavlovStats | Parsed server log data for statistics |   
| PavlovServers | Persistent players, audit log, map rotations |   
| Pterodactyl | Read-only access to your existing Pterodactyl DB |   
| PPSC | PPSC user accounts and API keys |   
   
Migrations for PavlovStats, PavlovServers, and PPSC run automatically on startup. Pterodactyl is read-only and must already exist.  
**appsettings.json**  
{  
   "ConnectionStrings": {  
     "PavlovStats": "server=...;database=pavlovstats;...",  
     "PavlovServers": "server=...;database=pavlovservers;...",  
     "Pterodactyl": "server=...;database=pterodactyl;...",  
     "PPSC": "server=...;database=ppsc;..."  
   },  
   
   // Pterodactyl panel base URL (no trailing slash)  
   "pterodactyl_baseurl": "https://panel.example.com",  
   
   // API key for RCON access — assign this system account to every server  
   // that should be accessible in PPSC  
   // Required permissions: File Read, File Read-Content, File Update, Startup Read  
   "pterodactyl_apikey": "ptla_...",  
   
   // Separate API key for statistics — assign to servers that should have stats generated  
   // Required permissions: File Create, File Read, File Read-Content, File Delete  
   "pterodactyl_stats_apikey": "ptla_...",  
   
   // Path to Game.ini inside the Pavlov egg container  
   // Default for the parkervcp egg: /Pavlov/Saved/Config/LinuxServer/Game.ini  
   "pavlov_gameinipath": "/Pavlov/Saved/Config/LinuxServer/Game.ini",  
   
   // Path where Pavlov demo files are stored (for stats demo downloads)  
   "pavlov_demospath": "/Pavlov/Saved/Demos",  
   
   // Steam Web API key — for player avatars, names, VAC/game ban lookup  
   "steam_apikey": "...",  
   
   // mod.io API key — for workshop map names and thumbnails  
   "modio_apikey": "...",  
   
   // Local cache file paths (JSON)  
   "mapscache": "mapscache.json",  
   "steam_summarycache": "steam_summarycache.json",  
   "steam_bancache": "steam_bancache.json",  
   
   // Base path if running behind a reverse proxy at a subpath (e.g. "/ppsc")  
   "basePath": ""  
 }  
   
**Pterodactyl API accounts**  
It is recommended to create two dedicated system accounts in Pterodactyl:  
- **RCON account** — assign to servers that should appear in the PPSC server list. Required permissions: File Read, File Read-Content, File Update, Startup Read.  
- **Stats account** — assign to servers that should have statistics generated. Required permissions: File Create, File Read, File Read-Content, File Delete.  
Keeping these separate lets you control independently which servers are controllable via RCON and which have statistics generated.  
**Stats prerequisite**  
Add the following to Game.ini on each server you want statistics for:  
[/Script/Pavlov.PavlovGameState]  
 bVerboseLogging=true  
   
Without this, log files will not contain the detail required for statistics.  
**Running**  
cd PterodactylPavlovServerController  
 dotnet run  
   
Or publish for production:  
dotnet publish -c Release  
   
**Architecture Notes**  
**RCON connections**  
PPSC opens exactly one RCON connection per server on startup, regardless of how many users are browsing the UI. The connection polls the server every second for server info and player list. This keeps player tracking up to date and prevents Pavlov from being overloaded by multiple concurrent RCON sessions.  
**Statistics generation**  
The stats engine (PavlovStatsReader) reads log files directly from the Pterodactyl file API and parses them into the PavlovStats database. Output is a self-contained static HTML file written back to the server via Pterodactyl and served by PPSC at /stats/{serverId}. Stats are regenerated every 8 hours. Public access requires no login.  
**Caching**  
Steam player profiles, VAC ban records, and mod.io map metadata are cached as local JSON files. mod.io requests are rate-limited to one per second.  
**Reverse proxy**  
Set basePath in appsettings.json if mounting PPSC at a subpath (e.g. behind nginx at /ppsc). Leave empty for root mounting.  
