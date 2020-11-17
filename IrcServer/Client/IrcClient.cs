﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace IrcServer
{
    class IrcClient
    {
        public IrcClient(TcpClient tcpClient, IMessageHandler handler)
        {
            m_tcpHandler = new ClientTcpHandler(tcpClient, (type, msg) => 
                handler.HandleMessage(new Message(type, this, msg)));
            m_registered = false;
        }

        public bool IsRegistered()
        {
            lock (this)
            {
                return m_registered;
            }
        }

        public void Register(string nickname)
        {
            lock(this)
            {
                m_nickname = nickname;
                m_registered = true;
            }
        }

        public string GetNickname()
        {
            return m_nickname;
        }

        public void SendMessage(string msg)
        {
            m_tcpHandler.SendMessage(msg);
        }

        private ClientTcpHandler m_tcpHandler;
        private bool m_registered;
        private string m_nickname;
    }
}