//RockPaperTCP
//Tilly Dewing Fall 2019 Networking Project

using System;

namespace RockPaperTCP
{ 
    class Program
    {
        static int msPerFrame = 1000 / 10; //No idea why 21 makes it run at 20 when 20 makes it run @ 16 I asume theres some rounding errors some where
        static long timeOfLastFrame = 0;
        static long elaspsedTicks = 0;
        static int frameCount = 0;
        static int fps = 0;
        public static bool running = true;



        static void Main(string[] args)
        {
            MainMenu.InitializeMenu();
            while (running)
            {
                Console.CursorVisible = false;
                Tick();
            }
        }

        public static void Tick()
        {
            int waitTime = (int)(timeOfLastFrame + msPerFrame) - Environment.TickCount;
            Input.UpdateInput(); //polls input to detect any down keys
            if (waitTime > 0)
            {
                return;
            }

            timeOfLastFrame = Environment.TickCount;
            frameCount += 1;
            if (Environment.TickCount >= elaspsedTicks + 1000) //calculates current frame
            {
                fps = frameCount;
                frameCount = 0;
                elaspsedTicks = Environment.TickCount;
            }
            OnUpdate();
            OnEndUpdate();
        }

        public static void OnUpdate() //Called at the begining of each frame
        {
            //Console.WriteLine(Client.running);
            //Client.Update();
            if (MainMenu.running)
            {
                MainMenu.Update();
            }
            else if (Server.running)
            {
                Server.Update();
            }
            else if (Client.running)
            {
                Client.Update();
            }
        }
        public static void OnEndUpdate() //called at the end of the frame
        {
            Input.EndUpdateInput(); //clears keys pressed after frame is done.
        }
    }
}
