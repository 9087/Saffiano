using OpenGL;
using System.Text;

namespace Saffiano
{
    internal class Window
    {
        public static event KeyboardEventHandler KeyboardEvent;
        public static event MouseEventHandler MouseEvent;
        public static event ResizedEventHandler Resized;
        public static event CreatedEventHandler Created;
        public static event CharEventHandler CharEvent;

        public static Win32Window window
        {
            get;
            private set;
        }

        internal static double GetTickCount()
        {
            return Win32Window.GetTickCount();
        }

        internal static void Sleep(double millisecondsTimeout)
        {
            Win32Window.Sleep(millisecondsTimeout);
        }

        private static void Initialize()
        {
            window = new Win32Window();
            window.KeyboradEvent += OnWindowKeyboradEventDispatched;
            window.MouseEvent += OnWindowMouseEventDispatched;
            window.Resized += OnWindowResized;
            window.Created += OnWindowCreated;
            window.CharEvent += OnWindowCharEventDispatched;
        }

        private static void Uninitialize()
        {
            window.KeyboradEvent -= OnWindowKeyboradEventDispatched;
            window.MouseEvent -= OnWindowMouseEventDispatched;
            window.Resized -= OnWindowResized;
            window.Created -= OnWindowCreated;
            window.CharEvent -= OnWindowCharEventDispatched;
            window = null;
        }

        private static void OnWindowCreated()
        {
            Created?.Invoke();
        }

        private static void OnWindowResized(Vector2 size)
        {
            Resized?.Invoke(size);
        }

        private static void OnWindowMouseEventDispatched(MouseEvent args)
        {
            MouseEvent?.Invoke(args);
        }

        private static void OnWindowKeyboradEventDispatched(KeyboardEvent args)
        {
            KeyboardEvent?.Invoke(args);
        }

        private static void OnWindowCharEventDispatched(CharEvent args)
        {
            CharEvent?.Invoke(args);
        }

        private static bool Update()
        {
            return window.ProcessMessages();
        }

        public static bool GetKey(KeyCode key)
        {
            return window.GetKey(key);
        }

        public static Vector3 GetMousePosition()
        {
            return window.GetMousePosition();
        }

        public static Vector2 GetSize()
        {
            return window.GetSize();
        }
    }
}
