﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

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
                    ConsoleWriter.WriteToConsole(TextType.ServerError, "ERROR: The connection to the server closed unexpectedly.  Our apologies.");
                    Environment.Exit(0);
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
                var msgParts = Regex.Matches(m, "[^\\s\"']+|\"([^\"]*)\"|'([^']*)'")
                                    .Cast<Match>().Select(iMatch => iMatch.Value.Replace("\"", "").Replace("'", "")).ToArray();

                if (msgParts.Count() < 1)
                {
                    return;
                }

                var command = msgParts[0];

                switch (command)
                {
                    case "300":
                        Handle300(msgParts.Skip(1).ToArray());
                        break;
                    case "301":
                        Handle301(msgParts.Skip(1).ToArray());
                        break;
                    case "302":
                        Handle302(msgParts.Skip(1).ToArray());
                        break;
                    case "303":
                        Handle303(msgParts.Skip(1).ToArray());
                        break;
                    case "304":
                        Handle304(msgParts.Skip(1).ToArray());
                        break;
                    case "305":
                        Handle305(msgParts.Skip(1).ToArray());
                        break;
                    case "306":
                        Handle306(msgParts.Skip(1).ToArray());
                        break;
                    case "307":
                        Handle307(msgParts.Skip(1).ToArray());
                        break;
                    case "308":
                        Handle308(msgParts.Skip(1).ToArray());
                        break;
                    case "309":
                        Handle309(msgParts.Skip(1).ToArray());
                        break;
                    case "310":
                        Handle310(msgParts.Skip(1).ToArray());
                        break;
                    case "311":
                        Handle311(msgParts.Skip(1).ToArray());
                        break;
                    case "312":
                        Handle312(msgParts.Skip(1).ToArray());
                        break;
                    case "402":
                        Handle402(msgParts.Skip(1).ToArray());
                        break;
                    case "403":
                        Handle403(msgParts.Skip(1).ToArray());
                        break;
                    case "404":
                        Handle404(msgParts.Skip(1).ToArray());
                        break;
                    case "405":
                        Handle405(msgParts.Skip(1).ToArray());
                        break;
                    case "406":
                        Handle406(msgParts.Skip(1).ToArray());
                        break;
                    case "407":
                        Handle407(msgParts.Skip(1).ToArray());
                        break;
                    case "408":
                        Handle408(msgParts.Skip(1).ToArray());
                        break;
                    case "409":
                        Handle409(msgParts.Skip(1).ToArray());
                        break;
                    case "410":
                        Handle410(msgParts.Skip(1).ToArray());
                        break;
                    case "411":
                        Handle411(msgParts.Skip(1).ToArray());
                        break;
                    case "412":
                        Handle412(msgParts.Skip(1).ToArray());
                        break;
                    case "413":
                        Handle413(msgParts.Skip(1).ToArray());
                        break;
                    case "414":
                        Handle414(msgParts.Skip(1).ToArray());
                        break;
                }
            }
        }

        static private void Handle300(string[] arguments)
        {
            ConsoleWriter.WriteToConsole(TextType.ServerReply, "List of rooms:");
        }

        static private void Handle301(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var roomname = arguments[0];
            ConsoleWriter.WriteToConsole(TextType.ServerReply, String.Format("   {0}", roomname));
        }

        static private void Handle302(string[] arguments)
        {
            ConsoleWriter.WriteToConsole(TextType.ServerReply, "End of room list.");
        }

        static private void Handle303(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var roomname = arguments[0]; 
            ConsoleWriter.WriteToConsole(TextType.ServerReply, String.Format("List of members in room {0}:", roomname));
        }

        static private void Handle304(string[] arguments)
        {
            if (arguments.Length < 2)
            {
                return;
            }

            var nickname = arguments[1];
            ConsoleWriter.WriteToConsole(TextType.ServerReply, String.Format("   {0}", nickname));
        }

        static private void Handle305(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var roomname = arguments[0];
            ConsoleWriter.WriteToConsole(TextType.ServerReply, String.Format("End of member list for room {0}:", roomname));
        }

        static private void Handle306(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var nickname = arguments[0];
            ConsoleWriter.WriteToConsole(TextType.ServerReply, String.Format("Successfully registered under nickname: {0}", nickname));
        }

        static private void Handle307(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var roomname = arguments[0];
            ConsoleWriter.WriteToConsole(TextType.ServerReply, String.Format("Successfully created room: {0}", roomname));
        }

        static private void Handle308(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var roomname = arguments[0];

            if (arguments.Length > 1)
            {
                var nickname = arguments[1];
                ConsoleWriter.WriteToConsole(TextType.ServerNotification, String.Format("{0} joined room {1}", nickname, roomname));
                return;
            }

            ConsoleWriter.WriteToConsole(TextType.ServerReply, String.Format("Successfully joined room: {0}", roomname));
        }

        static private void Handle309(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var roomname = arguments[0];

            if (arguments.Length > 1)
            {
                var nickname = arguments[1];
                ConsoleWriter.WriteToConsole(TextType.ServerNotification, String.Format("{0} left room {1}", nickname, roomname));
                return;
            }

            ConsoleWriter.WriteToConsole(TextType.ServerReply, String.Format("Successfully left room: {0}", roomname));
        }

        static private void Handle310(string[] arguments)
        {
            ConsoleWriter.WriteToConsole(TextType.ServerReply, "Successfully disconnected from the server");
        }

        static private void Handle311(string[] arguments)
        {
            if (arguments.Length < 3)
            {
                return;
            }

            var roomname = arguments[0];
            var nickname = arguments[1];
            var text = arguments[2];

            ConsoleWriter.WriteToConsole(TextType.ServerNotification, String.Format("{0} in {1} says: {2}", nickname, roomname, text));
        }

        static private void Handle312(string[] arguments)
        {
            ConsoleWriter.WriteToConsole(TextType.ServerNotification, String.Format("The server has shut down.  Please try again later."));
            Environment.Exit(0);
        }

        static private void Handle402(string[] arguments)
        {
            if (arguments.Length < 2)
            {
                return;
            }

            var command = arguments[0];
            var roomname = arguments[1];
            ConsoleWriter.WriteToConsole(TextType.ServerError, String.Format("ERROR: Couldn't execute command {0} on room {1}. Reason: no such room", command, roomname));
        }

        static private void Handle403(string[] arguments)
        {
            if (arguments.Length < 2)
            {
                return;
            }

            var command = arguments[0];
            var roomname = arguments[1];
            ConsoleWriter.WriteToConsole(TextType.ServerError, String.Format( "ERROR: Couldn't execute command {0} on room {1}. Reason: not in room", command, roomname));
        }

        static private void Handle404(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var roomname = arguments[0];
            ConsoleWriter.WriteToConsole(TextType.ServerError, String.Format("ERROR: Failed to join room {0}. Reason: already joined", roomname));
        }

        static private void Handle405(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var roomname = arguments[0];
            ConsoleWriter.WriteToConsole(TextType.ServerError, String.Format("ERROR: Failed to create room {0}. Reason: too many rooms", roomname));
        }

        static private void Handle406(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var roomname = arguments[0];
            ConsoleWriter.WriteToConsole(TextType.ServerError, String.Format("ERROR: Failed to create room {0}. Reason: room name in use", roomname));
        }

        static private void Handle407(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var roomname = arguments[0];
            ConsoleWriter.WriteToConsole(TextType.ServerError, String.Format( "ERROR: Failed to create room {0}. Reason: erroneous room name", roomname));
        }

        static private void Handle408(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var command = arguments[0];
            ConsoleWriter.WriteToConsole(TextType.ServerError, String.Format( "ERROR: Unknown command: {0}", command));
        }
        
        static private void Handle409(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var nickname = arguments[0];
            ConsoleWriter.WriteToConsole(TextType.ServerError, String.Format( "ERROR: Failed to register under {0}. Reason: erroneous nickname", nickname));
        }

        static private void Handle410(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var nickname = arguments[0];
            ConsoleWriter.WriteToConsole(TextType.ServerError, String.Format("ERROR: Failed to register under {0}. Reason: nickname in use", nickname));
        }

        static private void Handle411(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var command = arguments[0];
            ConsoleWriter.WriteToConsole(TextType.ServerError, String.Format("ERROR: Failed to execute command \"{0}\". Reason: not registered", command));
        }

        static private void Handle412(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var command = arguments[0];
            ConsoleWriter.WriteToConsole(TextType.ServerError, String.Format("ERROR: Failed to execute command \"{0}\". Reason: not enough params: {0}", command));
        }

        static private void Handle413(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var nickname = arguments[0];
            ConsoleWriter.WriteToConsole(TextType.ServerError, String.Format( "ERROR: Failed to register under {0}. Reason: already registered", nickname));
        }

        static private void Handle414(string[] arguments)
        {
            if (arguments.Length < 1)
            {
                return;
            }

            var roomname = arguments[0];
            ConsoleWriter.WriteToConsole(TextType.ServerError, String.Format( "ERROR: Failed to join room {0}. Reason: room is full", roomname));
        }
    }
}
