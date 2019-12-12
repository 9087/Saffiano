using System;

namespace Saffiano
{
    public sealed class Debug
    {
        public static void LogFormat(String message, params object[] objects)
        {
            Console.WriteLine("[LOG] {0}", String.Format(message, objects));
        }

        public static void Log(object message)
        {
            Console.WriteLine("[LOG] {0}", message);
        }

        public static void LogErrorFormat(String message, params object[] objects)
        {
            Console.WriteLine("[ERROR] {0}", String.Format(message, objects));
        }

        public static void LogError(object message)
        {
            Console.WriteLine("[ERROR] {0}", message);
        }
    }
}
