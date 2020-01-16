using OpenGL;

namespace Saffiano
{
    internal class Window
    {
        private static bool available;

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
            available = true;
            window = new Win32Window();
            window.KeyboradEvent += OnWindowKeyboradEventDispatched;
        }

        private static void OnWindowKeyboradEventDispatched(KeyboardEvent args)
        {
            KeyboardEvent?.Invoke(args);
        }

        private static void Uninitialize()
        {
            window = null;
            available = false;
        }

        private static bool Update()
        {
            return window.ProcessMessages();
        }

        public static bool GetKey(KeyCode key)
        {
            return window.GetKey(key);
        }
    }
}
