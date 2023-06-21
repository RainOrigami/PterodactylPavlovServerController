# Pterodactyl Pavlov VR Server Controller

ASP.net web API (.net 7) for controlling aspects of Pavlov VR servers in Pterodactyl

This project is in ongoing development and WIP. Expect bugs. Only tested with SND and EFP servers so far!

Example of generated server, map, gun and player statistics: https://pavlov.bloodisgood.org/stats/hearth

Screenshots RCON tool:
![image](https://user-images.githubusercontent.com/51454971/207735949-8b683379-bfce-4e0d-a8ba-a9e38a59b5ca.png)
![image](https://user-images.githubusercontent.com/51454971/207370777-2a82d388-e559-4b3f-bd51-60cf268e6f56.png)
![image](https://user-images.githubusercontent.com/51454971/207370942-1db451c3-c9ba-4c65-ac33-eb622347433b.png)
![image](https://user-images.githubusercontent.com/51454971/207371400-bacc1678-c265-49dc-93d8-a50d97a7dacb.png)

Screenshot audit log:
![image](https://user-images.githubusercontent.com/51454971/207370120-c13f4af1-d1eb-46b7-92ea-b19e116fe4a7.png)

Screenshot map rotation editor:
![image](https://user-images.githubusercontent.com/51454971/207660927-0e74fd44-1b97-4d81-91ed-545c0cf7bfea.png)

# Features

- **User management based on Pterodactyl users**  
  Accesses Pterodactyl user table for reading users    
  Uses Pterodactyl user API keys to interact with Pterodactyl    
- **Pavlov server control through RCON**  
  Automatically fetch available servers and connection information from Pterodactyl    
  Automatically fetch map rotation from Pterodactyl  
  Fetch server name from Game.ini as fallback from Pterodactyl  
  Fetch banned players list from RCON for easy unbanning  
  Fetch map information from Steam workshop (map image, name) - no more dealing with UGC123456789  
  Fetch user information from Steam API (avatar, name, VAC bans, game bans)  
  Persistent players, to ban or inspect when they have left the server  
  Kicking, banning, team switch individual players  
  Cheats (give cash, give item, give vehicle, etc...)  
  Server control (ammo type, pin, name tags, mods, ...)  
  Measure player time on server and last seen  
  Ban reason and player comments  
  Time-based bans and automatic unbanning  
  Audit log
- **Pavlov server automation through RCON**  
  Reserved slots functionality  
  Kill skin (set player skin upon kill threshold, once per round)  
  Pause SND (resets SND while no players are online)  
  SND Warmup round with random loadouts  
  Kick players with too high average ping  
  Shift map rotation to always have the current map at the top
- **Map rotation editor**  
  Change map rotation  
  Save multiple map rotation presets
- **Pavlov statistics**  
  Process Pavlov server logs into statistics  
  Include VAC state  
  Include player time spent on server  
  Include last seen date and time  
  Works with SND and EFP
  
# Install

This tool requires multiple databases. Only mysql/mariadb is supported.

## appsettings.json
- `PavlovStats` - Database to store converted server logs containing statistics data for generating the server stats
- `PavlovServers` - Database to store persistent players, audit log and map rotations
- `Pterodactyl` - Your existing Pterodactyl database (mysql)
- `PPSC` - PPSC user database stores users (without password!) and API keys for registration and login
- `pterodactyl_baseurl` - The base URL of your Pterodactyl installation, used for API requests
- `pterodactyl_apikey` - The generic Pterodactyl API key - used for getting Pavlov servers for controling. Recommended to create a system account, use its API key and assign it to all Pavlov servers that should be accessible through PPSC (permissions: File Read, File Read-Content, File Update, Startup Read)
- `pterodactyl_stats_apikey` - The statistics Pterodactyl API key - used for getting Pavlov servers for creating statistics. Recommended to create a system account, use its API key and assign it to all Pavlov servers that should have a statistic generated (permissions: File Create, File Read, File Read-Content, File Delete)
- `pavlov_gameinipath` - The path of the Game.ini in your Pavlov egg (usually `/Pavlov/Saved/Config/LinuxServer/Game.ini` when using this egg: https://github.com/parkervcp/eggs/tree/master/game_eggs/steamcmd_servers/pavlov_vr)
- `steam_apikey` - Your Steam API key for fetching player information and bans
- `workshop_mapscache` - Path to your workshop maps cache file, used for caching workshop map data
- `steam_summarycache` - Path to your steam user summary cache file, used for caching steam user data
- `steam_bancache` - Path to your steam user bans cache file, used for caching steam user bans data
- `basePath` - Application base path, in case you use a reverse proxy

It is recommended to create two Pterodactyl system accounts, one for PPSC to access the server for rcon, and one for generating statistics. This way you or your users can control specifically which server should be accessible in PPSC rcon and which server should used for statistics generation. These are independent, you can generate stats for servers that are not accessible to PPSC rcon.

For upgrading it is important that `PavlovStats`, `PavlovServers` and `PPSC` are all seperate databases.

# Some notes

## Rcon
Once a server is visible to the `pterodactyl_apikey` it will be connected to through Pavlov RCON. That means, each server will have a persistent RCON connection from PPSC. PPSC will only ever open one connection, no matter how many users access the RCON page. This lets PPSC monitor the players and add them to the persistent players database. It also prevents Pavlov from being overloaded by too many concurrent RCON connections.

## Stats
Stats require you to set `bVerboseLogging=true` in Game.ini. This causes the log files to become very large but to contain a lot of detailed statistical information.  
Stats are not live, they are generated every eight hours. This is because they have to sift through a lot of data. If you have well-visited servers, generating stats can take from anywhere between 6 seconds to tens of minutes per server. Once generated, the stats are saved into stats/{serverid}.html and served through /stats/{serverid} and are always publically available, no authentication is required to read them.

## Registration and user management
To register as user in PPSC you need to use your Pterodactyl username and password and create and supply a Pterodactyl API key. Only your username and API key are stored within PPSC. This way password changes and forgotten password processes are handled by Pterodactyl and will apply to PPSC too. When logging in as a user, the password will always be checked against the Pterodactyl DB.

Only servers where the registered user is added as a user, or owner, will be visible in RCON.

A way to let the user change their registered API key will come soon.
