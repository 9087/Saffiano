using System;
using System.Threading;

namespace Saffiano
{
    public sealed class Application
    {
        public static void Initialize()
        {
        }

        public static void Uninitialize()
        {
        }

        public static void Run()
        {
            const float fps = 60.0f;
            int timePerFrame = (int)(1000.0f / fps);
            while (true)
            {
                int begin = System.Environment.TickCount;
                Application.Update();
                int phase = System.Environment.TickCount - begin;
                if (phase < timePerFrame)
                {
                    Thread.Sleep((int)(timePerFrame - phase));
                }
            };
        }

        public static void Update()
        {
        }

        public static void LoadLevel(string name)
        {
        }

        public static void LoadLevel(Level level)
        {
        }
    }
}
