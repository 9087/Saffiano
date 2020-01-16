using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Saffiano
{
    internal class Timer : IComparable<Timer>
    {
        public delegate void TimerCallback();
        private static uint count = 0;
        private float timestamp;
        private TimerCallback callback;
        private uint id;

        public int CompareTo(Timer other)
        {
            int c = this.timestamp.CompareTo(other.timestamp);
            if (c != 0)
            {
                return c;
            }
            return this.id.CompareTo(other.id);
        }

        internal static SortedSet<Timer> timers = new SortedSet<Timer>();

        private static bool Update()
        {
            List<Timer> endeds = new List<Timer>();
            foreach (Timer timer in Timer.timers)
            {
                if (timer.timestamp <= Time.time)
                {
                    endeds.Add(timer);
                }
            }
            foreach (Timer timer in endeds)
            {
                timer.callback();
                Timer.Destroy(timer);
            }
            return true;
        }

        private Timer(float time, TimerCallback callback)
        {
            this.timestamp = time + Time.time;
            this.callback = callback;
            this.id = Timer.count++;
            Timer.timers.Add(this);
        }

        internal static Timer Create(float time, TimerCallback callback)
        {
            return new Timer(time, callback);
        }

        internal static void Destroy(Timer timer)
        {
            Timer.timers.Remove(timer);
        }

        internal static int GetRunningTimerCount()
        {
            return Timer.timers.Count;
        }
    }
}
