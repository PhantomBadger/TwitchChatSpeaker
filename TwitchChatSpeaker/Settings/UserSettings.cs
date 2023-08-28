namespace Settings;

public class UserSettings
{
    public UserSettings()
    {
        // Default configuration values go here
        // The way this structures this is that we don't need to provide our own
        // default in the main code, since it's already defined here and is overwritten
        // when the config file is loaded by the ConfigHelper

        TwitchOAuthKey = "";
        TwitchChannelName = "";
        MinimumEmojiLimit = 1;
        EmojiPercentageLimit = 0;
        FilteredWords = new List<string>();
        PubliclyDeclareFilters = false;
    }
    
    // We actually define types here!
    public string TwitchOAuthKey { get; set; }
    public string TwitchChannelName { get; set; }
    public int MinimumEmojiLimit { get; set; }
    public int EmojiPercentageLimit { get; set; }
    public List<string> FilteredWords { get; set; }
    public bool PubliclyDeclareFilters { get; set; }
}