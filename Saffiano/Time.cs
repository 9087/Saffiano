using System;

namespace Saffiano
{
    public sealed class Time
    {
        internal static long ticks;

        public static float time
        {
            get;
            private set;
        }

        private static void Initialize()
        {
            Time.ticks = DateTime.Now.Ticks;
        }

        private static void Uninitialize()
        {
        }

        private static bool Update()
        {
            Time.time = (DateTime.Now.Ticks - Time.ticks) / 10000000.0f;
            return true;
        }
    }
}
