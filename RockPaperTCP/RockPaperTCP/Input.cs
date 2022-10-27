//RockPaperTCP
//Tilly Dewing Fall 2019 Networking Project

using System;
using System.Collections.Generic;
using System.Text;

namespace RockPaperTCP
{
    class Input
    {
        public static List<ConsoleKey> keysDown = new List<ConsoleKey>();

        public static bool GetKey(ConsoleKey key)
        {
            if (keysDown.Contains(key))
            {
                return true;
            }
            return false;
        }

        public static void UpdateInput()
        {
            //Check input
            if (Console.KeyAvailable)
            {
                keysDown.Add(Console.ReadKey(true).Key);
            }
        }
        public static void EndUpdateInput()
        {
            keysDown.Clear();
        }
    }
}
