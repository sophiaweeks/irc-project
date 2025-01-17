﻿using System;
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
        public void Exit()
        {
            lock (m_messageQueue)
            {
                m_messageQueue.Enqueue(new Message(MessageType.Exit, null, ""));
                Monitor.Pulse(m_messageQueue);
            }
        }

        public void HandleMessage(Message message)
        {
            lock(m_messageQueue)
            {
                m_messageQueue.Enqueue(message);
                Monitor.Pulse(m_messageQueue);
            }
        }

        // IIrcServer
        public void RemoveClient(IrcClient client)
        {
            InternalRemoveClient(client);
        }

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
            Console.WriteLine("Created room {0}", roomname);
        }

        public void RemoveRoom(Room room)
        {
            m_rooms.Remove(room);
            Console.WriteLine("Room {0} was destroyed", room.GetName());
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

        public List<Room> GetRooms()
        {
            return m_rooms;
        }

        // private methods
        private void ProcessQueue()
        {
            while (true)
            {
                Message message = null;
                lock (m_messageQueue)
                {
                    while (m_messageQueue.Count < 1)
                    {
                        Monitor.Wait(m_messageQueue);
                    }
                    message = m_messageQueue.Dequeue();
                }
                
                ProcessMessage(message);
            }
        }

        private void ProcessMessage(Message message)
        {
            switch (message.Type)
            {
                case MessageType.Exit:
                    InternalExit();
                    break;
                case MessageType.ConnectionClosed:
                    Console.WriteLine("Client {0} closed connection unexpectedly", message.Client.IsRegistered() ? message.Client.GetNickname() : message.Client.ToString());
                    InternalRemoveClient(message.Client);
                    break;
                case MessageType.Standard:
                    m_messageProcessor.ParseMessage(message);
                    break;
            }
        }

        private void InternalExit()
        {
            List<IrcClient> copyClientList;
            lock (m_clients)
            {
                copyClientList = m_clients;
            }

            foreach (IrcClient c in copyClientList)
            {
                c.SendMessage("312 CR LF"); //RPL_SERVERCLOSED
            }
        }

        private void AddClient(TcpClient client)
        {
            lock (m_clients)
            {
                m_clients.Add(new IrcClient(client, this));
                Console.WriteLine("New client added");
            }
        }

        private void InternalRemoveClient(IrcClient removeClient)
        {
            var removeRooms = new List<Room>();
            foreach (Room r in m_rooms)
            {
                if (r.Members.Contains(removeClient))
                {
                    r.Members.Remove(removeClient);
                    if (r.Members.Count == 0)
                    {
                        removeRooms.Add(r);
                    }
                    else
                    {
                        string notification = String.Format("309 {0} {1} CR LF", r.GetName(), removeClient.GetNickname()); //RPL_LEAVESUCCEEDED
                        r.SendMessage(notification);
                    }
                    Console.WriteLine("{0} left room {1}", removeClient.GetNickname(), r.GetName());
                }
            }

            foreach (Room r in removeRooms)
            {
                m_rooms.Remove(r);
                Console.WriteLine("Room {0} was destroyed", r.GetName());
            }
            
            lock (m_clients)
            {
                m_clients.Remove(removeClient);
                Console.WriteLine("Client {0} disconnected", removeClient.IsRegistered() ? removeClient.GetNickname() : removeClient.ToString());
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
