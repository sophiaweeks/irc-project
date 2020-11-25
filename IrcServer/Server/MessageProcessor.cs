using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace IrcServer
{
    class MessageProcessor
    {
        public MessageProcessor(IIrcServer server)
        {
            m_ircServer = server;
        }

        public void ParseMessage(Message message)
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

                switch(command)
                {
                    case "NICK":
                        HandleNick(msgParts.Skip(1).ToArray(), message.Client);
                        break;
                    case "QUIT":
                        HandleQuit(msgParts.Skip(1).ToArray(), message.Client);
                        break;
                    case "CREATE":
                        HandleCreate(msgParts.Skip(1).ToArray(), message.Client);
                        break;
                    case "JOIN":
                        HandleJoin(msgParts.Skip(1).ToArray(), message.Client);
                        break;
                    case "PART":
                        HandlePart(msgParts.Skip(1).ToArray(), message.Client);
                        break;
                    case "MSG":
                        HandleMsg(msgParts.Skip(1).ToArray(), message.Client);
                        break;
                    case "LIST":
                        HandleList(msgParts.Skip(1).ToArray(), message.Client);
                        break;
                    case "NAMES":
                        HandleNames(msgParts.Skip(1).ToArray(), message.Client);
                        break;
                    default:
                        HandleUnknownCommand(command, message.Client);
                        break;
                }
            }
        }

        private void HandleNick(string[] arguments, IrcClient client)
        {
            if (client.IsRegistered())
            {
                string msg = String.Format("413 {0} CR LF", client.GetNickname()); //ERR_ALREADYREGISTERED
                client.SendMessage(msg);
                return;
            }

            if (arguments.Count() < 1)
            {
                string msg = "412 NICK CR LF"; //ERR_NEEDMOREPARAMS
                client.SendMessage(msg);
                return;
            }

            var nickname = arguments[0];

            if (nickname.Length > MAX_NICKNAME_LENGTH)
            {
                string msg = String.Format("409 {0} CR LF", nickname); //ERR_ERONEOUSNICKNAME
                client.SendMessage(msg);
                return;
            }

            if (m_ircServer.IsNicknameInUse(nickname))
            {
                string msg = String.Format("410 {0} CR LF", nickname); //ERR_NICKNAMEINUSE
                client.SendMessage(msg);
                return;
            }

            client.Register(nickname);

            string response = String.Format("306 {0} CR LF", nickname); //RPL_REGISTERSUCCEEDED
            client.SendMessage(response);
        }

        private void HandleQuit(string[] arguments, IrcClient client)
        {
            m_ircServer.RemoveClient(client);
            string response = String.Format("310 CR LF"); //RPL_QUITSUCCEEDED
            client.SendMessage(response);
        }

        private void HandleCreate(string[] arguments, IrcClient client)
        {
            if (!client.IsRegistered())
            {
                string msg = String.Format("411 CREATE CR LF"); //ERR_NOTREGISTERED
                client.SendMessage(msg);
                return;
            }

            if (arguments.Count() < 1)
            {
                string msg = "412 CREATE CR LF"; //ERR_NEEDMOREPARAMS
                client.SendMessage(msg);
                return;
            }

            var roomname = arguments[0];

            if (roomname.Length > MAX_ROOM_LENGTH)
            {
                string msg = String.Format("407 {0} CR LF", roomname); //ERR_ERRONEOUSROOMNAME
                client.SendMessage(msg);
                return;
            }

            if (m_ircServer.IsRoomNameInUse(roomname))
            {
                string msg = String.Format("406 {0} CR LF", roomname); //ERR_ROOMNAMEINUSE
                client.SendMessage(msg);
                return;
            }

            if (m_ircServer.GetNumRooms() >= MAX_ROOM_NUM)
            {
                string msg = String.Format("405 {0} CR LF", roomname); //ERR_TOOMANYROOMS
                client.SendMessage(msg);
                return;
            }

            m_ircServer.AddRoom(roomname);

            string response = String.Format("307 {0} CR LF", roomname); //RPL_CREATESUCCEEDED
            client.SendMessage(response);

            Room room = m_ircServer.GetRoom(roomname);
            room.Members.Add(client);

            response = String.Format("308 {0} CR LF", roomname); //RPL_JOINSUCCEEDED
            client.SendMessage(response);
        }

        private void HandleJoin(string[] arguments, IrcClient client)
        {
            if (!client.IsRegistered())
            {
                string msg = String.Format("411 JOIN CR LF"); //ERR_NOTREGISTERED
                client.SendMessage(msg);
                return;
            }

            if (arguments.Count() < 1)
            {
                string msg = "412 JOIN CR LF"; //ERR_NEEDMOREPARAMS
                client.SendMessage(msg);
                return;
            }

            var roomname = arguments[0];

            Room room = m_ircServer.GetRoom(roomname);
            if (room == null)
            {
                string msg = String.Format("402 JOIN {0} CR LF", roomname); //ERR_NOSUCHROOM
                client.SendMessage(msg);
                return;
            }

            if (room.Members.Contains(client))
            {
                string msg = String.Format("404 {0} CR LF", roomname); //ERR_ALREADYJOINED
                client.SendMessage(msg);
                return;
            }

            if (room.Members.Count >= MAX_NUM_ROOM_MEMBERS)
            {
                string msg = String.Format("414 {0} CR LF", roomname); //ERR_ROOMISFULL
                client.SendMessage(msg);
                return;
            }

            string notification = String.Format("308 {0} {1} CR LF", roomname, client.GetNickname()); //RPL_JOINSUCCEEDED
            room.SendMessage(notification);

            room.Members.Add(client);

            string response = String.Format("308 {0} CR LF", roomname); //RPL_JOINSUCCEEDED
            client.SendMessage(response);
        }

        private void HandlePart(string[] arguments, IrcClient client)
        {
            if (!client.IsRegistered())
            {
                string msg = String.Format("411 PART CR LF"); //ERR_NOTREGISTERED
                client.SendMessage(msg);
                return;
            }

            if (arguments.Count() < 1)
            {
                string msg = "412 PART CR LF"; //ERR_NEEDMOREPARAMS
                client.SendMessage(msg);
                return;
            }

            var roomname = arguments[0];
            Room room = m_ircServer.GetRoom(roomname);
            if (room == null)
            {
                string msg = String.Format("402 PART {0} CR LF", roomname); //ERR_NOSUCHROOM
                client.SendMessage(msg);
                return;
            }

            if (!room.Members.Contains(client))
            {
                string msg = String.Format("403 PART {0} CR LF", roomname); //ERR_NOTINROOM
                client.SendMessage(msg);
                return;
            }

            room.Members.Remove(client);

            string response = String.Format("309 {0} CR LF", roomname); //RPL_PARTSUCCEEDED
            client.SendMessage(response);

            if (room.Members.Count == 0)
            {
                m_ircServer.RemoveRoom(room);
                return;
            }

            string notification = String.Format("309 {0} {1} CR LF", roomname, client.GetNickname()); //RPL_PARTSUCCEEDED
            room.SendMessage(notification);
        }

        private void HandleMsg(string[] arguments, IrcClient client)
        {
            if (!client.IsRegistered())
            {
                string msg = String.Format("411 MSG CR LF"); //ERR_NOTREGISTERED
                client.SendMessage(msg);
                return;
            }

            if (arguments.Count() < 2)
            {
                string msg = "412 MSG CR LF"; //ERR_NEEDMOREPARAMS
                client.SendMessage(msg);
                return;
            }

            var roomname = arguments[0];
            Room room = m_ircServer.GetRoom(roomname);
            if (room == null)
            {
                string msg = String.Format("402 MSG {0} CR LF", roomname); //ERR_NOSUCHROOM
                client.SendMessage(msg);
                return;
            }

            if (!room.Members.Contains(client))
            {
                string msg = String.Format("403 MSG {0} CR LF", roomname); //ERR_NOTINROOM
                client.SendMessage(msg);
                return;
            }

            string text = arguments[1];
            string notification = String.Format("311 {0} {1} \"{2}\" CR LF", roomname, client.GetNickname(), text); //RPL_MSGSUCCEEDED
            room.SendMessage(notification);
        }

        private void HandleList(string[] arguments, IrcClient client)
        {
            var rooms = m_ircServer.GetRooms();
            client.SendMessage(String.Format("300 CR LF")); //RPL_LISTSTART

            foreach (Room r in rooms)
            {
                client.SendMessage(String.Format("301 {0} CR LF", r.GetName())); //RPL_LIST
            }

            client.SendMessage(String.Format("302 CR LF")); //RPL_LISTEND
        }

        private void HandleNames(string[] arguments, IrcClient client)
        {
            if (arguments.Count() < 1)
            {
                string msg = "412 NAMES CR LF"; //ERR_NEEDMOREPARAMS
                client.SendMessage(msg);
                return;
            }

            var roomname = arguments[0];
            var room = m_ircServer.GetRoom(roomname);

            if (room == null)
            {
                string msg = String.Format("402 NAMES {0} CR LF", roomname); //ERR_NOSUCHROOM
                client.SendMessage(msg);
                return;
            }

            client.SendMessage(String.Format("303 {0} CR LF", roomname)); //RPL_NAMESSTART

            foreach (IrcClient c in room.Members)
            {
                client.SendMessage(String.Format("304 {0} {1} CR LF", roomname, c.GetNickname())); //RPL_NAMES
            }

            client.SendMessage(String.Format("305 {0} CR LF", roomname)); //RPL_NAMESEND
        }

        private void HandleUnknownCommand(string command, IrcClient client)
        {
            string msg = String.Format("408 {0} CR LF", command); //ERR_UNKNOWNCOMMAND
            client.SendMessage(msg);
        }

        private IIrcServer m_ircServer;

        private const int MAX_NICKNAME_LENGTH = 9;
        private const int MAX_ROOM_LENGTH = 20;
        private const int MAX_ROOM_NUM = 500;
        private const int MAX_NUM_ROOM_MEMBERS = 50;
    }
}
