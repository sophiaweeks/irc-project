using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

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
                    ParseMessage(message);
                    break;
            }
        }

        private void ParseMessage(Message message)
        {
            var contents = message.Contents;
            var messages = contents.Split("CR LF", StringSplitOptions.RemoveEmptyEntries);

            foreach (var m in messages)
            {
                var pieces = m.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (pieces.Count() < 1)
                {
                    return;
                }

                var command = pieces[0];

                if (command == "NICK")
                {
                    HandleNick(pieces.Skip(1).ToArray(), message.Client);
                }
                else
                {
                    HandleUnknownCommand(command, message.Client);
                }
            }
        }

        private void HandleNick(string[] arguments, IrcClient client)
        {
            if (client.IsRegistered())
            {
                string msg = String.Format("413 {0} CR LF", client.GetNickname()); //ERR_ALREADYREGISTERED
                client.SendMessage(msg);
                return;
            }
            
            if (arguments.Count() < 1)
            {
                string msg = "412 NICK CR LF"; //ERR_NEEDMOREPARAMS
                client.SendMessage(msg);
                return;
            }

            var nickname = arguments[0];

            if (nickname.Length > MAX_NICKNAME_LENGTH)
            {
                string msg = String.Format("409 {0} CR LF", nickname); //ERR_ERONEOUSNICKNAME
                client.SendMessage(msg);
                return;
            }

            if (IsNicknameInUse(nickname))
            {
                string msg = String.Format("410 {0} CR LF", nickname); //ERR_NICKNAMEINUSE
                client.SendMessage(msg);
                return;
            }

            client.Register(nickname);
        }


        private void HandleUnknownCommand(string command, IrcClient client)
        {
            string msg = String.Format("408 {0} CR LF", command); //ERR_UNKNOWNCOMMAND
            client.SendMessage(msg);
        }

        private bool IsNicknameInUse(string nickname)
        {
            List<IrcClient> copyClientList;
            lock (m_clients)
            {
                copyClientList = m_clients;
            }

            foreach (IrcClient client in copyClientList)
            {
                if (client.IsRegistered() && client.GetNickname() == nickname)
                {
                    return true;
                }
            }

            return false;
        }

        private void ReceiveClients()
        {
            while (!m_exit)
            {
                TcpClient tcpClient = m_listener.AcceptTcpClient();

                IrcClient client = new IrcClient(tcpClient, this);
                lock (m_clients)
                {
                    m_clients.Add(client);
                }
            }
        }

        //private void NotifyClients(string msg)
        //{
        //    List<IrcClient> copyClientList;
        //    lock(m_clients)
        //    {
        //        copyClientList = m_clients;
        //    }

        //    foreach (IrcClient c in copyClientList)
        //    {
        //        c.SendMessage(msg);
        //    }
        //}

        private void RemoveClient(IrcClient removeClient)
        {
            lock (m_clients)
            {
                m_clients.Remove(removeClient);
            }
            //NotifyClients("Client left :(");
        }

        private bool m_exit;
        private Thread m_receiveClients;
        private TcpListener m_listener;
        private List<IrcClient> m_clients = new List<IrcClient>();
        
        private Queue<Message> m_messageQueue = new Queue<Message>();
        private Thread m_processQueue;

        private const int MAX_NICKNAME_LENGTH = 9;
    }
}
