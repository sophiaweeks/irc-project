using System;
using System.Collections.Generic;
using System.Text;

namespace IrcServer
{
    class Room
    {
        public Room(string name, IrcClient creator)
        {
            m_name = name;
            Members = new List<IrcClient> { creator };
        }

        public string GetName()
        {
            return m_name;
        }

        private string m_name;
        public List<IrcClient> Members;
    }
}
