using System;
using System.Collections.Generic;
using System.Text;

namespace IrcServer
{
    interface IIrcServer
    {
        void Exit();
        void RemoveClient(IrcClient client);
        bool IsNicknameInUse(string nickname);
        bool IsRoomNameInUse(string roomname);
        int GetNumRooms();
        void AddRoom(string roomname);
        void RemoveRoom(Room room);
        Room GetRoom(string rooomname);
        List<Room> GetRooms();
    }
}
