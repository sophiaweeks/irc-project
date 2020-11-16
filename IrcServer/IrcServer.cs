using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography.X509Certificates;

namespace IrcServer
{
    class IrcServer : IMessageHandler
    {
        public IrcServer(string ip, int port)
        {
            IPAddress localAdd = IPAddress.Parse(ip);
            m_listener = new TcpListener(localAdd, port);
            Console.WriteLine("Listening...");
            m_listener.Start();

            m_exit = false;
            m_processQueue = new Thread(ProcessQueue);
            m_receiveClients = new Thread(ReceiveClients);

            m_processQueue.Start();
            m_receiveClients.Start();
        }

        ~IrcServer()
        {
            m_listener.Stop();
            m_exit = true;
            m_receiveClients.Join();
        }

        public void SendMessage(Message message)
        {
            lock(m_messageQueue)
            {
                m_messageQueue.Enqueue(message);
                Monitor.Pulse(m_messageQueue);
            }
        }

        private void ProcessQueue()
        {
            while (!m_exit)
            {
                Message message = null;
                lock (m_messageQueue)
                {
                    Monitor.Wait(m_messageQueue);
                    if (m_messageQueue.Count > 0)
                    {
                        message = m_messageQueue.Dequeue();
                    }
                }

                if (message != null)
                {
                    ProcessMessage(message);
                }
            }
        }

        private void ProcessMessage(Message message)
        {
            switch (message.Type)
            {
                case MessageType.ConnectionClosed:
                    RemoveClient(message.Client);
                    break;
                case MessageType.Standard:
                    NotifyClients(message.Contents);
                    break;
            }
        }

        private void ReceiveClients()
        {
            while (!m_exit)
            {
                TcpClient tcpClient = m_listener.AcceptTcpClient();
                Console.WriteLine("Client added!");

                IrcClient client = new IrcClient(tcpClient, this);
                if (client.SendMessage("You are added.") == 0)
                {
                    lock (m_clients)
                    {
                        m_clients.Add(client);
                    }
                    NotifyClients("New client added!");
                }
            }
        }

        private void NotifyClients(string msg)
        {
            List<IrcClient> copyClientList;
            lock(m_clients)
            {
                copyClientList = m_clients;
            }

            foreach (IrcClient c in copyClientList)
            {
                c.SendMessage(msg);
            }
        }

        private void RemoveClient(IrcClient removeClient)
        {
            lock (m_clients)
            {
                m_clients.Remove(removeClient);
            }
            NotifyClients("Client left :(");
        }

        private bool m_exit;
        private Thread m_receiveClients;
        private TcpListener m_listener;
        private List<IrcClient> m_clients = new List<IrcClient>();
        
        private Queue<Message> m_messageQueue = new Queue<Message>();
        private Thread m_processQueue;
    }
}
