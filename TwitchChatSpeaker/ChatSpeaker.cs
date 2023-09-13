using System.Net;
using System.Speech.Synthesis;
using TwitchLib.Client.Models;
using TwitchLib.Client;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Settings;
using Logging;
using TwitchChatSpeaker.Emojis;
using TwitchChatSpeaker.Helpers;
using ILogger = Logging.API.ILogger;
using System.Windows;
using System.Threading;

namespace TwitchChatSpeaker
{
    /// <summary>
    /// Class for handling the Speech through Synthesizer and Twitch Chat
    /// </summary>
    public class ChatSpeaker
    {
        public delegate void OnIsActiveChangedDelegate(string twitchChannel);
        public delegate void OnChatMessageSpeakStartDelegate(string speakerName, string message);
        public delegate void OnChatMessageSpeakEndDelegate();
        public event OnIsActiveChangedDelegate OnIsActiveChanged;
        public event OnChatMessageSpeakStartDelegate OnChatMessageSpeakStart;
        public event OnChatMessageSpeakEndDelegate OnChatMessageSpeakEnd;

        public ReadOnlyCollection<InstalledVoice> InstalledVoices { get; private set; }
        public InstalledVoice VoiceToUse { get; set; }
        public bool ShouldRandomizeVoice { get; set; }
        public bool IsActive { get; private set; }

        private readonly ILogger logger;
        private readonly BlockingCollection<TTSMessage> messagesToReadOut;
        private readonly ConcurrentQueue<TTSMessage> messagesToReadOutQueue;
        private readonly UserSettings userSettings;
        private readonly SpeechSynthesizer synthesizer;
        private readonly Random random;
        private readonly CancellationTokenSource cancellationTokenSource;

        private CancellationToken cancellationToken;
        private EmojiManager emoteManager;
        private TwitchClient twitchClient;
        private Thread processingThread;
        private bool isSpeaking;
        private string lastMessage = "";
        private int uniqueAttemptCount = 0;

        /// <summary>
        /// Ctor for creating a <see cref="ChatSpeaker"/>
        /// </summary>
        public ChatSpeaker(ILogger logger, UserSettings userSettings)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.userSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
            messagesToReadOutQueue = new ConcurrentQueue<TTSMessage>();
            messagesToReadOut = new BlockingCollection<TTSMessage>(messagesToReadOutQueue);

            isSpeaking = false;

            synthesizer = new SpeechSynthesizer();
            synthesizer.SetOutputToDefaultAudioDevice();
            synthesizer.Rate = 3;

            InstalledVoices = synthesizer.GetInstalledVoices();
            if (InstalledVoices.Count <= 0)
            {
                MessageBox.Show($"You dont have any Windows voices installed!");
                logger.Error($"You dont have any Windows voices installed!");
                return;
            }
            VoiceToUse = InstalledVoices[0];
            logger.Information($"Found '{InstalledVoices.Count}' installed voices");
            random = new Random();

            // Seems to be required to save the TwitchLib dying sometimes
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // Setup cancellation token
            cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Starts the current TTS, connects to twitch
        /// </summary>
        public Task<bool> StartTTS()
        {
            if (IsActive)
            {
                return Task.FromResult(false);
            }
            
            // Initialise the settings and attempt to load the OAuth Token
            var oAuthToken = userSettings.TwitchOAuthKey;
            var twitchName = userSettings.TwitchChannelName;

            // If the Oauth Token is bad, exit now
            if (string.IsNullOrWhiteSpace(oAuthToken))
            {
                MessageBox.Show($"No valid OAuth token found in the {Constants.SettingsFileName} file!");
                logger.Error($"No valid OAuth token found in the {Constants.SettingsFileName} file!");
                return Task.FromResult(false);
            }
            if (string.IsNullOrWhiteSpace(twitchName))
            {
                MessageBox.Show($"No valid OAuth token found in the {Constants.SettingsFileName} file!");
                logger.Error($"No valid TwitchAccountName found in the {Constants.SettingsFileName} file!");
                return Task.FromResult(false);
            }

            oAuthToken = oAuthToken.Trim();
            twitchName = twitchName.Trim();
            logger.Information($"Setting up Twitch Chat Client for '{twitchName}'");

            // Set up the EmojiManager
            emoteManager = new EmojiManager(userSettings.TwitchChannelId, logger);

            // Set up the TwitchClient using our provided credentials
            return Task.Run(() =>
            {
                try
                {
                    // Seems to be required to save the TwitchLib dying sometimes
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    var credentials = new ConnectionCredentials(twitchName, oAuthToken);
                    var clientOptions = new ClientOptions();
                    WebSocketClient webSocketClient = new WebSocketClient(clientOptions);
                    twitchClient = new TwitchClient(webSocketClient);
                    twitchClient.Initialize(credentials, twitchName);

                    twitchClient.OnMessageReceived += TwitchClient_OnMessageReceived;

                    twitchClient.Connect();
                }
                catch (Exception e)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Encountered error when trying to create Twitch Client. Check your Oauth token is correct!");
                    });
                    logger.Error($"Encountered error when trying to create Twitch Client. Check your Oauth token is correct! {e.ToString()}");

                    if (twitchClient != null)
                    {
                        twitchClient.OnMessageReceived -= TwitchClient_OnMessageReceived;
                        twitchClient.Disconnect();
                        twitchClient = null;
                    }
                    return false;
                }

