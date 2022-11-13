# Pterodactyl Pavlov VR Server Controller

ASP.net web API (.net 6) for controlling aspects of Pavlov VR servers in Pterodactyl

This project is in ongoing development and WIP. Expect bugs. Only tested with SND servers so far!

Example of generated server, map, gun and player statistics: https://admin.bloodisgood.net/ppsc/stats/072fd1a7

Screenshot RCON tool:
![image](https://user-images.githubusercontent.com/51454971/201521103-1d8d53bc-abb1-479d-9327-893b17a4a759.png)

Screenshot Google map sheet:
![image](https://user-images.githubusercontent.com/51454971/201521477-8aa1d69b-dd15-424a-a288-4d3c7b91fdd8.png)

# Features

- **User management based on Pterodactyl users**  
  Accesses Pterodactyl user table for reading users    
  Uses Pterodactyl user API keys to interact with Pterodactyl    
- **Pavlov server control through Pterodactyl API**  
  Update server map list from a Google sheets document for easier management    
- **Pavlov server control through RCON**  
  Automatically fetch available servers and connection information from Pterodactyl    
  Automatically fetch map rotation from Pterodactyl    
  Fetch server name from Game.ini as fallback from Pterodactyl    
  Fetch banned players list from RCON for easy unbanning    
  Fetch map information from Steam workshop (map image, name) - no more dealing with UGC123456789  
  Fetch user information from Steam API (avatar, name, VAC bans, game bans)
  Persistent players, to ban or inspect when they have left the server
  Kicking, banning, team switch individual players
  Cheats coming soon (give cash, give item, give vehicle, etc...)
- **Pavlov statistics**
  Process Pavlov server logs into statistics
  
# Install

This tool requires multiple databases. Supported are sqlserver, mysql and sqlite. For the Pterodactyl DB only mysql is supported.

## appsettings.json
- `PavlovStats` - Database to store converted server logs containing statistics data for generating the server stats
- `PavlovServers` - Database to store persistent players
- `Pterodactyl` - Your existing Pterodactyl database (mysql)
- `PPSC` - PPSC user database stores users (without password!) and API keys for registration and login
- `db_type` - Specify the db type of the three databases `PavlovStats`, `PavlovServers` and `PPSC`. Possible values: `sqlserver`, `mysql`, `sqlite`.
- `google_serviceaccountemail` - Your Google service account email for interfacing with Google sheets API
- `google_jsoncredentialpath` - The path to your Google service account json credentials file
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

The three databases `PavlovStats`, `PavlovServers` and `PPSC` can probably all point to the same database, if you don't want to create three different DBs. Not 100% sure tho.

# Some notes

## Rcon
Once a server is visible to the `pterodactyl_apikey` it will be connected to through Pavlov RCON. That means, each server will have a persistent RCON connection from PPSC. PPSC will only ever open one connection, no matter how many users access the RCON page. This lets PPSC monitor the players and add them to the persistent players database. It also prevents Pavlov from being overloaded by too many concurrent RCON connections.

## Stats
Stats require you to set `bVerboseLogging=true` in Game.ini. This causes the log files to become very large but to contain a lot of detailed statistical information.  
Stats are not live, they are generated every eight hours. This is because they have to sift through a lot of data. If you have well-visited servers, generating stats can take from anywhere between 6 seconds to tens of minutes per server. Once generated, the stats are saved into stats/{serverid}.html and served through /stats/{serverid} and are always publically available, no authentication is required to read them.

## Google sheets update document
You can create a Google sheets document to manage the map rotation. Instead of having to handle this UGC123456789 crap, you can set it up so you only need the workshop url.

Resolve map name example: Map URL in A1, get page title in A2: `=IMPORTXML(A1,"//title")`, then get map title in A3: `=right(A2, len(A2)-find("::", A2)-1)`.

Your Google sheet must have exactly these four colums at the beginning: URL, PageTitle, MapName, GameMode

The macro to have PPSC read the maps from the sheet can be triggered by a drawing on the sheet which has a script assigned:

```
function updateMaps() {
  SpreadsheetApp.getActive().toast("Sending...");
  var result = UrlFetchApp.fetch("https://your.ppsc.com/api/maps/update?apikey=xxpterodactylapikeyxx&spreadsheetId=xxspreadsheetidxx&tabName=xxmapstabnamexx&serverId=xxserveridxx", {"method": "post", "muteHttpExceptions": true, "validateHttpsCertificates": false });
  if (result.getResponseCode() != 200) {
    SpreadsheetApp.getUi().alert("Error reading maps");
    return;
  }

  SpreadsheetApp.getUi().alert("Maps updated successfully. Restart the server to apply.");
}
```

Replace `xxpterodactylapikeyxx` with an API key of a user that has File Write permissions on the server. Here you can create a system user again. But note that everybody with access to the Google sheet can see this API key. Replace `xxspreadsheetidxx` with the spreadsheet id, `xxmapstabnamexx` with the name of the tab that contains the required map columns and `xxserveridxx` with the Pterodactyl server id of the target server.

This process will soon be replaced by a proper map rotation editor within PPSC so Google sheets is not required anymore.

## Registration and user management
To register as user in PPSC you need to use your Pterodactyl username and password and create and supply a Pterodactyl API key. Only your username and API key are stored within PPSC. This way password changes and forgotten password processes are handled by Pterodactyl and will apply to PPSC too. When logging in as a user, the password will always be checked against the Pterodactyl DB.

Only servers where the registered user is added as a user, or owner, will be visible in RCON.

A way to let the user change their registered API key will come soon.
