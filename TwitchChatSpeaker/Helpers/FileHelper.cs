using Logging;
using Logging.API;
using Newtonsoft.Json;
using Settings;
using System.IO;

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
    
    public static UserSettings LoadSettings(ILogger logger)
    {
        var settings = new UserSettings(); // Sets the default configuration values for now.
    
        logger.Information(GetFilePath(Constants.SettingsFileName));

        if (!File.Exists(GetFilePath(Constants.SettingsFileName)))
        {
            // Convert default UserSettings into JSON using JsonConvert and save file.
            logger.Information("Couldn't find config file.");
            var defaultFileContents = JsonConvert.SerializeObject(settings, Formatting.Indented).Replace("\r\n", "\n"); // Normalise some stuff
            File.WriteAllText(GetFilePath(Constants.SettingsFileName), defaultFileContents);
        }
        else
        {
            // File found, read and deseralize into a UserSettings which we store as settings.
            logger.Information("Found config");
            var fileContents = File.ReadAllText(GetFilePath(Constants.SettingsFileName));
            settings = JsonConvert.DeserializeObject<UserSettings>(fileContents);
            logger.Information(JsonConvert.SerializeObject(settings, Formatting.Indented).Replace("\r\n", "\n"));
            if (settings == null)
            {
                throw new InvalidDataException($"Failed to deserialize settings at {GetFilePath(Constants.SettingsFileName)}");
            }
        }

        return settings;
    }

    public static void SaveSettings(UserSettings settings)
    {
        var fileContents = JsonConvert.SerializeObject(settings, Formatting.Indented).Replace("\r\n", "\n"); // Normalise some stuff
        File.WriteAllTextAsync(GetFilePath(Constants.SettingsFileName), fileContents);
    }
}