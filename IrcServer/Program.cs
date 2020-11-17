using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace IrcServer
{
    class Program
    {
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";

        static void Main(string[] args)
        {
            IrcServer server;
            try
            {
                server = new IrcServer(SERVER_IP, PORT_NO);
            }
            catch(Exception)
            {
                Console.WriteLine("Could not start the server. Please try again later.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("The IRC server is operational!");
            Console.ReadLine();
        }
    }
}
