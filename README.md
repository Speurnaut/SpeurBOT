# SpeurBOT

SpeurBOT is a Twitch chat bot with OBS integration. It can connect to Twitch chat to read and send messages, interact with OBS Studio via websockets to allow users to interact with the stream, and features a Text-To-Speech engine.

## Setup

Clone the repository to your local machine. You will need Visual Studio, or something similar, to compile and run the bot.

## Configuration

You will need to enter the following details into the appsettings.json file to properly configure the bot for use.

### Twitch

The bot does not have its own Twitch account. Either register a new account for the bot, or allow it to connect using your own account.

#### Nick

The username of the account the bot will connect as.

#### Pass

An OAuth token for your bot to authenticate with. Go to https://twitchapps.com/tmi/ and sign in with your bot's credentials to recieve an OAuth token.

#### Channel

The name of the channel the bot should connect to when it runs. This will be a Twitch username.

### OBS

The bot integrates with OBS via the OBS WebSocket server. In OBS, go to Tools > WebSocket Server Settings. Here you can enable the WebSocket server, and click on Connect Info to see the necessary connection details.

#### IP

The OBS WebSocket Server IP address.

#### Port

The OBS WebSocket Server port.

#### Password

The OBS Websocket Server password.

#### Source Group

The name of the OBS group in which you have placed your bot's sources.

If you want to group the sources your bot interacts with, you **must** place all of them into one group, and enter that group's name into the configuration file.

If you do not want to group your sources, then they must **all** be at the root level of your OBS scenes. You cannot have some in the group and some not. If you are leaving your sources at the root level, leave this setting blank.

#### TTS File Path

The bot can display Text-To-Speech messages onscreen in OBS, by writing messages to a local HTML file which is used as an OBS Browser Source. Please specify where you would like this file to be stored.

## Usage

Either run the bot from your IDE, or compile an .exe and run it. The bot will automatically attempt to connect to Twitch chat and OBS using the details you have entered. Check the console window to see if the bot was able to successfully connect. If the connection was succesful, you can enter the built-in commands into the Twitch Channel that your bot is connected to in order to test if it is working.

## Commands

`!status` - the bot will report the status of each major component. It will post to Twitch Chat, confirming that it is connected to Twitch Chat and to the OBS WebSocket Server, and the Text-To-Speech Engine will make an audio announcement.

`!tts` - the bot will read your message aloud using the TTS Engine. It will also write the message to a local HTML file at the path specified in the configuration, which can be added to OBS as a browser source to allow you to display messages onscreen. For this to work you will need to add your local HTML file into OBS as a Browser Source called "TTS".

`!alarm` - this will activate a full-screen video overlay in OBS. You can optionally provide a message to be read after the alarm has sounded. In order for this to work you will need to create a media source called "Alarm".