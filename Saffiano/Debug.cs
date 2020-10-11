using System;
using System.Reflection;

namespace Saffiano
{
    public sealed class Debug
    {
        public static void LogFormat(String message, params object[] objects)
        {
            Log(string.Format(message, objects));
        }

        public static void Log(object message)
        {
            Console.WriteLine("[LOG] {0}", message);
        }

        public static void LogErrorFormat(String message, params object[] objects)
        {
            LogError(string.Format(message, objects));
        }

        public static void LogError(object message)
        {
            Console.WriteLine("[ERROR] {0}", message);
            throw new Exception(message.ToString());
        }

        public static void LogWarningFormat(String message, params object[] objects)
        {
            LogWarning(string.Format(message, objects));
        }

        public static void LogWarning(object message)
        {
            Console.WriteLine("[WARNING] {0}", message);
        }

        public static void LogException(Exception exception)
        {
            Console.WriteLine(string.Format("{0}\n{1}", exception.ToString(), exception.StackTrace));
        }

        public static void LogException(TargetInvocationException tie)
        {
            LogException(tie.InnerException);
        }

        public static void Assert(bool condition, object message=null)
        {
#if DEBUG
            if (!condition)
            {
                if (message == null)
                {
                    throw new Exception();
                }
                else
                {
                    throw new Exception(message.ToString());
                }
            }
#endif
        }
    }
}
