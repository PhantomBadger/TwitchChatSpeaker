using Logging;
using Logging.API;
using Newtonsoft.Json;
using TwitchChatSpeaker.Emojis.types;
using TwitchChatSpeaker.Emojis.utils;
using TwitchLib.Client.Models;

namespace TwitchChatSpeaker.Emojis;

/// <summary>
/// Responsible for managing the detection and assertion of emotes within given strings
/// </summary>
public class EmojiManager
{
    private readonly HttpClient client = new HttpClient();
    private readonly string channelId;
    private readonly List<SevenTVEmote> emotes;
    
    /// <summary>
    /// Ctor for creating an <see cref="EmojiManager"/>
    /// </summary>
    /// <param name="channelId">The ID of the channel to query 7TV for to access emote sets. Cannot be null.</param>
    public EmojiManager(string channelId)
    {
        this.channelId = channelId ?? throw new ArgumentNullException(nameof(channelId));
        emotes = new List<SevenTVEmote>();
        client = new HttpClient();

        var globalEmoteResponseString = client.GetStringAsync(Constants.SevenTVGlobal).GetAwaiter().GetResult();
        var channelEmoteResponseString = client.GetStringAsync($"{Constants.SevenTVChannel}{this.channelId}").GetAwaiter().GetResult();
        
        var globalEmoteSet = JsonConvert.DeserializeObject<SevenTVEmoteSet>(globalEmoteResponseString);
        var channelUser = JsonConvert.DeserializeObject<SevenTVUser>(channelEmoteResponseString);
        
        emotes.AddRange(globalEmoteSet!.Emotes);
        emotes.AddRange(channelUser!.EmoteSet.Emotes);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the provided string is an emote, and <see langword="false"/> if not
    /// </summary>
    /// <param name="potentialEmote">The string to determine if it's an emote or not</param>
    /// <returns><see langword="true"/> if the provided string is an emote, and <see langword="false"/> if not</returns>
    public bool IsEmote(string potentialEmote)
    {
        if (string.IsNullOrWhiteSpace(potentialEmote))
        {
            return false;
        }

        var isEmote = false;
        foreach (var emote in emotes)
        {
            if (isEmote)
            {
                continue;
            }
            if (emote.Name == potentialEmote)
            {
                isEmote = true;
            }
        }

        return isEmote;
    }

    /// <summary>
    /// Gets the <see cref="SevenTVEmote"/> for the provided string if one exists
    /// </summary>
    /// <param name="potentialEmote">The string to determine if it's an emote or not</param>
    /// <returns>The <see cref="SevenTVEmote"/> of the string if found, <see langword="null"/> if none is found</returns>
    public SevenTVEmote? GetEmote(string potentialEmote)
    {
        SevenTVEmote? isEmote = null;
        if (string.IsNullOrWhiteSpace(potentialEmote))
        {
            return isEmote;
        }

        foreach (var emote in emotes)
        {
            if (isEmote != null)
            {
                continue;
            }
            if (emote.Name == potentialEmote)
            {
                isEmote = emote;
            }
        }

        return isEmote;
    }

    /// <summary>
    /// Gets the number of emotes present in a provided string
    /// </summary>
    /// <param name="str">The string to determine if it's an emote or not. Checks each word within the string</param>
    /// <returns>The number of emotes found within the string</returns>
    public int GetEmoteCount(string str)
    {
        var count = 0;
        if (string.IsNullOrWhiteSpace(str))
        {
            return count;
        }

        var splitStr = str.Split(" ");

        foreach (var word in splitStr)
        {
            if (IsEmote(word))
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Gets the total length of all emote names within the provided string
    /// </summary>
    /// <param name="str">The string to determine if it's an emote or not. Checks each word within the string</param>
    /// <returns>The total length of all emote names within the provided string</returns>
    public double GetTotalEmoteNameCount(string str)
    {
        var count = 0;
        if (string.IsNullOrWhiteSpace(str))
        {
            return count;
        }
        var splitStr = str.Split(" ");

        foreach (var word in splitStr)
        {
            var potentialEmote = GetEmote(word);
            if (potentialEmote != null)
            {
                count += potentialEmote.Name.Length;
            }
        }

        return count;
    }
}