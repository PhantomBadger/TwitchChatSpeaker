using Newtonsoft.Json;

namespace TwitchChatSpeaker.Emojis.types;

/// <summary>
/// Aggregate class mapping to Emote Data from 7TV's API
/// </summary>
public class SevenTVEmoteData
{
    [JsonProperty("animated")]
    public bool Animated { get; set; }

    [JsonProperty("flags")]
    public int Flags { get; set; }

    [JsonProperty("host")]
    public SevenTVEmoteDataHost Host { get; set; }

    [JsonProperty("id")]
    public string ID { get; set; }

    [JsonProperty("lifecycle")]
    public int Lifecycle { get; set; }

    [JsonProperty("listed")]
    public bool Listed { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Default ctor for creating a new <see cref="SevenTVEmoteData"/>
    /// </summary>
    public SevenTVEmoteData()
    {
        Animated = false;
        Flags = 0;
        Host = new SevenTVEmoteDataHost();
        ID = "";
        Lifecycle = 3;
        Listed = false;
        Name = "";
    }
}

