using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
                var pieces = m.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                if (pieces.Count() < 1)
                {
                    return;
                }

                var command = pieces[0];

                switch(command)
                {
                    case "NICK":
                        HandleNick(pieces.Skip(1).ToArray(), message.Client);
                        break;
                    case "CREATE":
                        HandleCreate(pieces.Skip(1).ToArray(), message.Client);
                        break;
                    case "JOIN":
                        HandleJoin(pieces.Skip(1).ToArray(), message.Client);
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
                string msg = String.Format("402 {0} CR LF", roomname); //ERR_NOSUCHROOM
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
