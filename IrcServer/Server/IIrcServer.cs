using System;
using System.Collections.Generic;
using System.Text;

namespace IrcServer
{
    interface IIrcServer
    {
        bool IsNicknameInUse(string nickname);
        bool IsRoomNameInUse(string roomname);
        int GetNumRooms();
        void AddRoom(string roomname, IrcClient creator);
    }
}
