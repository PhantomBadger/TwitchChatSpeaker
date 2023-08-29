using Newtonsoft.Json;

namespace TwitchChatSpeaker.Emojis.types;

/// <summary>
/// Aggregate class mapping to the User payload from 7TV's API
/// </summary>
public class SevenTVUser
{
    [JsonProperty("display_name")]
    public string DisplayName { get; set; }

    [JsonProperty("emote_capacity")]
    public int EmoteCapacity { get; set; }

    [JsonProperty("emote_set")]
    public SevenTVEmoteSet EmoteSet { get; set; }

    [JsonProperty("id")]
    public string ID { get; set; }

    [JsonProperty("platform")]
    public string Platform { get; set; }

    [JsonProperty("username")]
    public string Username { get; set; }

    /// <summary>
    /// Default ctor for creating a <see cref="SevenTVUser"/>
    /// </summary>
    public SevenTVUser()
    {
        DisplayName = "";
        EmoteCapacity = 1000;
        EmoteSet = new SevenTVEmoteSet();
        ID = "";
        Platform = "";
        Username = "";
    }
}

