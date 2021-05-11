using System;
using System.Reflection;
using System.Text;

namespace Saffiano
{
    public sealed class Debug
    {
        private enum LogType
        {
            Info = ConsoleColor.Gray,
            Warning = ConsoleColor.DarkYellow,
            Error = ConsoleColor.Red,
        }

        static Debug()
        {
            Console.OutputEncoding = Encoding.Unicode;
        }

        private static void WriteLineInternal(LogType logType, object message)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = (ConsoleColor)logType;
            string head = string.Format("{0} {1,7} {2,-7} ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), Time.frameCount, logType.ToString().ToUpper());
            Console.WriteLine("{0}{1}", head, message.ToString().Replace("\n", "\n" + head));
            Console.ForegroundColor = old;
        }

        public static void LogFormat(String message, params object[] objects)
        {
            Log(string.Format(message, objects));
        }

        public static void Log(object message)
        {
            WriteLineInternal(LogType.Info, message);
        }

        public static void LogErrorFormat(String message, params object[] objects)
        {
            LogError(string.Format(message, objects));
        }

        public static void LogError(object message)
        {
            WriteLineInternal(LogType.Error, message);
            throw new Exception(message.ToString());
        }

        public static void LogWarningFormat(String message, params object[] objects)
        {
            LogWarning(string.Format(message, objects));
        }

        public static void LogWarning(object message)
        {
            WriteLineInternal(LogType.Warning, message);
        }

        public static void LogException(Exception exception)
        {
            WriteLineInternal(LogType.Error, string.Format("{0}\n{1}", exception.ToString(), exception.StackTrace));
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
                throw message == null ? new Exception() : new Exception(message.ToString());
            }
#endif
        }
    }
}
