//RockPaperTCP
//Tilly Dewing Fall 2019 Networking Project

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RockPaperTCP
{
    class MainMenu
    {
        private static string menuText;
        private static string menuTextFilePath = "MenuText.txt";
        private static ConsoleColor titleColor = ConsoleColor.Cyan;
        private static ConsoleColor textColor = ConsoleColor.White;
        private static ConsoleColor menuHighlight = ConsoleColor.DarkCyan;
        private static int selectedOption = 0;
        private static int defaultPort = 25555;

        public static bool running = true;

        public static void InitializeMenu()
        {
            menuText = File.ReadAllText(menuTextFilePath);
            Console.Clear();
            Server.Shutdown();
        }

        public static void Update()
        {
            
            if (Input.GetKey(ConsoleKey.DownArrow))
            {
                selectedOption++;
            }
            else if (Input.GetKey(ConsoleKey.UpArrow))
            {
                selectedOption--;
            }

            if (Input.GetKey(ConsoleKey.Enter))
            {
                running = false;
                string name;
                switch (selectedOption)
                {
                    case 0:
                        Console.Clear();
                        Console.Write("Please enter a player name: ");
                        name = Console.ReadLine();
                        Server.StartServer(name);
                        break;
                    case 1:
                        Console.Clear();
                        Console.Write("Please enter a player name: ");
                        name = Console.ReadLine();
                        Console.Write("Enter a server IP to connect to: ");
                        string ip = Console.ReadLine();
                        Client.InitializeClient(ip, defaultPort, name);
                        break;
                    case 2:
                        Program.running = false;
                        break;
                }
            }
            selectedOption = Math.Clamp(selectedOption, 0, 2);

            DrawMenu();
        }

        static void HostGame()
        { 
            
        }

        public static void DrawMenu()
        {
            if (!running)   //stops menu from being drawn if not running
            {
                return;
            }

            Console.SetCursorPosition(0, 0);
            ConsoleExtentions.WriteColor(menuText, titleColor);
            Console.WriteLine("\n_______________________________________________________________________________\n");

            switch(selectedOption)
            {
                case 0:
                    ConsoleExtentions.WriteLineBackgroundColor("Host New Game", textColor, menuHighlight);
                    ConsoleExtentions.WriteLineColor("Join Game", textColor);
                    ConsoleExtentions.WriteLineColor("Quit Game", textColor);
                    break;
                case 1:                    
                    ConsoleExtentions.WriteLineColor("Host New Game", textColor);
                    ConsoleExtentions.WriteLineBackgroundColor("Join Game", textColor, menuHighlight);
                    ConsoleExtentions.WriteLineColor("Quit Game", textColor);
                    break;
                case 2:
                    ConsoleExtentions.WriteLineColor("Host New Game", textColor);
                    ConsoleExtentions.WriteLineColor("Join Game", textColor);
                    ConsoleExtentions.WriteLineBackgroundColor("Quit Game", textColor, menuHighlight);
                    break;
            }

        }
    }
}
