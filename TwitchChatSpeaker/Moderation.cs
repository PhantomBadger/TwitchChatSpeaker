using Logging;
using Logging.API;
using Settings;
using TwitchChatSpeaker.Emojis;

namespace TwitchChatSpeaker;

public static class Moderation
{
    public static bool FilterCheck(string message, UserSettings settings)
    {
        var trip = false;
        foreach (var word in settings.FilteredWords)
        {
            if (trip) continue;
            if (message.ToLower().Contains(word.ToLower())) trip = true;
        }

        return trip;
    }

    public static (bool, int, double?) EmojiCheck(string message, UserSettings settings, EmojiManager emojiManager)
    {
        ILogger logger = new ConsoleLogger();
        
        var totalEmoteCount = emojiManager.GetEmoteCount(message);
        if (totalEmoteCount <= settings.MaximumEmojiLimit) return (false, totalEmoteCount, null);
        
        var totalEmoteNameCount = emojiManager.GetTotalEmoteNameCount(message);
        var totalMessageCount = Double.Parse(message.Length.ToString());
        
        logger.Information(totalEmoteNameCount.ToString());
        logger.Information(totalMessageCount.ToString());

        var percentageOfEmotesToFullMessage = (totalEmoteNameCount / totalMessageCount) * 100;
        
        logger.Information(percentageOfEmotesToFullMessage.ToString());

        if (percentageOfEmotesToFullMessage > settings.EmojiPercentageLimit) return (true, totalEmoteCount, percentageOfEmotesToFullMessage);
        
        return (false, totalEmoteCount, percentageOfEmotesToFullMessage);
    }
}