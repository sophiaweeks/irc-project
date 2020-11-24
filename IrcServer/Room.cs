using System;
using System.Collections.Generic;
using System.Text;

namespace IrcServer
{
    class Room
    {
        public Room(string name)
        {
            m_name = name;
            Members = new List<IrcClient>();
        }

        public string GetName()
        {
            return m_name;
        }

        public void SendMessage(string msg)
        {
            foreach (IrcClient c in Members)
            {
                c.SendMessage(msg);
            }
        }

        private string m_name;
        public List<IrcClient> Members;
    }
}
