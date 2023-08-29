using Newtonsoft.Json;

namespace TwitchChatSpeaker.Emojis.types;

/// <summary>
/// Aggregate class mapping to the Emote Data Host File Payload from 7TV's API
/// </summary>
public class SevenTVEmoteDataHostFile
{
    [JsonProperty("format")]
    public string Format { get; set; }

    [JsonProperty("frame_count")]
    public int FrameCount { get; set; }

    [JsonProperty("height")]
    public int Height { get; set; }

    [JsonProperty("width")]
    public int Width { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("size")]
    public int Size { get; set; }

    [JsonProperty("static_name")]
    public string StaticName { get; set; }

    /// <summary>
    /// Default ctor for creating a <see cref="SevenTVEmoteDataHostFile"/>
    /// </summary>
    public SevenTVEmoteDataHostFile()
    {
        Format = "";
        FrameCount = 1;
        Height = 0;
        Width = 0;
        Name = "";
        Size = 0;
        StaticName = "";
    }
}

