using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchChatSpeaker
{
    public class TTSMessage
    {
        public string Message;
        public int EmojiCount;
        public double? PercentageOfEmotes;

        public TTSMessage(string message, int emojiCount, double? perEmojiCount)
        {
            Message = message;
            EmojiCount = emojiCount;
            PercentageOfEmotes = perEmojiCount;
        }
    }
}
