using System.Net;
using System.Net.Sockets;
using System.Text;

#pragma warning disable CS8602 //wyłuskanie odwołania, które może mieć wartośc 'null'.

//dzień dobry tutaj ja ze wczoraj wieczorem

//pisze ten kod bo najprawdopodobniej nie dostane sie na kolejny etap z konkursu kuratoryjnego z informatyki a zależy mi na dobrej ocenie na koneic roku :)

//CLIENT-SIDE LAN MESSENGER

// ~ rudynakodach (8D)
namespace SimpleMessenger
{
    public static class Client
    {
        public static Socket _clientSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public const int serverPort = 100; //host only on port 100.

        public static string username; //dont allow 'null' names

        public static void Main()
        {
            Console.WriteLine("Mode: [HOST/CLIENT]");
            Console.Write("> ");
            string mode = Console.ReadLine().Trim();


            if (mode == "HOST")
            {
                Server.ServerInit();
                return;
            }
            else if (mode == "CLIENT")
            {
                Console.Title = "Client - SimpleMessenger";
                while (string.IsNullOrWhiteSpace(username))
                {
                    Console.WriteLine("Username:");
                    Console.Write("> ");
                    username = Console.ReadLine().Trim();
                }
                ConnectLoop();
                Thread thread = new Thread(ReceiveResponse);
                thread.Start();
                while (true)
                {
                    GetInput();
                }
            }
        }

        public static void GetInput()
        {
            Console.Write("> ");
            string userInput = Console.ReadLine().Trim();


            if (userInput.StartsWith("./"))
            {
                EnterCommand(userInput.Replace("./", null).Trim());
            }
            else
            {
                UploadEncryptedString(userInput);
            }
        }

        /// <summary>
        /// Usage of slash '/' commands.
        /// </summary>
        /// <param name="command">Command (without prefix)</param>
        public static void EnterCommand(string command)
        {
            string initialCommand = command.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries)[0];
            List<string> commandParams = command.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();

            switch (initialCommand)
            {
                case "fore":
                    switch (commandParams[1])
                    {
                        case "yellow":
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;

                        case "red":
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                        
                        case "cyan":
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            break;

                        case "blue":
                            Console.ForegroundColor = ConsoleColor.Blue;
                            break;

                        case "green" or "lime":
                            Console.ForegroundColor = ConsoleColor.Green;
                            break;

                        case "white" or "reset":
                            Console.ResetColor();   
                            break;

                        default:
                            break;
                    }
                    break;

                case "cls" or "clear":
                    Console.Clear();
                    break;

                case "exit": //exit gracefully
                    _clientSocket.Shutdown(SocketShutdown.Both); //dont crash the server when leacing PROPERLY
                    _clientSocket.Close(); //close the socket
                    Environment.Exit(0); //safely exit the app
                    break;

                default:
                    Console.WriteLine("Unknown command.");
                    GetInput();
                    break;
            }
        }

        /// <summary>
        /// Attempt to connect to the server until succeeded.
        /// </summary>
        public static int connectionAttempts = 0;
        public static void ConnectLoop()
        {
            while (!_clientSocket.Connected)
            {
                try
                {
                    _clientSocket.Connect(IPAddress.Loopback, serverPort);
                }
                catch (SocketException ex)
                {
                    connectionAttempts += 1;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.WriteLine($"Connection attempts: {connectionAttempts}\n");
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Send an ASCII encrypted message in bytes to the server.
        /// </summary>
        /// <param name="text">Text to send and encode.</param>
        public static void UploadEncryptedString(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(FormatSentMessage(text));
            _clientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        /// <summary>
        /// Receive responses from the server.
        /// </summary>
        public static void ReceiveResponse()
        {
            while (true)
            {
                byte[] buffer = new byte[2048];
                int received = _clientSocket.Receive(buffer, SocketFlags.None);

                if (received == 0) { return; } //empty response; quit

                byte[] data = new byte[received];
                Array.Copy(buffer, data, received);
                string text = Encoding.ASCII.GetString(data);

                Console.WriteLine(text);

                Thread.Sleep(100);
            }
        }

        public static string FormatSentMessage(string text)
        {
            string message =
                $"@u/{username}\t\t{DateTime.Now.ToLongTimeString()}\n" +
                $"{text}\n";

            return message;
        }
    }
}