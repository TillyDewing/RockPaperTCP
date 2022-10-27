//RockPaperTCP
//Tilly Dewing Fall 2019 Networking Project

using System;
using System.Collections.Generic;
using System.Text;

namespace RockPaperTCP
{
    class ConsoleExtentions  //Class to extend functionality of default Console class.
    {
        public static void WriteColor(string text, ConsoleColor color)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = oldColor;
        }

        public static void WriteBackgroundColor(string text, ConsoleColor textColor, ConsoleColor backGroundColor)
        {
            ConsoleColor oldBackgroundCol = Console.BackgroundColor;
            Console.BackgroundColor = backGroundColor;
            WriteColor(text, textColor);
            Console.BackgroundColor = oldBackgroundCol;
        }

        public static void WriteLineBackgroundColor(string text, ConsoleColor textColor, ConsoleColor backGroundColor)
        {
            ConsoleColor oldBackgroundCol = Console.BackgroundColor;
            Console.BackgroundColor = backGroundColor;
            WriteLineColor(text, textColor);
            Console.BackgroundColor = oldBackgroundCol;
        }

        public static void WriteLineColor(string text, ConsoleColor color)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = oldColor;
        }
    }
}
