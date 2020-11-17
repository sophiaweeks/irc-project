using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

namespace IrcServer
{
    class ServerTcpHandler
    {
        public ServerTcpHandler(string ip, int port, Action<TcpClient> newClientCb)
        {
            IPAddress localAdd = IPAddress.Parse(ip);
            m_listener = new TcpListener(localAdd, port);
            Console.WriteLine("Listening...");
            m_listener.Start();

            m_newClientCb = newClientCb;

            m_receiveClients = new Thread(ReceiveClients);
            m_receiveClients.Start();
        }

        ~ServerTcpHandler()
        {
            m_listener.Stop();
            m_receiveClients.Abort();
        }

        private void ReceiveClients()
        {
            while (true)
            {
                TcpClient tcpClient = m_listener.AcceptTcpClient();
                m_newClientCb(tcpClient);
            }
        }

        private Thread m_receiveClients;
        private TcpListener m_listener;
        private Action<TcpClient> m_newClientCb;
    }
}
