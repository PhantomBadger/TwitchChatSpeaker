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
using Microsoft.Extensions.Logging;
using TwitchChatSpeaker.Emojis;
using TwitchChatSpeaker.Helpers;
using ILogger = Logging.API.ILogger;

namespace TwitchChatSpeaker
{
    public class Program
    {
        private static ILogger logger;
        private static UserSettings UserSettings;
        private static EmojiManager EmoteManager;
        
        // State parameters
        private static BlockingCollection<TTSMessage> MessagesToReadOut;
        private static ConcurrentQueue<TTSMessage> MessagesToReadOutQueue;
        private static bool isSpeaking;
        private static string LastMessage = "";
        private static int UniqueAttemptCount = 0;
        
        /// <summary>
        /// Main entry point of the application, setups up required synthesizers and twitch clients and then polls an async queue for
        /// messages whilst the client listens for them on a different thread
        /// </summary>
        static void Main(string[] args)
        {
            logger = new ConsoleLogger();
            MessagesToReadOutQueue = new ConcurrentQueue<TTSMessage>();
            MessagesToReadOut = new BlockingCollection<TTSMessage>(MessagesToReadOutQueue);
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
            UserSettings = FileHelper.LoadSettingsAsync().GetAwaiter().GetResult(); // Bypass for no async here.
            
            // Seems to be required to save the TwitchLib dying sometimes
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Initialise the settings and attempt to load the OAuth Token
            var oAuthToken = UserSettings.TwitchOAuthKey;
            var twitchName = UserSettings.TwitchChannelName;
            
            // If the Oauth Token is bad, exit now
            if (string.IsNullOrWhiteSpace(oAuthToken))
            {
                logger.Error($"No valid OAuth token found in the {Constants.SettingsFileName} file!");
                logger.Information($"Press any key to exit...");
                logger.ReadChar();
                return;
            }
            if (string.IsNullOrWhiteSpace(twitchName))
            {
                logger.Error($"No valid TwitchAccountName found in the {Constants.SettingsFileName} file!");
                logger.Information($"Press any key to exit...");
                logger.ReadChar();
                return;
            }

            oAuthToken = oAuthToken.Trim();
            twitchName = twitchName.Trim();
            logger.Information($"Setting up Twitch Chat Client for '{twitchName}'");
            
            // Set up the EmojiManager
            EmoteManager = new EmojiManager(UserSettings.TwitchChannelId);

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
                TTSMessage nextMessage = MessagesToReadOut.Take();
                isSpeaking = true;

                // pick a new voice
                var newVoice = installedVoices[random.Next(installedVoices.Count)];
                synthesizer.SelectVoice(newVoice.VoiceInfo.Name);

                var percentString = "Percentage not calculated";
                if (nextMessage.PercentageOfEmotes != null)
                {
                    percentString = $"{percentString}% emote";
                }

                logger.Information($"Speaking \"{nextMessage.Message}\" with {nextMessage.EmojiCount} emotes ({percentString}).");
                synthesizer.Speak(nextMessage.Message);
                
                // Set last message to avoid repeats.
                LastMessage = nextMessage.Message;

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

            // Remove a weird bug I've noticed with styff
            string rawMessage = e.ChatMessage.Message;
            if (rawMessage.Length > UserSettings.MaxMessageLength)
            {
                return;
            }

            if (UserSettings.PrioritiseUniqueMessages && UniqueAttemptCount < UserSettings.AttemptsBeforeFailingUnique)
            {
                // Try to get a message that isn't equal to the previous message.
                if (rawMessage == LastMessage)
                {
                    UniqueAttemptCount++;
                    logger.Information(
                        $"Message read matches previous read message (\"{rawMessage}\"), trying {(UserSettings.AttemptsBeforeFailingUnique - UniqueAttemptCount) + 1} more times.");
                    return;
                }
            }

            // Moderation checks
            if (UserSettings.FilterMessages && Moderation.FilterCheck(rawMessage, UserSettings))
            {
                return;
            }
            EmojiCheckResult result = Moderation.EmojiCheck(rawMessage, UserSettings, EmoteManager);
            if (UserSettings.LimitEmojis && result.ContainsTooManyEmotes)
            {
                return;
            }

            // We reset it here because then it's either unique or repeated but it's going through!
            UniqueAttemptCount = 0;
            
            MessagesToReadOut.Add(new TTSMessage(rawMessage, result.TotalEmotesInMessage, result.EmotePercentage));
        }
    }
}