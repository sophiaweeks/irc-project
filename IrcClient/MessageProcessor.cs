using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection.Metadata;

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
                    case "306":
                        Handle306(pieces.Skip(1).ToArray());
                        break;
                    case "307":
                        Handle307(pieces.Skip(1).ToArray());
                        break;
                    case "308":
                        Handle308(pieces.Skip(1).ToArray());
                        break;
                    case "402":
                        Handle402(pieces.Skip(1).ToArray());
                        break;
                    case "404":
                        Handle404(pieces.Skip(1).ToArray());
                        break;
                    case "405":
                        Handle405(pieces.Skip(1).ToArray());
                        break;
                    case "406":
                        Handle406(pieces.Skip(1).ToArray());
                        break;
                    case "407":
                        Handle407(pieces.Skip(1).ToArray());
                        break;
                    case "408":
                        Handle408(pieces.Skip(1).ToArray());
                        break;
                    case "409":
                        Handle409(pieces.Skip(1).ToArray());
                        break;
                    case "410":
                        Handle410(pieces.Skip(1).ToArray());
                        break;
                    case "411":
                        Handle411(pieces.Skip(1).ToArray());
                        break;
                    case "412":
                        Handle412(pieces.Skip(1).ToArray());
                        break;
                    case "413":
                        Handle413(pieces.Skip(1).ToArray());
                        break;
                    case "414":
                        Handle414(pieces.Skip(1).ToArray());
                        break;
                }
            }
        }

        static private void Handle306(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var nickname = arguments[0];
            Console.WriteLine("Successfully registered under nickname: {0}", nickname);
        }

        static private void Handle307(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var roomname = arguments[0];
            Console.WriteLine("Successfully created room: {0}", roomname);
        }

        static private void Handle308(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var roomname = arguments[0];
            Console.WriteLine("Successfully joined room: {0}", roomname);
        }

        static private void Handle402(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var roomname = arguments[0];
            Console.WriteLine("ERROR: Couldn't join room {0} due to no such room", roomname);
        }

        static private void Handle404(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var roomname = arguments[0];
            Console.WriteLine("ERROR: Couldn't join room {0} due to already joined", roomname);
        }

        static private void Handle405(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var roomname = arguments[0];
            Console.WriteLine("ERROR: Couldn't create room {0} due to too many rooms", roomname);
        }

        static private void Handle406(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var roomname = arguments[0];
            Console.WriteLine("ERROR: Couldn't create room {0} due to room name in use", roomname);
        }

        static private void Handle407(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var roomname = arguments[0];
            Console.WriteLine("ERROR: Couldn't create room {0} due to erroneous room name", roomname);
        }

        static private void Handle408(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var command = arguments[0];
            Console.WriteLine("ERROR: Unknown command: {0}", command);
        }
        
        static private void Handle409(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var nickname = arguments[0];
            Console.WriteLine("ERROR: Failed to register due to erroneous nickname: {0}", nickname);
        }

        static private void Handle410(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var nickname = arguments[0];
            Console.WriteLine("ERROR: Failed to register due to nickname in use: {0}", nickname);
        }

        static private void Handle411(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var command = arguments[0];
            Console.WriteLine("ERROR: Couldn't execute command \"{0}\" because you are not registered", command);
        }

        static private void Handle412(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var command = arguments[0];
            Console.WriteLine("ERROR: Couldn't execute command \"{0}\" due to not enough params: {0}", command);
        }

        static private void Handle413(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var nickname = arguments[0];
            Console.WriteLine("ERROR: Failed to register due already registered under nickname: {0}", nickname);
        }

        static private void Handle414(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var nickname = arguments[0];
            Console.WriteLine("ERROR: Failed to register due already registered under nickname: {0}", nickname);
        }
    }
}
