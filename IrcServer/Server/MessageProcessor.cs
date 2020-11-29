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
            Console.WriteLine("Received NICK request from {0}", client.IsRegistered() ? client.GetNickname() : client.ToString());
            
            if (client.IsRegistered())
            {
                string msg = String.Format("413 {0} CR LF", client.GetNickname()); //ERR_ALREADYREGISTERED
                client.SendMessage(msg);
                Console.WriteLine("  Returned 413 (ERR_ALREADYREGISTERED)");
                return;
            }

            if (arguments.Count() < 1)
            {
                string msg = "412 NICK CR LF"; //ERR_NEEDMOREPARAMS
                client.SendMessage(msg);
                Console.WriteLine("  Returned 412 (ERR_NEEDMOREPARAMS)");
                return;
            }

            var nickname = arguments[0];

            if (nickname.Length > MAX_NICKNAME_LENGTH || nickname.Contains(" "))
            {
                string msg = String.Format("409 {0} CR LF", nickname); //ERR_ERONEOUSNICKNAME
                client.SendMessage(msg);
                Console.WriteLine("  Returned 409 (ERR_ERONEOUSNICKNAME): {0}", nickname);
                return;
            }

            if (m_ircServer.IsNicknameInUse(nickname))
            {
                string msg = String.Format("410 {0} CR LF", nickname); //ERR_NICKNAMEINUSE
                client.SendMessage(msg);
                Console.WriteLine("  Returned 410 (ERR_NICKNAMEINUSE): {0}", nickname);
                return;
            }

            client.Register(nickname);

            string response = String.Format("306 {0} CR LF", nickname); //RPL_REGISTERSUCCEEDED
            client.SendMessage(response);
            Console.WriteLine("  Returned 306 (RPL_REGISTERSUCCEEDED): {0}", nickname);
        }

        private void HandleQuit(string[] arguments, IrcClient client)
        {
            Console.WriteLine("Received QUIT request from {0}", client.IsRegistered() ? client.GetNickname() : client.ToString());
            
            m_ircServer.RemoveClient(client);
            string response = String.Format("310 CR LF"); //RPL_QUITSUCCEEDED
            client.SendMessage(response);
            Console.WriteLine("  Returned 310 (RPL_QUITSUCCEEDED)");
        }

        private void HandleCreate(string[] arguments, IrcClient client)
        {
            Console.WriteLine("Received CREATE request from {0}", client.IsRegistered() ? client.GetNickname() : client.ToString());
            
            if (!client.IsRegistered())
            {
                string msg = String.Format("411 CREATE CR LF"); //ERR_NOTREGISTERED
                client.SendMessage(msg);
                Console.WriteLine("  Returned 411 (ERR_NOTREGISTERED)");
                return;
            }

            if (arguments.Count() < 1)
            {
                string msg = "412 CREATE CR LF"; //ERR_NEEDMOREPARAMS
                client.SendMessage(msg);
                Console.WriteLine("  Returned 412 (ERR_NEEDMOREPARAMS)");
                return;
            }

            var roomname = arguments[0];

            if (roomname.Length > MAX_ROOM_LENGTH || roomname.Contains(" "))
            {
                string msg = String.Format("407 {0} CR LF", roomname); //ERR_ERRONEOUSROOMNAME
                client.SendMessage(msg);
                Console.WriteLine("  Returned 407 (ERR_ERRONEOUSROOMNAME): {0}", roomname);
                return;
            }

            if (m_ircServer.IsRoomNameInUse(roomname))
            {
                string msg = String.Format("406 {0} CR LF", roomname); //ERR_ROOMNAMEINUSE
                client.SendMessage(msg);
                Console.WriteLine("  Returned 406 (ERR_ROOMNAMEINUSE): {0}", roomname);
                return;
            }

            if (m_ircServer.GetNumRooms() >= MAX_ROOM_NUM)
            {
                string msg = String.Format("405 {0} CR LF", roomname); //ERR_TOOMANYROOMS
                client.SendMessage(msg);
                Console.WriteLine("  Returned 405 (ERR_TOOMANYROOMS): {0}", roomname);
                return;
            }

            m_ircServer.AddRoom(roomname);

            string response = String.Format("307 {0} CR LF", roomname); //RPL_CREATESUCCEEDED
            client.SendMessage(response);
            Console.WriteLine("  Returned 307 (RPL_CREATESUCCEEDED): {0}", roomname);

            Room room = m_ircServer.GetRoom(roomname);
            room.Members.Add(client);
            Console.WriteLine("{0} joined room {1}", client.GetNickname(), room.GetName());

            response = String.Format("308 {0} CR LF", roomname); //RPL_JOINSUCCEEDED
            client.SendMessage(response);
            Console.WriteLine("  Returned 307 (RPL_JOINSUCCEEDED): {0}", roomname);
        }

        private void HandleJoin(string[] arguments, IrcClient client)
        {
            Console.WriteLine("Received JOIN request from {0}", client.IsRegistered() ? client.GetNickname() : client.ToString());
            
            if (!client.IsRegistered())
            {
                string msg = String.Format("411 JOIN CR LF"); //ERR_NOTREGISTERED
                client.SendMessage(msg);
                Console.WriteLine("  Returned 411 (ERR_NOTREGISTERED)");
                return;
            }

            if (arguments.Count() < 1)
            {
                string msg = "412 JOIN CR LF"; //ERR_NEEDMOREPARAMS
                client.SendMessage(msg);
                Console.WriteLine("  Returned 412 (ERR_NEEDMOREPARAMS)");
                return;
            }

            var roomname = arguments[0];

            Room room = m_ircServer.GetRoom(roomname);
            if (room == null)
            {
                string msg = String.Format("402 JOIN {0} CR LF", roomname); //ERR_NOSUCHROOM
                client.SendMessage(msg);
                Console.WriteLine("  Returned 402 (ERR_NOSUCHROOM): {0}", roomname);
                return;
            }

            if (room.Members.Contains(client))
            {
                string msg = String.Format("404 {0} CR LF", roomname); //ERR_ALREADYJOINED
                client.SendMessage(msg);
                Console.WriteLine("  Returned 404 (ERR_ALREADYJOINED): {0}", roomname);
                return;
            }

            if (room.Members.Count >= MAX_NUM_ROOM_MEMBERS)
            {
                string msg = String.Format("414 {0} CR LF", roomname); //ERR_ROOMISFULL
                client.SendMessage(msg);
                Console.WriteLine("  Returned 414 (ERR_ROOMISFULL): {0}", roomname);
                return;
            }

            string notification = String.Format("308 {0} {1} CR LF", roomname, client.GetNickname()); //RPL_JOINSUCCEEDED
            room.SendMessage(notification);
            Console.WriteLine("  Returned 308 (RPL_JOINSUCCEEDED): {0}", roomname);

            room.Members.Add(client);

            string response = String.Format("308 {0} CR LF", roomname); //RPL_JOINSUCCEEDED
            client.SendMessage(response);
            Console.WriteLine("  Notified room {0} of join", roomname);
        }

        private void HandlePart(string[] arguments, IrcClient client)
        {
            Console.WriteLine("Received PART request from {0}", client.IsRegistered() ? client.GetNickname() : client.ToString());

            if (!client.IsRegistered())
            {
                string msg = String.Format("411 PART CR LF"); //ERR_NOTREGISTERED
                client.SendMessage(msg);
                Console.WriteLine("  Returned 411 (ERR_NOTREGISTERED)");
                return;
            }

            if (arguments.Count() < 1)
            {
                string msg = "412 PART CR LF"; //ERR_NEEDMOREPARAMS
                client.SendMessage(msg);
                Console.WriteLine("  Returned 412 (ERR_NEEDMOREPARAMS)");
                return;
            }

            var roomname = arguments[0];
            Room room = m_ircServer.GetRoom(roomname);
            if (room == null)
            {
                string msg = String.Format("402 PART {0} CR LF", roomname); //ERR_NOSUCHROOM
                client.SendMessage(msg);
                Console.WriteLine("  Returned 402 (ERR_NOSUCHROOM): {0}", roomname);
                return;
            }

            if (!room.Members.Contains(client))
            {
                string msg = String.Format("403 PART {0} CR LF", roomname); //ERR_NOTINROOM
                client.SendMessage(msg);
                Console.WriteLine("  Returned 403 (ERR_NOTINROOM) {0}", roomname);
                return;
            }

            room.Members.Remove(client);

            string response = String.Format("309 {0} CR LF", roomname); //RPL_PARTSUCCEEDED
            client.SendMessage(response);
            Console.WriteLine("  Returned 309 (RPL_PARTSUCCEEDED) {0}", roomname);

            if (room.Members.Count == 0)
            {
                m_ircServer.RemoveRoom(room);
                return;
            }

            string notification = String.Format("309 {0} {1} CR LF", roomname, client.GetNickname()); //RPL_PARTSUCCEEDED
            room.SendMessage(notification);
            Console.WriteLine("  Notified room {0} of part", roomname);
        }

        private void HandleMsg(string[] arguments, IrcClient client)
        {
            Console.WriteLine("Received MSG request from {0}", client.IsRegistered() ? client.GetNickname() : client.ToString());
            
            if (!client.IsRegistered())
            {
                string msg = String.Format("411 MSG CR LF"); //ERR_NOTREGISTERED
                client.SendMessage(msg);
                Console.WriteLine("  Returned 411 (ERR_NOTREGISTERED)");
                return;
            }

            if (arguments.Count() < 2)
            {
                string msg = "412 MSG CR LF"; //ERR_NEEDMOREPARAMS
                client.SendMessage(msg);
                Console.WriteLine("  Returned 412 (ERR_NEEDMOREPARAMS)");
                return;
            }

            var roomname = arguments[0];
            Room room = m_ircServer.GetRoom(roomname);
            if (room == null)
            {
                string msg = String.Format("402 MSG {0} CR LF", roomname); //ERR_NOSUCHROOM
                client.SendMessage(msg);
                Console.WriteLine("  Returned 402 (ERR_NOSUCHROOM): {0}", roomname);
                return;
            }

            if (!room.Members.Contains(client))
            {
                string msg = String.Format("403 MSG {0} CR LF", roomname); //ERR_NOTINROOM
                client.SendMessage(msg);
                Console.WriteLine("  Returned 403 (ERR_NOTINROOM): {0}", roomname);
                return;
            }

            string text = arguments[1];
            string notification = String.Format("311 {0} {1} \"{2}\" CR LF", roomname, client.GetNickname(), text); //RPL_MSGSUCCEEDED
            room.SendMessage(notification);
            Console.WriteLine("  Returned 311 (RPL_MSGSUCCEEDED): {0}", roomname);
            Console.WriteLine("  Sent message to room {0}", roomname);
        }

        private void HandleList(string[] arguments, IrcClient client)
        {
            Console.WriteLine("Received LIST request from {0}", client.IsRegistered() ? client.GetNickname() : client.ToString());
            
            var rooms = m_ircServer.GetRooms();
            client.SendMessage(String.Format("300 CR LF")); //RPL_LISTSTART
            Console.WriteLine("  Returned 300 (RPL_LISTSTART)");

            foreach (Room r in rooms)
            {
                client.SendMessage(String.Format("301 {0} CR LF", r.GetName())); //RPL_LIST
                Console.WriteLine("  Returned 301 (RPL_LIST): {0}", r.GetName());
            }

            client.SendMessage(String.Format("302 CR LF")); //RPL_LISTEND
            Console.WriteLine("  Returned 302 (RPL_LISTEND)");
        }

        private void HandleNames(string[] arguments, IrcClient client)
        {
            Console.WriteLine("Received NAMES request from {0}", client.IsRegistered() ? client.GetNickname() : client.ToString());
            
            if (arguments.Count() < 1)
            {
                string msg = "412 NAMES CR LF"; //ERR_NEEDMOREPARAMS
                client.SendMessage(msg);
                Console.WriteLine("  Returned 412 (ERR_NEEDMOREPARAMS)");
                return;
            }

            var roomname = arguments[0];
            var room = m_ircServer.GetRoom(roomname);

            if (room == null)
            {
                string msg = String.Format("402 NAMES {0} CR LF", roomname); //ERR_NOSUCHROOM
                client.SendMessage(msg);
                Console.WriteLine("  Returned 402 (ERR_NOSUCHROOM)");
                return;
            }

            client.SendMessage(String.Format("303 {0} CR LF", roomname)); //RPL_NAMESSTART
            Console.WriteLine("  Returned 303 (RPL_NAMESSTART): {0}", roomname);

            foreach (IrcClient c in room.Members)
            {
                client.SendMessage(String.Format("304 {0} {1} CR LF", roomname, c.GetNickname())); //RPL_NAMES
                Console.WriteLine("  Returned 304 (RPL_NAMES): room {0}, client: {1}", roomname, c.GetNickname());
            }

            client.SendMessage(String.Format("305 {0} CR LF", roomname)); //RPL_NAMESEND
            Console.WriteLine("  Returned 305 (RPL_NAMESEND): {0}", roomname);
        }

        private void HandleUnknownCommand(string command, IrcClient client)
        {
            Console.WriteLine("Received uknown request \"{0}\" from {1}", command, client.IsRegistered() ? client.GetNickname() : client.ToString());
            
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
