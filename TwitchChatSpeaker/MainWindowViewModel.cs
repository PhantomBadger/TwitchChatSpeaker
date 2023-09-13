using Logging.API;
using Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TwitchChatSpeaker.Helpers;
using TwitchChatSpeaker.Logging;

namespace TwitchChatSpeaker
{
    internal class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Whether the TTS system is currently active or not
        /// </summary>
        public bool IsTTSActive
        {
            get
            {
                return isTTSActive;
            }
            set
            {
                if (isTTSActive != value)
                {
                    isTTSActive = value;
                    RaisePropertyChanged(nameof(IsTTSActive));
                    RaisePropertyChanged(nameof(StartStopTTSText));
                }
            }
        }
        private bool isTTSActive;

        /// <summary>
        /// Whether the TTS system is currently active or not
        /// </summary>
        public bool IsTTSLoading
        {
            get
            {
                return isTTSLoading;
            }
            set
            {
                if (isTTSLoading != value)
                {
                    isTTSLoading = value;
                    RaisePropertyChanged(nameof(IsTTSLoading));
                    RaisePropertyChanged(nameof(StartStopTTSText));
                }
            }
        }
        private bool isTTSLoading;

        /// <summary>
        /// The text to be displayed pending on the state of the TTS system
        /// </summary>
        public string StartStopTTSText
        {
            get
            {
                return IsTTSLoading ? "Starting..." : (IsTTSActive ? "Stop TTS" : "Start TTS");
            }
        }

        /// <summary>
        /// The voices available for use
        /// </summary>
        public ObservableCollection<string> VoiceNames
        {
            get
            {
                return voiceNames;
            }
            set
            {
                if (voiceNames != value)
                {
                    voiceNames = value;
                    RaisePropertyChanged(nameof(VoiceNames));
                }
            }
        }
        private ObservableCollection<string> voiceNames;

        /// <summary>
        /// The selected voice name
        /// </summary>
        public string SelectedVoiceName
        {
            get
            {
                return selectedVoiceName;
            }
            set
            {
                if (selectedVoiceName != value)
                {
                    selectedVoiceName = value;
                    if (voiceNameLookup.ContainsKey(selectedVoiceName) && ttsSpeaker != null)
                    {
                        ttsSpeaker.VoiceToUse = voiceNameLookup[selectedVoiceName];
                    }
                    RaisePropertyChanged(nameof(SelectedVoiceName));
                }
            }
        }
        private string selectedVoiceName;

        /// <summary>
        /// Whether the selected voice should be randomized or not
        /// </summary>
        public bool IsVoiceRandomized
        {
            get
            {
                return isVoiceRandomized;
            }
            set
            {
                if (isVoiceRandomized != value)
                {
                    isVoiceRandomized = value;
                    if (ttsSpeaker != null)
                    {
                        ttsSpeaker.ShouldRandomizeVoice = isVoiceRandomized;
                    }
                    RaisePropertyChanged(nameof(IsVoiceRandomized));
                }
            }
        }
        private bool isVoiceRandomized;

        /// <summary>
        /// The TTS Message currently being spoken
        /// </summary>
        public string CurrentTTSMessage
        {
            get
            {
                return currentTTSMessage;
            }
            set
            {
                if (currentTTSMessage != value)
                {
                    currentTTSMessage = value;
                    RaisePropertyChanged(nameof(CurrentTTSMessage));
                }
            }
        }
        private string currentTTSMessage;

        /// <summary>
        /// The channel currently connected to
        /// </summary>
        public string WindowHeader
        {
            get
            {
                return windowHeader;
            }
            set
            {
                if (windowHeader != value)
                {
                    windowHeader = value;
                    RaisePropertyChanged(nameof(WindowHeader));
                }
            }
        }
        private string windowHeader;

        /// <summary>
        /// Called to Start or Stop the TTS system
        /// </summary>
        public ICommand StartStopTTSCommand { get; private set; }

