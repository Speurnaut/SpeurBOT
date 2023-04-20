using Microsoft.Extensions.Configuration;
using SpeurBOT.Clients;
using SpeurBOT.TTS;

namespace SpeurBOT
{
    class Program
    {
        readonly static ChatClient _chatClient;
        readonly static ObsClient _obsClient;
        readonly static TtsEngine _ttsEngine;
        readonly static string _speurnaut;

        static Program()
        {
            _chatClient = new ChatClient();
            _obsClient = new ObsClient();
            _ttsEngine = new TtsEngine();
        }

        static async Task Main(string[] args)
        {
            Header();
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration config = builder.Build();
            _ = StartChatClient(config["twitch:nick"], config["twitch:pass"], config["twitch:channel"]);
            var obsWebsocketUrl = $"ws://{config["obs:ip"]}:{config["obs:port"]}";
            StartObsClient(obsWebsocketUrl, config["obs:password"], config["obs:sourceGroup"]);

            await Task.Delay(-1);
        }

        static async Task StartChatClient(string nick, string pass, string channel)
        {
            _ = _chatClient.Start(nick, pass, channel);

            _chatClient.OnMessage += async (sender, twitchChatMessage) =>
            {
                Console.WriteLine($"{twitchChatMessage.Sender}: '{twitchChatMessage.Message}'");

                if (twitchChatMessage.Message.StartsWith('!'))
                {
                    if (twitchChatMessage.Message.StartsWith("!status"))
                    {
                        if (twitchChatMessage.Sender == _speurnaut) await Commands.Status(_chatClient, _obsClient.Connected, _ttsEngine);
                    }
                    if (twitchChatMessage.Message.StartsWith("!alarm"))
                    {
                        await Commands.Alarm(_obsClient, GetTwitchMessageWithoutCommand(twitchChatMessage));
                    }
                    if (twitchChatMessage.Message.StartsWith("!tts"))
                    {
                        await Commands.TTS(_obsClient, GetTwitchMessageWithoutCommand(twitchChatMessage));
                    }
                }
            };
        }

        static void StartObsClient(string url, string password, string? sourceGroup)
        {
            _obsClient.Start(url, password, sourceGroup);
        }

        private void onConnect(object sender, EventArgs e)
        {
            Console.WriteLine("Connected to OBS.");
        }

        private void onDisconnect(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected from OBS.");
        }

        static string? GetTwitchMessageWithoutCommand(TwitchChatMessage message)
        {
            if (message.Message.Contains(' ')) return message.Message.Substring(message.Message.IndexOf(' ') + 1);
            return null;
        }

        static void Header()
        {
            Console.WriteLine(@"-----------------------------------------------------");
            Console.WriteLine(@"     _____                       ____  ____  ______");
            Console.WriteLine(@"    / ___/____  ___  __  _______/ __ )/ __ \/_  __/");
            Console.WriteLine(@"    \__ \/ __ \/ _ \/ / / / ___/ __  / / / / / /   ");
            Console.WriteLine(@"   ___/ / /_/ /  __/ /_/ / /  / /_/ / /_/ / / /    ");
            Console.WriteLine(@"  /____/ .___/\___/\__,_/_/  /_____/\____/ /_/     ");
            Console.WriteLine(@"      /_/                                          ");
            Console.WriteLine(@"-----------------------------------------------------");
            Console.WriteLine($"Started at {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");
            Console.WriteLine(@"-----------------------------------------------------");
        }
    }
}