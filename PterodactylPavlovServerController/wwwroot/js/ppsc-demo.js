// Development helper: spawn fabricated player cards from the browser console
// to check PlayerCard formatting without needing a real player on the server.
//
//   ppsc.spawnPlayer()                                  default demo player
//   ppsc.spawnPlayer({ name: "Long Name", ping: 220 })  override any field
//   ppsc.spawnPlayer({ banned: true, vacBans: 2 })      ban / VAC formatting
//   ppsc.spawnPlayer({ online: false })                 offline-style card
//   ppsc.clearPlayers()                                 remove them all
//
// Fields: name, ping, score, cash, kills, deaths, assists, team, dead,
//         vacBans, gameBans, daysSinceLastBan, banned, banReason, online,
//         country, avatar, totalHours.
window.ppsc = {
    spawnPlayer: function (options) {
        return DotNet.invokeMethodAsync(
            "PterodactylPavlovServerController",
            "SpawnDemoPlayer",
            options ? JSON.stringify(options) : null
        ).then(function (message) {
            console.log("%c" + message, "color: #7D7");
            return message;
        });
    },
    clearPlayers: function () {
        return DotNet.invokeMethodAsync(
            "PterodactylPavlovServerController",
            "ClearDemoPlayers",
            null
        ).then(function (message) {
            console.log("%c" + message, "color: #FD7");
            return message;
        });
    },
    help: function () {
        console.log("ppsc.spawnPlayer({name, ping, score, cash, kills, deaths, assists, team, dead, vacBans, gameBans, daysSinceLastBan, banned, banReason, online, country, avatar, totalHours})");
        console.log("ppsc.clearPlayers()");
    }
};

// --- diagnostics -----------------------------------------------------------
//
//   ppsc.traceTabs()   log how long each tab takes from click to content
//   ppsc.transport()   report whether Blazor is on WebSocket or long-polling
//
// Server-side render time is logged in the journal. If the server says a few
// milliseconds but traceTabs says seconds, the time is going into transport or
// into the browser, not into the app.
window.ppsc.traceTabs = function () {
    var content = document.querySelector(".tab-content") || document.body;
    var clickedAt = null;
    var clickedTab = null;

    document.addEventListener("click", function (event) {
        var link = event.target.closest(".nav-link");
        if (link) {
            clickedAt = performance.now();
            clickedTab = link.textContent.trim();
            console.log("%c[tab] " + clickedTab + " clicked", "color: #7AF");
        }
    }, true);

    new MutationObserver(function () {
        if (clickedAt === null) {
            return;
        }

        var elapsed = Math.round(performance.now() - clickedAt);
        console.log("%c[tab] " + clickedTab + " content appeared after " + elapsed + "ms", "color: #7D7");
        clickedAt = null;
    }).observe(content, { childList: true, subtree: true });

    console.log("Tab tracing enabled. Click a tab.");
};

window.ppsc.transport = function () {
    // A WebSocket connection does not show up as a resource entry, whereas
    // long-polling produces a steady stream of _blazor requests.
    var polls = performance.getEntriesByType("resource").filter(function (entry) {
        return entry.name.indexOf("_blazor") !== -1;
    });

    console.log("_blazor HTTP requests seen: " + polls.length);
    if (polls.length > 5) {
        console.warn("Looks like long-polling, not WebSocket. Check the Apache proxy for this vhost: it needs mod_proxy_wstunnel and an Upgrade rule for the _blazor path.");
    } else {
        console.log("Looks like a WebSocket connection (few or no _blazor HTTP requests).");
    }

    console.log("Confirm in DevTools: Network tab, WS filter - there should be one long-lived _blazor connection.");
};
