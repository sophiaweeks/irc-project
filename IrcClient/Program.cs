using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;
using System.Text.RegularExpressions;

namespace IrcClient
{
    class Program
    {
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";
        static void Main(string[] args)
        {
            ConsoleWriter.WriteToConsole(TextType.Program, "Welcome to IRC!");
            ConsoleWriter.WriteToConsole(TextType.Program, "Connecting to server...");

            IrcClient client;
            try
            {
                client = new IrcClient(SERVER_IP, PORT_NO);
            }
            catch(Exception)
            {
                ConsoleWriter.WriteToConsole(TextType.Program, "Connection failed - the server may not be running");
                Console.ReadLine();
                return;
            }

            ConsoleWriter.WriteToConsole(TextType.Program, "Connection successful!");
            ConsoleWriter.WriteToConsole(TextType.Program, "Press --h for a list of commands. Hint: start by registering under a nickname.");
            Console.WriteLine();

            RunProgram(client);

            Console.WriteLine();
            ConsoleWriter.WriteToConsole(TextType.Program, "Goodbye!");
            Thread.Sleep(1000);

            Environment.Exit(0);
        }

        static private void RunProgram(IrcClient client)
        {
            bool quit = false;
            while (!quit)
            {
                var msg = Console.ReadLine();

                var msgParts = Regex.Matches(msg, "[^\\s\"']+|\"([^\"]*)\"|'([^']*)'")
                    .Cast<Match>().Select(iMatch => iMatch.Value.Replace("\"", "").Replace("'", "")).ToArray();

                if (msgParts.Count() < 1)
                {
                    ConsoleWriter.WriteToConsole(TextType.Program, "Unrecognized command.");
                    continue;
                }

                var command = msgParts[0];

                switch (command)
                {
                    case "--h":
                        PrintCommands();
                        break;
                    case "--q":
                        Quit(client);
                        quit = true;
                        break;
                    case "--r":
                        TryRegister(client, msgParts.Skip(1).ToArray());
                        break;
                    case "--c":
                        TryCreate(client, msgParts.Skip(1).ToArray());
                        break;
                    case "--j":
                        TryJoin(client, msgParts.Skip(1).ToArray());
                        break;
                    case "--p":
                        TryPart(client, msgParts.Skip(1).ToArray());
                        break;
                    case "--m":
                        TryMessage(client, msgParts.Skip(1).ToArray());
                        break;
                    case "--l":
                        TryList(client);
                        break;
                    case "--n":
                        TryNames(client, msgParts.Skip(1).ToArray());
                        break;
                    default:
                        Console.WriteLine("Unrecognized command.");
                        break;
                }
            }
        }

        static private void PrintCommands()
        {
            ConsoleWriter.WriteToConsole(TextType.Program, "List of IRC commands:");
            ConsoleWriter.WriteToConsole(TextType.Program, "  --h: print list of commands");
            ConsoleWriter.WriteToConsole(TextType.Program, "  --q: quit IRC");
            ConsoleWriter.WriteToConsole(TextType.Program, "  --r <nickname>: register under desired nickname");
            ConsoleWriter.WriteToConsole(TextType.Program, "  --c <room name>: create a new room with the desired name");
            ConsoleWriter.WriteToConsole(TextType.Program, "  --j <room name>: join the room with the specified name");
            ConsoleWriter.WriteToConsole(TextType.Program, "  --P <room name>: leave the room with the specified name");
            ConsoleWriter.WriteToConsole(TextType.Program, "  --m <room name> <text>: send a message to the room with the specified name");
            ConsoleWriter.WriteToConsole(TextType.Program, "  --l: list all open rooms");
            ConsoleWriter.WriteToConsole(TextType.Program, "  --n <room name>: list members in the specified room");
        }

        static private void Quit(IrcClient client)
        {
            client.Quit();
        }

        static private void TryRegister(IrcClient client, string[] args)
        {
            if (args.Length < 1)
            {
                ConsoleWriter.WriteToConsole(TextType.Program, "Must include a nickname");
                return;
            }

            if (args[0].Contains(" "))
            {
                ConsoleWriter.WriteToConsole(TextType.Program, "Nickname can't contain spaces");
                return;
            }

            var nickname = args[0];
            client.Register(nickname);
        }

        static private void TryCreate(IrcClient client, string[] args)
        {
            if (args.Length < 1)
            {
                ConsoleWriter.WriteToConsole(TextType.Program, "Must include a room name");
                return;
            }

            if (args[0].Contains(" "))
            {
                ConsoleWriter.WriteToConsole(TextType.Program, "Room name can't contain spaces");
                return;
            }

            var roomname = args[0];
            client.Create(roomname);
        }

        static private void TryJoin(IrcClient client, string[] args)
        {
            if (args.Length < 1)
            {
                ConsoleWriter.WriteToConsole(TextType.Program, "Must include a room name");
                return;
            }

            if (args[0].Contains(" "))
            {
                ConsoleWriter.WriteToConsole(TextType.Program, "Room name can't contain spaces");
                return;
            }

            var roomname = args[0];
            client.Join(roomname);
        }

        static private void TryPart(IrcClient client, string[] args)
        {
            if (args.Length < 1)
            {
                ConsoleWriter.WriteToConsole(TextType.Program, "Must include a room name");
                return;
            }

            if (args[0].Contains(" "))
            {
                ConsoleWriter.WriteToConsole(TextType.Program, "Room name can't contain spaces");
                return;
            }

            var roomname = args[0];
            client.Part(roomname);
        }

        static private void TryMessage(IrcClient client, string[] args)
        {
            if (args.Length < 2)
            {
                ConsoleWriter.WriteToConsole(TextType.Program, "Must include a room name and text");
                return;
            }

            if (args[0].Contains(" "))
            {
                ConsoleWriter.WriteToConsole(TextType.Program, "Room name can't contain spaces");
                return;
            }

            var roomname = args[0];
            var text = args[1];
            client.Message(roomname, text);
        }

        static private void TryList(IrcClient client)
        {
            client.List();
        }

        static private void TryNames(IrcClient client, string[] args)
        {
            if (args.Length < 1)
            {
                ConsoleWriter.WriteToConsole(TextType.Program, "Must include a room name and text");
                return;
            }

            if (args[0].Contains(" "))
            {
                ConsoleWriter.WriteToConsole(TextType.Program, "Room name can't contain spaces");
                return;
            }

            var roomname = args[0];
            client.Names(roomname);
        }
    }
}
