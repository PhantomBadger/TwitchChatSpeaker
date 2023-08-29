using Logging;
using Logging.API;
using Newtonsoft.Json;
using Settings;

namespace TwitchChatSpeaker.Helpers;

public static class FileHelper
{
    public static string GetFilePath(string name, string ext)
    {
        return $"{Directory.GetCurrentDirectory()}\\{name}.{ext}";
    }

    public static string GetFilePath(string file)
    {
        return $"{Directory.GetCurrentDirectory()}\\{file}";
    }
    
    public static async Task<UserSettings> LoadSettingsAsync()
    {
        var settings = new UserSettings(); // Sets the default configuration values for now.
    
        ILogger logger = new ConsoleLogger();
        logger.Information(GetFilePath(TwitchSettingsContext.SettingsFileName));

        if (!File.Exists(GetFilePath(TwitchSettingsContext.SettingsFileName)))
        {
            // Convert default UserSettings into JSON using JsonConvert and save file.
            logger.Information("Couldn't find config file.");
            var defaultFileContents = JsonConvert.SerializeObject(settings, Formatting.Indented).Replace("\r\n", "\n"); // Normalise some stuff
            await File.WriteAllTextAsync(GetFilePath(TwitchSettingsContext.SettingsFileName), defaultFileContents);
        }
        else
        {
            // File found, read and deseralize into a UserSettings which we store as settings.
            logger.Information("Found config");
            var fileContents = await File.ReadAllTextAsync(GetFilePath(TwitchSettingsContext.SettingsFileName));
            settings = JsonConvert.DeserializeObject<UserSettings>(fileContents);
            logger.Information(JsonConvert.SerializeObject(settings, Formatting.Indented).Replace("\r\n", "\n"));
            if (settings == null)
            {
                throw new InvalidDataException($"Failed to deserialize settings at {GetFilePath(TwitchSettingsContext.SettingsFileName)}");
            }
        }

        return settings;
    }

    public static async Task SaveSettingsAsync(UserSettings settings)
    {
        var fileContents = JsonConvert.SerializeObject(settings, Formatting.Indented).Replace("\r\n", "\n"); // Normalise some stuff
        await File.WriteAllTextAsync(GetFilePath(TwitchSettingsContext.SettingsFileName), fileContents);
    }
}