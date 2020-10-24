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

        public static float deltaTime
        {
            get;
            private set;
        }

        public static uint frameCount
        {
            get;
            private set;
        }

        private static void Initialize()
        {
            Time.ticks = DateTime.Now.Ticks;
            time = 0;
            frameCount = 0;
        }

        private static void Uninitialize()
        {
        }

        private static bool Update()
        {
            var current = (DateTime.Now.Ticks - Time.ticks) / 10000000.0f;
            deltaTime = current - time;
            time = current;
            frameCount++;
            return true;
        }
    }
}
