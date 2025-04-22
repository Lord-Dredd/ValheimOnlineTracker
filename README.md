⚔️ Valheim Online Tracker
A lightweight server-side mod that posts a list of currently online players and other things to a configurable Discord webhook.

Perfect for private or public Valheim servers that want to track player activity in real-time through Discord.

🔧 Features
🟢 Posts a list of online players at a configurable interval (default: every 10 minutes)

📣 Customizable messages for server startup, shutdown, and player activity

📁 Automatically generates a config file in BepInEx/config/OnlineTracker/

🔒 Works fully server-side — no client install required

🛠️ Easy to install, update, and share

This MOD is basic at this level, what I have now posting is as follows.

-Server Start / Stop
-Player Login / Logoff
-Players currently online, player names and time spent online. Today - This week - total
-Posts hourly (can be changed) when no one is online, to know server is still alive.
-Two WebHook options (honestly i forgot to test the second URL option)
-There are also two Milestones posts, one for logins and one for time. 100 logs or 50 hours both have a post.

📂 Installation
Install BepInEx on your Valheim dedicated server.

Drop the plugin .dll into:
BepInEx/plugins/OnlineTracker/

Launch the server once to generate the config: BepInEx/config/OnlineTracker/ValheimOnlineTracker.cfg

Edit the config file to add your Discord webhook and customize messages, after first run, most if not all posting text and times can be changed.

Normally I would use DiscordConnector and in the past it worked brilliantly for me, but this time around I was not able to make the leaderboards work, so me and the AI decided to try our hand at making our own MOD. I have used ChatGpt4 AI for this MOD. AI all the way!

Join us over in The Lands of Dredd Discord and play Valheim with us on our dedicated server.


https://discord.gg/BwZtCveGru

Lord Dredd



