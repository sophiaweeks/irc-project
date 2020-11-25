using System;
using System.Collections.Generic;
using System.Text;

namespace IrcClient
{
    enum TextType
    {
        Program,
        UserInput,
        ServerReply,
        ServerError,
        ServerNotification,
    }

    static class ConsoleWriter
    {
        private static object LOCK = new object();
        public static void WriteToConsole(TextType textType, string text)
        {
            lock (LOCK)
            {
                var currentColor = Console.ForegroundColor;
                switch (textType)
                {
                    case TextType.Program:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case TextType.UserInput:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case TextType.ServerReply:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("  ");
                        break;
                    case TextType.ServerError:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("  "); break;
                    case TextType.ServerNotification:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("  ");
                        break;
                }

                Console.WriteLine(text);
                Console.ForegroundColor = currentColor;
            }
        }
    }
}
