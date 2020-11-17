using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace IrcServer
{
    class ClientTcpHandler
    {
        public ClientTcpHandler(TcpClient tcpClient, Action<MessageType, string> newMessageCb)
        {
            m_client = tcpClient;
            m_newMessageCb = newMessageCb;
            m_stream = m_client.GetStream();

            m_connectionValid = true;
            m_receiveMessages = new Thread(ReceiveMessages);
            m_receiveMessages.Start();
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

                    m_newMessageCb(MessageType.Standard, Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));
                }
                catch (SystemException)
                {
                    m_connectionValid = false;
                    m_newMessageCb(MessageType.ConnectionClosed, "");
                }
            }
        }

        private TcpClient m_client;
        private NetworkStream m_stream;
        private Thread m_receiveMessages;
        private Action<MessageType, string> m_newMessageCb;
        bool m_connectionValid;
    }
}
