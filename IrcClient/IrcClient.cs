using System;
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
            m_client = new TcpClient(server, port);
            m_stream = m_client.GetStream();

            m_receiveMessages = new Thread(ReceiveMessages);
            m_receiveMessages.Start();
        }

        ~IrcClient()
        {
            m_client.Close();
            m_receiveMessages.Join();
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
            while (true)
            {
                byte[] bytesToRead = new byte[m_client.ReceiveBufferSize];
                int bytesRead = m_stream.Read(bytesToRead, 0, m_client.ReceiveBufferSize);
                Console.WriteLine("Received : " + Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));
            }
        }

        private TcpClient m_client;
        private NetworkStream m_stream;
        private Thread m_receiveMessages;

    }
}
