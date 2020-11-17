using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

namespace IrcClient
{
    enum MessageType
    {
        ConnectionClosed,
        Standard,
    }
    class Message
    {
        public Message(MessageType type, string contents)
        {
            Type = type;
            Contents = contents;
        }

        public MessageType Type;
        public string Contents;
    }
    
    class IrcClient
    {
        public IrcClient(string server, int port)
        {
            m_connectionValid = true;
            
            m_client = new TcpClient(server, port);
            m_stream = m_client.GetStream();

            m_processQueue = new Thread(ProcessQueue);
            m_receiveMessages = new Thread(ReceiveMessages);

            m_processQueue.Start();
            m_receiveMessages.Start();
        }

        ~IrcClient()
        {
            m_client.Close();
            m_receiveMessages.Join();
        }

        public void Register(string nickname)
        {
            string message = "NICK " + nickname + " CR LF";
            SendMessage(message);
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
                    ProcessMessage(message);
                }
            }
        }

        private void ProcessMessage(Message message)
        {
            switch (message.Type)
            {
                case MessageType.ConnectionClosed:
                    Console.WriteLine("ERROR: The connection to the server closed unexpectedly.  Our apologies.");
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

                switch(command)
                {
                    case "409":
                        Handle409(pieces.Skip(1).ToArray());
                        break;
                    case "410":
                        Handle410(pieces.Skip(1).ToArray());
                        break;
                    case "412":
                        Handle412(pieces.Skip(1).ToArray());
                        break;
                    case "413":
                        Handle413(pieces.Skip(1).ToArray());
                        break;
                }
            }
        }

        private void Handle409(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var nickname = arguments[0];
            Console.WriteLine("ERROR: Command \"NICK\" failed due to erroneous nickname: {0}", nickname);
        }

        private void Handle410(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var nickname = arguments[0];
            Console.WriteLine("ERROR: Command \"NICK\" failed due to nickname in use: {0}", nickname);
        }

        private void Handle412(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var command = arguments[0];
            Console.WriteLine("ERROR: Command {0} failed due to not enough params: {0}", command);
        }

        private void Handle413(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var nickname = arguments[0];
            Console.WriteLine("ERROR: Command \"NICK\" failed due already registered under nickname: {0}", nickname);
        }

        private int SendMessage(string msg)
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

                    lock(m_messageQueue)
                    {
                        m_messageQueue.Enqueue(new Message(MessageType.Standard, Encoding.ASCII.GetString(bytesToRead, 0, bytesRead)));
                        Monitor.Pulse(m_messageQueue);
                    }
                }
                catch(Exception)
                {
                    m_connectionValid = false;

                    lock(m_messageQueue)
                    {
                        m_messageQueue.Enqueue(new Message(MessageType.ConnectionClosed, ""));
                        Monitor.Pulse(m_messageQueue);
                    }
                }
            }
        }

        private TcpClient m_client;
        private NetworkStream m_stream;
        private Thread m_receiveMessages;
        private bool m_connectionValid;

        private Queue<Message> m_messageQueue = new Queue<Message>();
        private Thread m_processQueue;
    }
}
