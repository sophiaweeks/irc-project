﻿using System;
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
            Console.WriteLine("Welcome to IRC!");
            Console.WriteLine("Connecting to server...");

            IrcClient client;
            try
            {
                client = new IrcClient(SERVER_IP, PORT_NO);
            }
            catch(Exception)
            {
                Console.WriteLine("Connection failed - the server may not be running");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Connection successful!");
            Console.WriteLine("Press --h for a list of commands. Hint: start by registering under a nickname.");
            Console.WriteLine();

            RunProgram(client);

            Console.WriteLine();
            Console.WriteLine("Goodbye!");
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
                    Console.WriteLine("Unrecognized command.");
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
                    default:
                        Console.WriteLine("Unrecognized command.");
                        break;
                }
            }
        }

        static private void PrintCommands()
        {
            Console.WriteLine("List of IRC commands:");
            Console.WriteLine("  --h: print list of commands");
            Console.WriteLine("  --q: quit IRC");
            Console.WriteLine("  --r <nickname>: register under desired nickname");
            Console.WriteLine("  --c <room name>: create a new room with the desired name");
            Console.WriteLine("  --j <room name>: join the room with the specified name");
            Console.WriteLine("  --l <room name>: leave the room with the specified name");
            Console.WriteLine("  --m <room name> <text>: send a message to the room with the specified name");
        }

        static private void Quit(IrcClient client)
        {
            client.Quit();
        }

        static private void TryRegister(IrcClient client, string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Must include a nickname");
                return;
            }

            if (args[0].Contains(" "))
            {
                Console.WriteLine("Nickname can't contain spaces");
                return;
            }

            var nickname = args[0];
            client.Register(nickname);
        }

        static private void TryCreate(IrcClient client, string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Must include a room name");
                return;
            }

            if (args[0].Contains(" "))
            {
                Console.WriteLine("Room name can't contain spaces");
                return;
            }

            var roomname = args[0];
            client.Create(roomname);
        }

        static private void TryJoin(IrcClient client, string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Must include a room name");
                return;
            }

            if (args[0].Contains(" "))
            {
                Console.WriteLine("Room name can't contain spaces");
                return;
            }

            var roomname = args[0];
            client.Join(roomname);
        }

        static private void TryPart(IrcClient client, string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Must include a room name");
                return;
            }

            if (args[0].Contains(" "))
            {
                Console.WriteLine("Room name can't contain spaces");
                return;
            }

            var roomname = args[0];
            client.Part(roomname);
        }

        static private void TryMessage(IrcClient client, string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Must include a room name and text");
                return;
            }

            if (args[0].Contains(" "))
            {
                Console.WriteLine("Room name can't contain spaces");
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
    }
}
