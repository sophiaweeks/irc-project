using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace IrcClient
{
    class Program
    {
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";
        static void Main(string[] args)
        {
            Console.WriteLine("Creating client.");
            IrcClient client = new IrcClient(SERVER_IP, PORT_NO);

            while (true)
            {
                var msg = Console.ReadLine();
                client.SendMessage(msg);
            }
        }
    }
}
