//RockPaperTCP
//Tilly Dewing Fall 2019 Networking Project

using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace RockPaperTCP
{
    class Timer //Simple timer class for round timers
    {
        public static bool running;
        public static int startTime;
        public static int endTime;
        public static System.Timers.Timer timer;

        public static void StartTimer(int seconds)
        {
            timer = new System.Timers.Timer(15000);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
            running = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            StopTimer();
        }

        public static void StopTimer()
        {
            running = false;
            if (timer != null)
            {
                timer.Stop();
                timer.Elapsed -= OnTimedEvent;
                timer.Dispose();
            }
        }
    }
}
