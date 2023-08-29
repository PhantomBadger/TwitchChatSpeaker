using Newtonsoft.Json;

namespace TwitchChatSpeaker.Emojis.types;

/// <summary>
/// Aggregate class mapping to the Emote Data Host payload from 7TV's API
/// </summary>
public class SevenTVEmoteDataHost
{
    [JsonProperty("files")]
    public List<SevenTVEmoteDataHostFile> Files { get; set; }

    [JsonProperty("url")]
    public string URL { get; set; }

    /// <summary>
    /// Default ctor for creating a <see cref="SevenTVEmoteDataHost"/>
    /// </summary>
    public SevenTVEmoteDataHost()
    {
        Files = new List<SevenTVEmoteDataHostFile>();
        URL = "";
    }
}