        /// <summary>
        /// Called to Edit the Settings
        /// </summary>
        public ICommand EditSettingsCommand { get; private set; }

        private readonly ChatSpeaker ttsSpeaker;
        private readonly Dictionary<string, InstalledVoice> voiceNameLookup;
        private readonly UserSettings userSettings;
        private readonly ILogger logger;

        public MainWindowViewModel()
        {
            logger = new FileLogger();
            logger.Information($"Starting Logging at {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");

            // Get the user settings
            userSettings = FileHelper.LoadSettings(logger);

            // Make the TTS speaker
            ttsSpeaker = new ChatSpeaker(logger, userSettings);
            ttsSpeaker.OnIsActiveChanged += OnIsActiveChanged;
            ttsSpeaker.OnChatMessageSpeakStart += OnChatMessageSpeakStart;
            ttsSpeaker.OnChatMessageSpeakEnd += OnChatMessageSpeakEnd;

            // Initialise the variables
            IsTTSActive = false;
            CurrentTTSMessage = "";
            WindowHeader = $"Twitch Chat Speaker";

            // Populate the Voice Names
            VoiceNames = new ObservableCollection<string>();
            voiceNameLookup = new Dictionary<string, InstalledVoice>();
            for (int i = 0; i < ttsSpeaker.InstalledVoices.Count; i++)
            {
                VoiceNames.Add(ttsSpeaker.InstalledVoices[i].VoiceInfo.Name);
                voiceNameLookup.Add(ttsSpeaker.InstalledVoices[i].VoiceInfo.Name, ttsSpeaker.InstalledVoices[i]);
            }

            if (VoiceNames.Count > 0)
            {
                SelectedVoiceName = VoiceNames[0];
            }

            SetupCommands();
        }

        public void Dispose()
        {
            if (ttsSpeaker != null)
            {
                ttsSpeaker.OnIsActiveChanged -= OnIsActiveChanged;
                ttsSpeaker.OnChatMessageSpeakStart -= OnChatMessageSpeakStart;
                ttsSpeaker.OnChatMessageSpeakEnd -= OnChatMessageSpeakEnd;
            }
        }

        /// <summary>
        /// Called when the chat message stops being spoken
        /// </summary>
        private void OnChatMessageSpeakEnd()
        {
            CurrentTTSMessage = "";
        }

        /// <summary>
        /// Called when the chat message starts speaking
        /// </summary>
        private void OnChatMessageSpeakStart(string speakerName, string message)
        {
            CurrentTTSMessage = $"{speakerName}: {message}";
        }

        /// <summary>
        /// Called when the IsActive state changes
        /// </summary>
        private void OnIsActiveChanged(string twitchName)
        {
            IsTTSActive = ttsSpeaker.IsActive;
            if (!string.IsNullOrWhiteSpace(twitchName))
            {
                WindowHeader = $"Twitch Chat Speaker - Connected to {twitchName}";
            }
            else
            {
                WindowHeader = $"Twitch Chat Speaker";
            }
        }

        /// <summary>
        /// Sets up all the <see cref="ICommand"/> instances
        /// </summary>
        private void SetupCommands()
        {
            StartStopTTSCommand = new DelegateCommand(_ => ToggleTTSSystem());

            EditSettingsCommand = new DelegateCommand(_ => OpenEditSettingsWindow());
        }

        /// <summary>
        /// Opens the window to Edit Settings
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void OpenEditSettingsWindow()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Toggles the TTS Speaker
        /// </summary>
        private void ToggleTTSSystem()
        {
            if (ttsSpeaker.IsActive)
            {
                ttsSpeaker.StopTTS();
            }
            else
            {
                IsTTSLoading = true;
                Task<bool> startTask = ttsSpeaker.StartTTS();
                startTask.ContinueWith((antecedent) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsTTSLoading = false;
                    });
                });
            }
        }

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event
        /// </summary>
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
