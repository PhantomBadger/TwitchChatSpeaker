using Newtonsoft.Json;

namespace TwitchChatSpeaker.Emojis.types;

/// <summary>
/// Aggregate class mapping to the Emote Set payload from 7TV's API
/// </summary>
public class SevenTVEmoteSet
{
    [JsonProperty("capacity")]
    public int Capacity { get; set; }

    [JsonProperty("emote_count")]
    public int EmoteCount { get; set; }

    [JsonProperty("emotes")]
    public List<SevenTVEmote> Emotes { get; set; }

    [JsonProperty("flags")]
    public int Flags { get; set; }

    [JsonProperty("id")]
    public string ID { get; set; }

    [JsonProperty("immutable")]
    public bool Immutable { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("priviliged")]
    public bool Priviliged { get; set; }

    /// <summary>
    /// Default ctor for creating a <see cref="SevenTVEmoteSet"/>
    /// </summary>
    public SevenTVEmoteSet()
    {
        Capacity = 0;
        EmoteCount = 0;
        Emotes = new List<SevenTVEmote>();
        Flags = 0;
        ID = "";
        Immutable = true;
        Name = "";
        Priviliged = false;
    }
}

