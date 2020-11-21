using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Saffiano
{
    internal static class TypeExtend
    {
        public static MethodInfo GetPrivateStaticMethod(this Type type, string methodName)
        {
            return type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
        }
    }

    public sealed class Application
    {
        private static Type[] systems = new Type[]
        {
            typeof(Time),
            typeof(Transform),
            typeof(Timer),
            typeof(Window),
            typeof(Canvas),
            typeof(Input),
            typeof(Rendering),
            typeof(Resources),
        };

        private static MethodInfo[] updaters;

        public static int targetFrameRate { get; set; } = 60;

        static Application()
        {
            List<MethodInfo> methodInfos = new List<MethodInfo>();
            foreach (var system in systems)
            {
                var methodInfo = system.GetPrivateStaticMethod("Update");
                if (methodInfo != null)
                {
                    methodInfos.Add(methodInfo);
                }
            }
            updaters = methodInfos.ToArray();
        }

        public static void Initialize()
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            ScriptableMaterial.Prebuild();
            foreach (var system in systems)
            {
                system.GetPrivateStaticMethod("Initialize")?.Invoke(null, new object[] { });
            }
        }

        public static void Uninitialize()
        {
            foreach (var system in systems.Reverse())
            {
                system.GetPrivateStaticMethod("Uninitialize")?.Invoke(null, new object[] { });
            }
        }

        public static bool Update()
        {
            foreach (var updater in updaters)
            {
                try
                {
                    bool success = (bool)updater.Invoke(null, new object[] { });
                    if (!success)
                    {
                        return false;
                    }
                }
                catch (TargetInvocationException tie)
                {
                    Debug.LogException(tie);
                    return false;
                }
            }
            return true;
        }

        public static void Run()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            while (true)
            {
                double begin = Window.GetTickCount();
                if (!Update())
                {
                    break;
                }
                double millisecondsPerFrame = 1000.0 / (double)targetFrameRate;
                double phase = Window.GetTickCount() - begin;
                if (phase < millisecondsPerFrame)
                {
                    Window.Sleep(millisecondsPerFrame - phase);
                }
            };
        }

        public static void LoadLevel(string name)
        {
        }

        public static void LoadLevel(Level level)
        {
        }
    }
}
