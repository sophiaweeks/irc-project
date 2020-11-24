using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

namespace IrcServer
{
    class IrcServer : IIrcServer, IMessageHandler
    {
        public IrcServer(string ip, int port)
        {
            m_tcpHandler = new ServerTcpHandler(ip, port, client => AddClient(client));
            m_messageProcessor = new MessageProcessor(this);

            m_processQueue = new Thread(ProcessQueue);
            m_processQueue.Start();
        }

        ~IrcServer()
        {
            m_processQueue.Abort();
        }

        // IMessageHandler
        public void HandleMessage(Message message)
        {
            lock(m_messageQueue)
            {
                m_messageQueue.Enqueue(message);
                Monitor.Pulse(m_messageQueue);
            }
        }

        // IIrcServer
        public bool IsNicknameInUse(string nickname)
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

        public bool IsRoomNameInUse(string roomname)
        {
            foreach (Room room in m_rooms)
            {
                if (room.GetName() == roomname)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetNumRooms()
        {
            return m_rooms.Count();
        }

        public void AddRoom(string roomname)
        {
            m_rooms.Add(new Room(roomname));
        }

        public Room GetRoom(string roomname)
        {
            foreach (Room r in m_rooms)
            {
                if (r.GetName() == roomname)
                {
                    return r;
                }
            }

            return null;
        }

        // private methods
        private void ProcessQueue()
        {
            while (true)
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
                    m_messageProcessor.ParseMessage(message);
                    break;
            }
        }

        private void AddClient(TcpClient client)
        {
            lock (m_clients)
            {
                m_clients.Add(new IrcClient(client, this));
            }
        }

        private void RemoveClient(IrcClient removeClient)
        {
            lock (m_clients)
            {
                m_clients.Remove(removeClient);
            }
        }

        private ServerTcpHandler m_tcpHandler;
        private MessageProcessor m_messageProcessor;
        private List<IrcClient> m_clients = new List<IrcClient>();
        private List<Room> m_rooms = new List<Room>();
        private Queue<Message> m_messageQueue = new Queue<Message>();
        private Thread m_processQueue;

    }
}
