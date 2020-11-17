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
        }

        static private void TryRegister(IrcClient client, string message)
        {
            var pieces = message.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (pieces.Length < 2)
            {
                Console.WriteLine("Must include a nickname");
            }

            if (pieces.Length > 2)
            {
                Console.WriteLine("Nickname can't contain spaces");
            }

            var nickname = pieces[1];
            client.Register(nickname);
        }
    }
}
