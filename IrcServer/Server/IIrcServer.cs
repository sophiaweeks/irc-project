﻿using System;
using System.Collections.Generic;
using System.Text;

namespace IrcServer
{
    interface IIrcServer
    {
        bool IsNicknameInUse(string nickname);
    }
}
