using System;
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
            m_client = tcpClient;
            m_messageHandler = handler;
            m_stream = m_client.GetStream();

            m_connectionValid = true;
            m_receiveMessages = new Thread(ReceiveMessages);
            m_receiveMessages.Start();

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

        public int SendMessage(string msg)
        {
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(msg);
            try
            {
                m_stream.Write(bytesToSend, 0, bytesToSend.Length);
                return 0;
            }
            catch (SystemException)
            {
                return 1;
            }
        }

        private void ReceiveMessages()
        {
            while (m_connectionValid)
            {
                try
                {
                    byte[] bytesToRead = new byte[m_client.ReceiveBufferSize];
                    int bytesRead = m_stream.Read(bytesToRead, 0, m_client.ReceiveBufferSize);

                    m_messageHandler.HandleMessage(new Message(MessageType.Standard, this, Encoding.ASCII.GetString(bytesToRead, 0, bytesRead)));
                }
                catch (SystemException)
                {
                    m_connectionValid = false;
                    m_messageHandler.HandleMessage(new Message(MessageType.ConnectionClosed, this, ""));
                }
            }
        }

        private TcpClient m_client;
        private NetworkStream m_stream;
        private Thread m_receiveMessages;
        private IMessageHandler m_messageHandler;
        bool m_connectionValid;

        bool m_registered;
        string m_nickname;
    }
}
