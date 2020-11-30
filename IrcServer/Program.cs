using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace IrcServer
{
    class Program
    {
        const int DEFAULT_PORT_NO = 5000;
        const string DEFAULT_SERVER_IP = "127.0.0.1";

        static void Main(string[] args)
        {
            string server_ip = DEFAULT_SERVER_IP;
            int port_no = DEFAULT_PORT_NO;
            if (args.Count() >= 1)
            {
                server_ip = args[0].ToString();
            }
            if (args.Count() >= 2)
            {
                port_no = int.Parse(args[1]);
            }
            
            IrcServer server;
            try
            {
                server = new IrcServer(server_ip, port_no);
            }
            catch(Exception)
            {
                Console.WriteLine("Could not start the server. Please try again later.");
                Environment.Exit(0);
                return;
            }

            Console.WriteLine("The IRC server is operational on {0}:{1}!", server_ip, port_no);
            Console.WriteLine();
            Console.ReadLine();

            Console.WriteLine("Shutting down the server.");
            server.Exit();
            Environment.Exit(0);
        }
    }
}
