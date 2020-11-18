using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace IrcClient
{
    class TcpHandler
    {
        public TcpHandler(string server, int port, Action<Message> newMessageCb)
        {
            m_connectionValid = true;

            m_newMessageCb = newMessageCb;
            m_client = new TcpClient(server, port);
            m_stream = m_client.GetStream();

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

                    m_newMessageCb(new Message(MessageType.Standard, Encoding.ASCII.GetString(bytesToRead, 0, bytesRead)));
                }
                catch (Exception)
                {
                    m_connectionValid = false;

                    m_newMessageCb(new Message(MessageType.ConnectionClosed, ""));
                }
            }
        }

        private Action<Message> m_newMessageCb;
        private TcpClient m_client;
        private NetworkStream m_stream;
        private Thread m_receiveMessages;
        private bool m_connectionValid;
    }
}
