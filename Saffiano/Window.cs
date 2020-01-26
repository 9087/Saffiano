using OpenGL;

namespace Saffiano
{
    internal class Window
    {
        public delegate void KeyboardEventHandler(KeyboardEvent args);
        public static event KeyboardEventHandler KeyboardEvent;

        public delegate void MouseEventHandler(MouseEvent args);
        public static event MouseEventHandler MouseEvent;

        public static Win32Window window
        {
            get;
            private set;
        }

        private static void Initialize()
        {
            window = new Win32Window();
            window.KeyboradEvent += OnWindowKeyboradEventDispatched;
            window.MouseEvent += OnWindowMouseEventDispatched;
        }

        private static void OnWindowMouseEventDispatched(MouseEvent args)
        {
            MouseEvent?.Invoke(args);
        }

        private static void OnWindowKeyboradEventDispatched(KeyboardEvent args)
        {
            KeyboardEvent?.Invoke(args);
        }

        private static void Uninitialize()
        {
            window = null;
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
    }
}
