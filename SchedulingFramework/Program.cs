using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SchedulingFramework
{
    class Program


    {
        public static int count;
        public static bool currentRunning;
        private static Timer aTimer;

        static void Main(string[] args)
        {
            count = 0;
            currentRunning = false;

            // Create a timer and set a two second interval.
            aTimer = new System.Timers.Timer();
            aTimer.Interval = 2000;

            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;

            // Have the timer fire repeated events (true is the default)
            aTimer.AutoReset = true;

            // Start the timer
            aTimer.Enabled = true;
        }
        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            MQWatcher mqw = new MQWatcher();
            try
            {
                if (!currentRunning)
                {
                    List<Schedule> sch = new List<Schedule>();
                    string file = mqw.CreateFile(Schedule);
                    currentRunning = true;

                }
            }catch(Exception ex)
            {

            }
            finally
            {
                currentRunning = false;
            }
            
        }
    }
}
