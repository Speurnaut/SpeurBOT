using System.Speech.Synthesis;
using System.Text.RegularExpressions;

namespace SpeurBOT.TTS
{
    public class TtsEngine
    {
        const string logPrefix = "//TTSEngine//:";
        private readonly string _pattern = "(?<=[.!?])";
        private SpeechSynthesizer _speechSynthesizer;
        public bool IsSpeaking = false;

        public TtsEngine()
        {
            _speechSynthesizer = new SpeechSynthesizer();
            _speechSynthesizer.SpeakStarted += _onStartSpeaking;
            _speechSynthesizer.SpeakCompleted += _onStopSpeaking;
        }

        public void Say(string message)
        {
            PromptBuilder pb = new PromptBuilder();
            pb.StartParagraph();

            foreach (var sentence in Regex.Split(message, _pattern))
            {
                if (string.IsNullOrEmpty(sentence))
                    continue;

                pb.StartSentence();
                pb.AppendText(sentence);
                pb.EndSentence();
            }

            pb.EndParagraph();

            Log($"\"{message}\"");
            _speechSynthesizer.SpeakAsync(pb);
        }

        private void _onStopSpeaking(object? sender, SpeakCompletedEventArgs e)
        {
            IsSpeaking = false;
        }

        private void _onStartSpeaking(object? sender, SpeakStartedEventArgs e)
        {
            IsSpeaking = true;
        }

        private void Log(string message)
        {
            Console.WriteLine($"{logPrefix} {message}");
        }
    }
}
