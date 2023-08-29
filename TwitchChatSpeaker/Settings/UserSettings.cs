namespace Settings;

public class UserSettings
{
    public UserSettings()
    {
        // Default configuration values go here
        // The way this structures this is that we don't need to provide our own
        // default in the main code, since it's already defined here and is overwritten
        // when the config file is loaded by the FileHelper

        TwitchOAuthKey = "";
        TwitchChannelName = "";
        TwitchChannelId = "";
        MaxMessageLength = 80;
        LimitEmojis = true;
        MaximumEmojiLimit = 1;
        EmojiPercentageLimit = 0;
        PrioritiseUniqueMessages = false;
        AttemptsBeforeFailingUnique = 5;
        FilterMessages = true;
        FilteredWords = new List<string>();
    }
    
    // We actually define types here!
    public string TwitchOAuthKey { get; set; }
    public string TwitchChannelName { get; set; }
    public string TwitchChannelId { get; set; }
    public int MaxMessageLength { get; set; }
    public bool LimitEmojis { get; set; }
    public int MaximumEmojiLimit { get; set; }
    public double EmojiPercentageLimit { get; set; }
    public bool PrioritiseUniqueMessages { get; set; }
    public int AttemptsBeforeFailingUnique { get; set; }
    public bool FilterMessages { get; set; }
    public List<string> FilteredWords { get; set; }
}