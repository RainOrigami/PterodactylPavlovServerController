# Pterodactyl Pavlov VR Server Controller

ASP.net web API (.net 6) for controlling aspects of Pavlov VR servers in Pterodactyl, highly specific to the use case of DE-Community "Hearthstrike" Pavlov VR servers

# Features

Update map rotation from google drive sheets.

# Prerequisites

Pterodactyl with a Pavlov VR egg.

A google sheets sheet with a very specific format and optionally a macro:
- A1: URL
- B1: Page Title
- C1: `=ARRAYFORMULA(IF(ROW(A:A)=1, "Map Name", IF(ISBLANK(A:A), "", right(B:B, len(B:B)-find("::", B:B)-1))))`
- D1: Game Mode
- B2:B1000: `=IF(A2="","",IMPORTXML(A2,"//title"))` (formula for B2, drag down) - Sheets does not support importxml in arrayformula, those fuckers!
- Recommended but not required: A second sheet containing all valid game mode codes to data validate D1:D1000 with that list
- Optional: Macro, tied to a button, to update the maps in pterodactyl from within the google sheet (replace `the.host.of.this.app:port`, `yourpavlovserveridhere` and `yourapikeyhere` with your corresponding values):
  ```
	function updateMaps() {
		SpreadsheetApp.getActive().toast("Sending...");
		var result = UrlFetchApp.fetch("https://the.host.of.this.app:port/maps/update?spreadsheetId="+SpreadsheetApp.getActiveSpreadsheet().getId()+"&tabName=Maps&serverId=yourpavlovserveridhere", {"method": "post", "muteHttpExceptions": true, "validateHttpsCertificates": false, "headers" : { "x-api-key" : "yourapikeyhere" }});
		if (result.getResponseCode() != 200) {
			SpreadsheetApp.getUi().alert("Error while uploading, check your data!");
			return;
		}

		SpreadsheetApp.getUi().alert("Maps have been updated successfully.");
	}
  ```
- You! Yes You! You could change the colums! The only two required columns for this tool to do its job are URL, to extract the map ID, and game mode. But you will have to change Models/MapRowModel.cs to reflect your header.

# Usage

The authentication is weak, specifically you will create a new random api key and put it in appsettings apikey.
Provide this key in the x-app-key header of any request that you do against this api.

Generate a google sheets api service account credential in your google cloud console. Create and download a private json key.
The service account email goes in appsettings google_serviceaccountemail, the downloaded json file is put in a directory that is readable by the application and the path of the file is put into appsettings google_jsoncredentialpath.

In pterodactyl, create an api credential for your user (account -> api credentials, not admin -> application api) and put the pterodactyl base url in appsettings pterodactyl_baseurl and the api key in pterodactyl_bearertoken.

Lastly, specify the path to the Game.ini inside the Pavlov VR egg in appsettings pavlov_gameinipath.

# End user usage

The end user now only has to open your google sheet, add or remove urls of pavlov vr workshop maps, set their game mode and press the button. Easy as pie!

# Future releases

I hope to fork a Pavlov VR egg and integrate this somehow in the server settings so that no external tool is necessary and it can all be done from within pterodactyl with some bash magic, or whatever.

Also, this tool is supposed to someday generate player statistics (total kills, deaths and such) by using the pterodactyl console websocket. I'll probably do this first.