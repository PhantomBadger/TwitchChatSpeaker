using Logging;
using Logging.API;
using Settings;
using TwitchChatSpeaker.Emojis;
using TwitchLib.Client.Models;

namespace TwitchChatSpeaker;

public static class Moderation
{
    public static bool FilterCheck(string message, UserSettings settings)
    {
        var trip = false;
        foreach (var word in settings.FilteredWords)
        {
            if (trip)
            {
                continue;
            }
            if (message.ToLower().Contains(word.ToLower()))
            {
                trip = true;
            }
        }

        return trip;
    }

    public static EmojiCheckResult EmojiCheck(string message, EmoteSet twitchEmotes, UserSettings settings, EmojiManager emojiManager, ILogger logger)
    {        
        var totalEmoteCount = emojiManager.GetEmoteCount(message, twitchEmotes);
        if (totalEmoteCount <= settings.MaximumEmojiLimit)
        {
            return new EmojiCheckResult(false, totalEmoteCount, null);
        }
        
        var totalEmoteNameCount = emojiManager.GetTotalEmoteNameCount(message, twitchEmotes);
        var totalMessageCount = Double.Parse(message.Length.ToString());
        
        logger.Information(totalEmoteNameCount.ToString());
        logger.Information(totalMessageCount.ToString());

        var percentageOfEmotesToFullMessage = (totalEmoteNameCount / totalMessageCount) * 100;
        
        logger.Information(percentageOfEmotesToFullMessage.ToString());

        if (percentageOfEmotesToFullMessage > settings.EmojiPercentageLimit)
        {
            return new EmojiCheckResult(true, totalEmoteCount, percentageOfEmotesToFullMessage);
        }
        
        return new EmojiCheckResult(false, totalEmoteCount, percentageOfEmotesToFullMessage);
    }
}