                cancellationTokenSource.TryReset();
                processingThread = new Thread(ProcessVoiceRequests)
                {
                    IsBackground = true,
                };
                processingThread.Start();
                logger.Information($"Listening for chat messages in '{twitchName}'...");

                IsActive = true;
                OnIsActiveChanged?.Invoke(twitchName);
                return true;
            });
        }

        /// <summary>
        /// Stops the current TTS
        /// </summary>
        public void StopTTS()
        {
            if (!IsActive)
            {
                return;
            }

            twitchClient.OnMessageReceived -= TwitchClient_OnMessageReceived;
            twitchClient.Disconnect();
            twitchClient = null;

            cancellationTokenSource.Cancel();
            processingThread = null;

            IsActive = false;
            OnIsActiveChanged?.Invoke(string.Empty);
        }

        public void ProcessVoiceRequests()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TTSMessage nextMessage = messagesToReadOut.Take(cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                isSpeaking = true;

                // pick a new voice
                if (ShouldRandomizeVoice)
                {
                    var newVoice = InstalledVoices[random.Next(InstalledVoices.Count)];
                    synthesizer.SelectVoice(newVoice.VoiceInfo.Name);
                }
                else
                {
                    synthesizer.SelectVoice(VoiceToUse.VoiceInfo.Name);
                }

                var percentString = "Percentage not calculated";
                if (nextMessage.PercentageOfEmotes != null)
                {
                    percentString = $"{percentString}% emote";
                }

                logger.Information($"Speaking \"{nextMessage.Message}\" with {nextMessage.EmojiCount} emotes ({percentString}).");
                OnChatMessageSpeakStart?.Invoke(nextMessage.Username, nextMessage.Message);
                synthesizer.Speak(nextMessage.Message);
                OnChatMessageSpeakEnd?.Invoke();

                // Set last message to avoid repeats.
                lastMessage = nextMessage.Message;

                isSpeaking = false;
            }
        }

        /// <summary>
        /// Called by <see cref="TwitchClient.OnMessageReceived"/> whenever a chat message appears, drops messages if they're too long or we're already speaking
        /// </summary>
        private void TwitchClient_OnMessageReceived(object? sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            if (isSpeaking)
            {
                return;
            }

            if (twitchClient == null)
            {
                return;
            }

            // Remove a weird bug I've noticed with styff
            string rawMessage = e.ChatMessage.Message;
            if (rawMessage.Length > userSettings.MaxMessageLength)
            {
                return;
            }

            if (userSettings.PrioritiseUniqueMessages && uniqueAttemptCount < userSettings.AttemptsBeforeFailingUnique)
            {
                // Try to get a message that isn't equal to the previous message.
                if (rawMessage == lastMessage)
                {
                    uniqueAttemptCount++;
                    logger.Information(
                        $"Message read matches previous read message (\"{rawMessage}\"), trying {(userSettings.AttemptsBeforeFailingUnique - uniqueAttemptCount) + 1} more times.");
                    return;
                }
            }

            // Moderation checks
            if (userSettings.FilterMessages && Moderation.FilterCheck(rawMessage, userSettings))
            {
                return;
            }
            EmojiCheckResult result = Moderation.EmojiCheck(rawMessage, e.ChatMessage.EmoteSet, userSettings, emoteManager, logger);
            if (userSettings.LimitEmojis && result.ContainsTooManyEmotes)
            {
                return;
            }

            // We reset it here because then it's either unique or repeated but it's going through!
            uniqueAttemptCount = 0;
            
            messagesToReadOut.Add(new TTSMessage(e.ChatMessage.Username, rawMessage, result.TotalEmotesInMessage, result.EmotePercentage));
        }
    }
}