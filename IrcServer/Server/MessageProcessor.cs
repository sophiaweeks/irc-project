﻿using System;
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

                if (command == "NICK")
                {
                    HandleNick(pieces.Skip(1).ToArray(), message.Client);
                }
                else
                {
                    HandleUnknownCommand(command, message.Client);
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


        private void HandleUnknownCommand(string command, IrcClient client)
        {
            string msg = String.Format("408 {0} CR LF", command); //ERR_UNKNOWNCOMMAND
            client.SendMessage(msg);
        }

        private IIrcServer m_ircServer;

        private const int MAX_NICKNAME_LENGTH = 9;
    }
}
