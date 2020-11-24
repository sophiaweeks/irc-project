using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;

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

            while (true)
            {
                var msg = Console.ReadLine();
                if (msg.Length < 3)
                {
                    Console.WriteLine("Unrecognized command.");
                    continue;
                }

                var command = msg.Substring(0, 3);

                switch(command)
                {
                    case "--h":
                        PrintCommands();
                        break;
                    case "--r":
                        TryRegister(client, msg);
                        break;
                    case "--c":
                        TryCreate(client, msg);
                        break;
                    case "--j":
                        TryJoin(client, msg);
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
            Console.WriteLine("  --r <nickname>: register under desired nickname");
            Console.WriteLine("  --c <room name>: create a new room with the desired name");
            Console.WriteLine("  --j <room name>: join the room with the specified name");
        }

        static private void TryRegister(IrcClient client, string message)
        {
            var pieces = message.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (pieces.Length < 2)
            {
                Console.WriteLine("Must include a nickname");
                return;
            }

            if (pieces.Length > 2)
            {
                Console.WriteLine("Nickname can't contain spaces");
                return;
            }

            var nickname = pieces[1];
            client.Register(nickname);
        }

        static private void TryCreate(IrcClient client, string message)
        {
            var pieces = message.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (pieces.Length < 2)
            {
                Console.WriteLine("Must include a room name");
                return;
            }

            if (pieces.Length > 2)
            {
                Console.WriteLine("Room name can't contain spaces");
                return;
            }

            var roomname = pieces[1];
            client.Create(roomname);
        }

        static private void TryJoin(IrcClient client, string message)
        {
            var pieces = message.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (pieces.Length < 2)
            {
                Console.WriteLine("Must include a room name");
                return;
            }

            if (pieces.Length > 2)
            {
                Console.WriteLine("Room name can't contain spaces");
                return;
            }

            var roomname = pieces[1];
            client.Join(roomname);
        }
    }
}
