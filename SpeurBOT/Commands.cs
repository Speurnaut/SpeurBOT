using SpeurBOT.Clients;
using SpeurBOT.TTS;

namespace SpeurBOT
{
    public static class Commands
    {
        public static async Task Status(ChatClient chatClient, bool connectedToObs, TtsEngine ttsEngine)
        {
            await chatClient.SendMessage($"Connected to Twitch Chat.");
            if (connectedToObs) await chatClient.SendMessage($"Connected to OBS.");
            ttsEngine.Say("TTS is enabled.");
        }

        public static async Task Alarm(ObsClient obsClient, string? message)
        {
            obsClient.Alarm();
            if (!string.IsNullOrEmpty(message))
            {
                obsClient.TextToSpeech(message);
            }
        }

        public static async Task TTS(ObsClient obsClient, string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            obsClient.TextToSpeech(message);
        }
    }
}
