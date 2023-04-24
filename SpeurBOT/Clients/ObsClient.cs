using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Communication;
using OBSWebsocketDotNet.Types.Events;
using SpeurBOT.TTS;

namespace SpeurBOT.Clients
{
    public class ObsClient
    {
        const string logPrefix = "//OBSClient//: ";
        private readonly List<string> ttsUsers = new()
        {
            "speurnaut",
            "speurbot",
            "wyn_ryu"
        };

        private OBSWebsocket _obs;
        private TtsEngine _ttsEngine;
        private string _currentScene;
        private string? _groupName = null;
        private string _ttsFilePath = "tts.html";

        public bool Connected { get; set; }

        public ObsClient()
        {
            _ttsEngine = new TtsEngine();
        }

        public void Start(string url, string password, string ttsFilePath, string? groupName = null)
        {
            _ttsFilePath = ttsFilePath;
            Log("Connecting to OBS...");
            if (!string.IsNullOrEmpty(groupName)) _groupName = groupName;
            _obs = new OBSWebsocket();

            _obs.Connected += onConnect;
            _obs.Disconnected += onDisconnect;
            _obs.CurrentProgramSceneChanged += onCurrentSceneChanged;

            _obs.ConnectAsync(url, password);
        }

        public async Task TextToSpeech(string message, string? sender)
        {
            var trimmedMessage = message;

            var senderAvatar = sender;

            if (!ttsUsers.Contains(sender))
            {
                senderAvatar = "anonymous";
            }

            if (string.IsNullOrEmpty(sender))
            {
                sender = "anonymous";
            }

            int fontSize;

            if (message.Length <= 68)
            {
                fontSize = 47;
            } 
            else if (message.Length <= 101)
            {
                fontSize = 32;
            }
            else if (message.Length <= 271)
            {
                fontSize = 24;
            }
            else
            {
                fontSize = 24;
                trimmedMessage = message.Substring(0, 268) + "...";
            }

            while (_ttsEngine.IsSpeaking){ Thread.Sleep(100); }

            // Write message to local HTML file, which is registered as a browser source in OBS.
            string text = $"<html><head><link rel=\"stylesheet\" href=\"style.css\"></head><body><div class=\"wrapper\"><div class=\"row\"><div class=\"column left\"><div id=\"message\" style=\"font-size: {fontSize}px\">\"{trimmedMessage}\"</div></div><div class=\"column right\"><img id=\"avatar\" src=\"{senderAvatar}.png\" /></div></div><div class=\"row\"><div class=\"column full\"><div id=\"username\">- {sender}</div></div></div></div></body></html>";
            await File.WriteAllTextAsync(_ttsFilePath, text);

            Thread.Sleep(1000); // Give the source time to update before it is shown on screen

            SetSourceActive("TTS", true);

            _ttsEngine.Say(message);
            Thread.Sleep(100); // Ensure isSpeaking = true before we start checking

            while (_ttsEngine.IsSpeaking) { Thread.Sleep(1); } // wait for TTS to finish

            SetSourceActive("TTS", false);

            // Cooldown between TTS reads, otherwise they will overlap and the OBS browser source will switch while onscreen.
            Thread.Sleep(4000);
        }

        public async Task Alarm()
        {
            Log("Sounding Alarm!");
            SetSourceActive("Alarm", true);

            // Video is 4 seconds long, give extra second for loading
            Thread.Sleep(5000); 

            SetSourceActive("Alarm", false);
        }

        private void ChangeScene(string sceneName)
        {
            if (_currentScene != sceneName) _obs.SetCurrentProgramScene(sceneName);
        }

        private void SetSourceActive(string sourceName, bool active)
        {
            try
            {
                var sourceId = _obs.GetSceneItemId(_groupName ?? _currentScene, sourceName, 0);
                _obs.SetSceneItemEnabled(_groupName ?? _currentScene, sourceId, active);
                Log($"Source \"{sourceName}\" (ID: {sourceId}) has been " + (active ? "activated" : "deactivated"));
            }
            catch (ErrorResponseException ex)
            {
                if (ex.ErrorCode == 600)
                {
                    Log($"Could not find ID for source {sourceName} in " +
                        (_groupName != null ? $"group {_groupName}" : $"scene {_currentScene}"));
                } else
                {
                    Log(ex.Message);
                }
            }
        }

        private void onConnect(object? sender, EventArgs e)
        {
            Connected = true;
            Log("Connected to OBS.");
            _currentScene = _obs.GetCurrentProgramScene();
        }

        private void onDisconnect(object? sender, ObsDisconnectionInfo e)
        {
            Connected = false;
            if (!string.IsNullOrEmpty(e?.DisconnectReason))
            {
                Log($"Disconnected from OBS: {e.DisconnectReason}");
            }
            else
            {
                Log($"Disconnected from OBS");
            }
        }

        private void onCurrentSceneChanged(object? sender, ProgramSceneChangedEventArgs e)
        {
            Log($"Scene changed to {e.SceneName}");
            _currentScene = e.SceneName;
        }
        private void Log(string message)
        {
            Console.WriteLine($"{logPrefix} {message}");
        }
    }
}
