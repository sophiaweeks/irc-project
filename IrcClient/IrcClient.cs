﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace IrcClient
{    
    class IrcClient
    {
        public IrcClient(string server, int port)
        {
            m_tcpHandler = new TcpHandler(server, port, msg => NewMessage(msg));
            m_processQueue = new Thread(ProcessQueue);
            m_processQueue.Start();
        }

        ~IrcClient()
        {
            m_processQueue.Abort();
        }

        public void Register(string nickname)
        {
            string message = "NICK " + nickname + " CR LF";
            m_tcpHandler.SendMessage(message);
        }

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
                    MessageProcessor.ProcessMessage(message);
                }
            }
        }

        private void NewMessage(Message msg)
        {
            lock (m_messageQueue)
            {
                m_messageQueue.Enqueue(msg);
                Monitor.Pulse(m_messageQueue);
            }
        }

        private TcpHandler m_tcpHandler;
        private Queue<Message> m_messageQueue = new Queue<Message>();
        private Thread m_processQueue;
    }
}
