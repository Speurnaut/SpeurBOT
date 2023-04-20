using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace SpeurBOT.Clients
{
    public class ChatClient
    {
        const string logPrefix = "//ChatClient//:";
        const string ip = "irc.chat.twitch.tv";
        const int port = 6697;
        const int maxRetries = 5;

        private string channel;
        private StreamReader streamReader;
        private StreamWriter streamWriter;
        private TaskCompletionSource<int> connected = new TaskCompletionSource<int>();
        private readonly Dictionary<string, List<string>> userPools;

        public event TwitchChatEventHandler OnMessage = delegate { };
        public delegate void TwitchChatEventHandler(object sender, TwitchChatMessage e);

        public ChatClient()
        {
            this.userPools = new Dictionary<string, List<string>>();
        }

        public async Task Start(string nick, string pass, string channel)
        {
            this.channel = channel;

            Log("Connecting to Twitch Chat...");
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ip, port);

            SslStream sslStream = new SslStream(tcpClient.GetStream(), false, ValidateServerCertificate, null);

            await sslStream.AuthenticateAsClientAsync(ip);

            streamReader = new StreamReader(sslStream);
            streamWriter = new StreamWriter(sslStream) { NewLine = "\r\n", AutoFlush = true };

            await streamWriter.WriteLineAsync($"PASS {pass}");
            await streamWriter.WriteLineAsync($"NICK {nick}");
            await streamWriter.WriteLineAsync($"JOIN #{channel}");

            bool isConnected = false;
            int attempts = 1;

            while (!isConnected && attempts <= maxRetries)
            {
                if (attempts > 1)
                    Log($"Twitch Connection failed, retrying ({attempts})");

                string? response = await streamReader.ReadLineAsync();
                if (response != null && response.StartsWith(":tmi.twitch.tv") && response.Contains("Welcome, GLHF!"))
                {
                    isConnected = true;
                    Log("Connected to Twitch Chat.");
                    connected.SetResult(0);
                }
                else
                {
                    attempts++;
                }
            }

            while (true)
            {
                string? line = await streamReader.ReadLineAsync();
                await ParseMessage(line);
            }
        }

        public async Task ParseMessage(string line)
        {
            if (string.IsNullOrEmpty(line))
                return;

            string[] split = line.Split(" ");

            if (line.StartsWith("PING"))
            {
                await streamWriter.WriteLineAsync($"PONG {split[1]}");
            }

            if (split.Length > 2 && split[1] == "PRIVMSG")
            {
                int exclamationPointPosition = split[0].IndexOf("!");
                string username = split[0][1..exclamationPointPosition];
                int secondColonPosition = line.IndexOf(':', 1);
                string message = line[(secondColonPosition + 1)..];
                string channel = split[2].TrimStart('#');

                OnMessage(this, new TwitchChatMessage
                {
                    Message = message,
                    Sender = username,
                    Channel = channel
                });
            }
        }

        public async Task SendMessage(string message)
        {
            await connected.Task;
            await streamWriter.WriteLineAsync($"PRIVMSG #{channel} :{message}");
            Console.WriteLine($"speurbot: {message}");
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None;
        }

        private void Log(string message)
        {
            Console.WriteLine($"{logPrefix} {message}");
        }
    }
}
