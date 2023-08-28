using Newtonsoft.Json;
using Settings;

namespace TwitchChatSpeaker.Helpers;

public static class FileHelper
{
    public static async Task<UserSettings> LoadSettingsAsync()
    {
        var settings = new UserSettings(); // Sets the default configuration values for now.

        if (!File.Exists(TwitchSettingsContext.SettingsFileName))
        {
            // Convert default UserSettings into JSON using JsonConvert and save file.
            var defaultFileContents = JsonConvert.SerializeObject(settings, Formatting.Indented).Replace("\r\n", "\n"); // Normalise some stuff
            await File.WriteAllTextAsync(TwitchSettingsContext.SettingsFileName, defaultFileContents);
        }
        else
        {
            // File found, read and deseralize into a UserSettings which we store as settings.

            var fileContents = await File.ReadAllTextAsync(TwitchSettingsContext.SettingsFileName);
            settings = JsonConvert.DeserializeObject<UserSettings>(fileContents);
            if (settings == null)
                throw new InvalidDataException($"Failed to deserialize settings at {TwitchSettingsContext.SettingsFileName}");
        }

        return settings;
    }

    public static async Task SaveSettingsAsync(UserSettings settings)
    {
        var fileContents = JsonConvert.SerializeObject(settings, Formatting.Indented).Replace("\r\n", "\n"); // Normalise some stuff
        await File.WriteAllTextAsync(TwitchSettingsContext.SettingsFileName, fileContents);
    }
}