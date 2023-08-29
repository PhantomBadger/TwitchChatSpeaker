using Logging;
using Logging.API;
using Newtonsoft.Json;
using TwitchChatSpeaker.Emojis.types;
using TwitchChatSpeaker.Emojis.utils;

namespace TwitchChatSpeaker.Emojis;

public class EmojiManager
{
    public List<SevenTVEmote> emotes = new List<SevenTVEmote>();
    public string ChannelId;
    private static readonly HttpClient client = new HttpClient();
    
    public EmojiManager(string channelId)
    {
        ChannelId = channelId;

        var globalEmoteResponseString = client.GetStringAsync(Constants.SevenTVGlobal).GetAwaiter().GetResult();
        var channelEmoteResponseString = client.GetStringAsync($"{Constants.SevenTVChannel}{ChannelId}").GetAwaiter().GetResult();
        
        var globalEmoteSet = JsonConvert.DeserializeObject<SevenTVEmoteSet>(globalEmoteResponseString);
        var channelUser = JsonConvert.DeserializeObject<SevenTVUser>(channelEmoteResponseString);
        
        emotes.AddRange(globalEmoteSet!.emotes);
        emotes.AddRange(channelUser!.emote_set.emotes);
    }

    public bool IsEmote(string potentialEmote)
    {
        var isEmote = false;
        foreach (var emote in emotes)
        {
            if (isEmote) continue;
            if (emote.name == potentialEmote) isEmote = true;
        }

        return isEmote;
    }

    public SevenTVEmote? GetEmote(string potentialEmote)
    {
        SevenTVEmote? isEmote = null;
        foreach (var emote in emotes)
        {
            if (isEmote != null) continue;
            if (emote.name == potentialEmote) isEmote = emote;
        }

        return isEmote;
    }

    public int GetEmoteCount(string str)
    {
        var splitStr = str.Split(" ");
        var count = 0;

        foreach (var word in splitStr)
        {
            if (IsEmote(word)) count++;
        }

        return count;
    }

    public double GetTotalEmoteNameCount(string str)
    {
        var splitStr = str.Split(" ");
        var count = 0;

        foreach (var word in splitStr)
        {
            var potentialEmote = GetEmote(word);
            if (potentialEmote != null) count = count + potentialEmote.name.Length;
        }

        return count;
    }
}