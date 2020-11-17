using System;
using System.Collections.Generic;
using System.Text;

namespace IrcServer
{
    enum MessageType
    {
        ConnectionClosed,
        Standard,
    }

    class Message
    {
        public Message(MessageType type, IrcClient client, string contents)
        {
            Type = type;
            Client = client;
            Contents = contents;
        }
        
        public MessageType Type;
        public IrcClient Client;
        public string Contents;
    }
    
    interface IMessageHandler
    {
        void HandleMessage(Message message);
    }
}
