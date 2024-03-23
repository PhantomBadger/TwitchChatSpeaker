# Download the latest release [here](https://github.com/PhantomBadger/TwitchChatSpeaker/releases/tag/v1.0)

# TwitchChatSpeaker
A quick-and-simple application for speaking twitch chat messages via TTS

# Configuration guide
This is a simple guide for the configuration file, `TwitchChatSpeaker.settings.json`.
## Twitch settings
### TwitchOAuthKey - `String`
> The OAuth2 key to use to connect to the Twitch chat.
### TwitchChannelName - `String`
> The stream channel chat to connect to for reading messages.
### TwitchChannelId - `String`
> The channel ID of the streamer, used for 7TV emote grabbing.
## General settings
General misc settings without any category.
### MaxMessageLength - `Whole number`
> The maximum length a message can be for it to be read out.
# Emoji limiting
Emoji limiting allows you to limit the amount of 7TV emoji spam that is read out loud.
### LimitEmojis - `Boolean (true/false)`
> Whether messages should go through emoji checks before being read out.
### MaximumEmojiLimit - `Whole number`
> The maximum amount of 7TV emotes that can be sent **before** we begin percentage-based calculations.
### EmojiPercentageLimit - `Number`
> A number between 0 and 100. If a message has more percent of 7TV emojis than this number, the message is not selected for readout.
## Unique message prioritisation
Unique message priotitisation is also another tool to limit the amount of spam that is read out loud.<br>
It will keep track of the last message it read out and attempt to ignore messages which are equal to the previous message.<br>
This will only occur for a configurable amount of tries however before reading the duplicated message again.
### PrioritiseUniqueMessages - `Boolean (true/false)`
> Whether duplicate messages will be avoided in favour of unique messages as much as possible.
### AttemptsBeforeFailingUnique - `Whole number`
> How many times a duplicate message will be ignored before we give up on a unique message and say the duplicate.
## Filter
The filter allows you to prevent certain words from being sent, for example, a word you don't want read out or a 7TV emoji that has just too long of a name for your liking.<br>
If a message contains a filtered word, it will not be read out and a different message will be selected to be read out instead.
### FilterMessages - `Boolean (true/false)`
> Whether messages should go through filter checks before being read out.
### FilteredWords - `List of Strings`
> A list of words that, if present in a message, will not be selected for readout.
