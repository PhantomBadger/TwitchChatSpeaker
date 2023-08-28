using System.Net;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using TwitchLib.Client.Models;
using TwitchLib.Client;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Settings;
using Logging;
using Logging.API;
using TwitchChatSpeaker.Helpers;

namespace TwitchChatSpeaker
{
    public class Program
    {
        private static BlockingCollection<string> MessagesToReadOut;
        private static ConcurrentQueue<string> MessagesToReadOutQueue;
        private static bool isSpeaking;

        /// <summary>
        /// Main entry point of the application, setups up required synthesizers and twitch clients and then polls an async queue for
        /// messages whilst the client listens for them on a different thread
        /// </summary>
        static void Main(string[] args)
        {
            ILogger logger = new ConsoleLogger();
            MessagesToReadOutQueue = new ConcurrentQueue<string>();
            MessagesToReadOut = new BlockingCollection<string>(MessagesToReadOutQueue);
            isSpeaking = false;

            // Set up synthesizer to the default output - trust this is enough for now
            var synthesizer = new SpeechSynthesizer();
            synthesizer.SetOutputToDefaultAudioDevice();
            synthesizer.Rate = 3;

            // We will randomly pick a different installed voice for each message, if we have none installed report an error and exit
            ReadOnlyCollection<InstalledVoice> installedVoices = synthesizer.GetInstalledVoices();
            Random random = new Random();
            if (installedVoices.Count <= 0)
            {
                logger.Error($"You dont have any voices installed!!");
                logger.Information($"Press any key to exit...");
                logger.ReadChar();
                return;
            }
            logger.Information($"Will speak chat messages in one of '{installedVoices.Count}' installed voices");

            // Get the user settings
            var userSettings = FileHelper.LoadSettingsAsync().GetAwaiter().GetResult(); // Bypass for no async here.
            
            // Seems to be required to save the TwitchLib dying sometimes
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Initialise the settings and attempt to load the OAuth Token
            var oAuthToken = userSettings.TwitchOAuthKey;
            var twitchName = userSettings.TwitchChannelName;
            
            // If the Oauth Token is bad, exit now
            if (string.IsNullOrWhiteSpace(oAuthToken))
            {
                logger.Error($"No valid OAuth token found in the {TwitchSettingsContext.SettingsFileName} file!");
                logger.Information($"Press any key to exit...");
                logger.ReadChar();
                return;
            }
            if (string.IsNullOrWhiteSpace(twitchName))
            {
                logger.Error($"No valid TwitchAccountName found in the {TwitchSettingsContext.SettingsFileName} file!");
                logger.Information($"Press any key to exit...");
                logger.ReadChar();
                return;
            }

            oAuthToken = oAuthToken.Trim();
            twitchName = twitchName.Trim();
            logger.Information($"Setting up Twitch Chat Client for '{twitchName}'");

            // Set up the TwitchClient using our provided credentials
            TwitchClient twitchClient = null;
            try
            {
                var credentials = new ConnectionCredentials(twitchName, oAuthToken);
                var clientOptions = new ClientOptions();
                WebSocketClient webSocketClient = new WebSocketClient(clientOptions);
                twitchClient = new TwitchClient(webSocketClient);
                twitchClient.Initialize(credentials, twitchName);

                //twitchClient.OnLog += TwitchClient_OnLog;
                twitchClient.OnMessageReceived += TwitchClient_OnMessageReceived;

                twitchClient.Connect();
            }
            catch (Exception e)
            {
                logger.Error($"Encountered error when trying to create Twitch Client. Check your Oauth token is correct!");
                logger.Information($"Press any key to exit...");
                logger.ReadChar();
                return;
            }

            logger.Information($"Listening for chat messages in '{twitchName}'...");
            logger.Information($"================================");
            do
            {
                string nextMessage = MessagesToReadOut.Take();
                isSpeaking = true;

                // pick a new voice
                var newVoice = installedVoices[random.Next(installedVoices.Count)];
                synthesizer.SelectVoice(newVoice.VoiceInfo.Name);

                logger.Information($"Speaking \"{nextMessage}\"");
                synthesizer.Speak(nextMessage);

                isSpeaking = false;
            } while (true);
        }

        /// <summary>
        /// Called by <see cref="TwitchClient.OnMessageReceived"/> whenever a chat message appears, drops messages if they're too long or we're already speaking
        /// </summary>
        private static void TwitchClient_OnMessageReceived(object? sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            if (isSpeaking)
            {
                return;
            }

            string rawMessage = e.ChatMessage.Message;
            if (rawMessage.Length > 80)
            {
                return;
            }
            MessagesToReadOut.Add(rawMessage);
        }
    }
    
}