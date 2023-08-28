using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settings
{
    public abstract class TwitchSettingsContext
    {
        public const string SettingsFileName = "TwitchChatSpeaker.settings";
        public const string TwitchOAuthKey = "TwitchOAuth";
        public const string TwitchChannelName = "TwitchChannelName";

        public static Dictionary<string, string> GetDefaultSettings()
        {
            return new Dictionary<string, string>()
            {
                { TwitchOAuthKey, "" },
                { TwitchChannelName, "" }
            };
        }
    }
}
