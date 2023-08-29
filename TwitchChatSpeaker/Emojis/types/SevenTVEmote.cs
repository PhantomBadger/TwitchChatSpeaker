namespace TwitchChatSpeaker.Emojis.types;

public class SevenTVUser
{
    public string display_name;
    public int emote_capacity;
    public SevenTVEmoteSet emote_set;
    public string id;
    public string platform;
    public string username;

    public SevenTVUser()
    {
        display_name = "";
        emote_capacity = 1000;
        emote_set = new SevenTVEmoteSet();
        id = "";
        platform = "";
        username = "";
    }
}

public class SevenTVEmoteSet
{
    public int capacity;
    public int emote_count;
    public List<SevenTVEmote> emotes;
    public int flags;
    public string id;
    public bool immutable;
    public string name;
    public bool priviliged;

    public SevenTVEmoteSet()
    {
        capacity = 0;
        emote_count = 0;
        emotes = new List<SevenTVEmote>();
        flags = 0;
        id = "";
        immutable = true;
        name = "";
        priviliged = false;
    }
}

public class SevenTVEmote
{
    public SevenTVEmoteData data;
    public int flags;
    public string id;
    public string name;

    public SevenTVEmote()
    {
        data = new SevenTVEmoteData();
        flags = 0;
        id = "";
        name = "";
    }
}

public class SevenTVEmoteData
{
    public bool animated;
    public int flags;
    public SevenTVEmoteDataHost host;
    public string id;
    public int lifecycle;
    public bool listed;
    public string name;

    public SevenTVEmoteData()
    {
        animated = false;
        flags = 0;
        host = new SevenTVEmoteDataHost();
        id = "";
        lifecycle = 3;
        listed = false;
        name = "";
    }
}

public class SevenTVEmoteDataHost
{
    public List<SevenTVEmoteDataHostFile> files;
    public string url;

    public SevenTVEmoteDataHost()
    {
        files = new List<SevenTVEmoteDataHostFile>();
        url = "";
    }
}

public class SevenTVEmoteDataHostFile
{
    public string format;
    public int frame_count;
    public int height;
    public int width;
    public string name;
    public int size;
    public string static_name;

    public SevenTVEmoteDataHostFile()
    {
        format = "";
        frame_count = 1;
        height = 0;
        width = 0;
        name = "";
        size = 0;
        static_name = "";
    }
}