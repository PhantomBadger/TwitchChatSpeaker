# TwitchChatSpeaker
A quick-and-simple application for speaking twitch chat messages via TTS

# Configuration guide
### TwitchOAuthKey - `String`
> The OAuth2 key to use to connect to the Twitch chat.
### TwitchChannelName - `String`
> The stream channel chat to connect to for reading messages.
### TwitchChannelId - `String`
> The channel ID of the streamer, used for 7TV emote grabbing.
### MaxMessageLength - `Whole number`
> The maximum length a message can be for it to be read out.
### LimitEmojis - `Boolean (true/false)`
> Whether messages should go through emoji checks before being read out.
### MaximumEmojiLimit - `Whole number`
> The maximum amount of 7TV emotes that can be sent **before** we begin percentage-based calculations.
### EmojiPercentageLimit - `Number`
> A number between 0 and 100. If a message has more percent of 7TV emojis than this number, the message is not selected for readout.
### PrioritiseUniqueMessages - `Boolean (true/false)`
> Whether duplicate messages will be avoided in favour of unique messages as much as possible.
### AttemptsBeforeFailingUnique - `Whole number`
> How many times a duplicate message will be ignored before we give up on a unique message and say the duplicate.
### FilterMessages - `Boolean (true/false)`
> Whether messages should go through filter checks before being read out.
### FilteredWords - `List of Strings`
> A list of words that, if present in a message, will not be selected for readout.
