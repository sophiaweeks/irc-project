using System;
using System.Collections.Generic;
using System.Text;
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

    static class MessageProcessor
    {
        static public void ProcessMessage(Message message)
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

        static private void ParseMessage(Message message)
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

                switch (command)
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

        static private void Handle409(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var nickname = arguments[0];
            Console.WriteLine("ERROR: Command \"NICK\" failed due to erroneous nickname: {0}", nickname);
        }

        static private void Handle410(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var nickname = arguments[0];
            Console.WriteLine("ERROR: Command \"NICK\" failed due to nickname in use: {0}", nickname);
        }

        static private void Handle412(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var command = arguments[0];
            Console.WriteLine("ERROR: Command {0} failed due to not enough params: {0}", command);
        }

        static private void Handle413(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var nickname = arguments[0];
            Console.WriteLine("ERROR: Command \"NICK\" failed due already registered under nickname: {0}", nickname);
        }
    }
}
