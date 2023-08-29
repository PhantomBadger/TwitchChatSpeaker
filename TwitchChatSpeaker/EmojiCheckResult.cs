using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchChatSpeaker
{
    /// <summary>
    /// Aggregate class containing results from an <see cref="Moderation.EmojiCheck(string, Settings.UserSettings, Emojis.EmojiManager)"/> call
    /// </summary>
    public class EmojiCheckResult
    {
        public bool ContainsTooManyEmotes { get; private set; }
        public int TotalEmotesInMessage { get; private set; }
        public double? EmotePercentage { get; private set; }

        public EmojiCheckResult(bool containsTooManyEmotes, int totalEmotesInMessage, double? emotePercentage)
        {
            ContainsTooManyEmotes = containsTooManyEmotes;
            TotalEmotesInMessage = totalEmotesInMessage;
            EmotePercentage = emotePercentage;
        }
    }
}
