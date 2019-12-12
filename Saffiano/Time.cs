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

        internal static void Initialize()
        {
            Time.ticks = DateTime.Now.Ticks;
        }

        internal static void Uninitialize()
        {
        }

        internal static void Update()
        {
            Time.time = (DateTime.Now.Ticks - Time.ticks) / 10000000.0f;
        }
    }
}
