using Newtonsoft.Json;

namespace TwitchChatSpeaker.Emojis.types;

/// <summary>
/// Aggregate class mapping to the Emote payload from 7TV's API
/// </summary>
public class SevenTVEmote
{
    [JsonProperty("data")]
    public SevenTVEmoteData Data { get; set; }

    [JsonProperty("flags")]
    public int Flags { get; set; }

    [JsonProperty("id")]
    public string ID { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Default ctor for creating a <see cref="SevenTVEmote"/>
    /// </summary>
    public SevenTVEmote()
    {
        Data = new SevenTVEmoteData();
        Flags = 0;
        ID = "";
        Name = "";
    }
}
