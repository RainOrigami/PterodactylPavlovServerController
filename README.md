# Pterodactyl Pavlov VR Server Controller

ASP.net web API (.net 6) for controlling aspects of Pavlov VR servers in Pterodactyl, highly specific to the use case of DE-Community "Hearthstrike" Pavlov VR servers

Example of generated server, map, gun and player statistics: https://admin.bloodisgood.net/ppsc/api/stats?serverId=072fd1a7

Screenshot RCON tool:
![image](https://user-images.githubusercontent.com/51454971/199983104-7d4c243b-e89c-49be-9fbf-10cfdbbd007c.png)

Contains a web api for interfacing with pterodactyl, pavlov rcon, steamapi, google sheets.
Also contains a winforms pavlov rcon client that:
- fetches pavlov servers automatically from pterodactyl
- fetches ip/port/password automatically from pterodactyl
- fetches vac/game ban status from steam api
- fetches server bans and displays them
- fetches map rotation and map names automatically from Game.ini and steam workshop

# Usage

Oh boy... don't ask. Basically:

PterodactylPavlovServerController appsettings.json:
- apikey: a secret api key for accessing the api in general
- google_serviceaccountemail: the google sheets service account email
- google_jsoncredentialpath: the path to the google sheets service account json credentials file
- pterodactyl_baseurl: the pterodactyl base url
- pavlov_gameinipath: the path of the Game.ini
- workshop_mapscache: the path to the workshop maps cache file
- steam_apikey: your steam api key
- steam_summarycache: the path to the player summary cache file
- steam_bancache: the path to the player bans cache file

PterodactylPavlovRconClient PterodactylPavlovRconClient.dll.config:
- ppsc_basepath: the base url of PterodactylPavlovServerController
- ppsc_apikey: the secret api key for accessing the api in general
- ppsc_pterodactyl_key: your pterodactyl user api key

you'll figure the rest out. or not.
