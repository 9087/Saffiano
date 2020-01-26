using System.Collections.Generic;

namespace Saffiano
{
    internal class InputContext
    {
        internal List<KeyCode> downs = new List<KeyCode>();
        internal List<KeyCode> ups = new List<KeyCode>();
    };

    public sealed class Input
    {
        private static InputContext inputContext = new InputContext();
        private static InputContext nextFrameInputContext = new InputContext();

        public static Vector3 mousePosition
        {
            get
            {
                return Window.GetMousePosition();
            }
        }

        private static void Initialize()
        {
            Window.KeyboardEvent += OnWindowKeyboardEventDispatched;
            Window.MouseEvent += OnWindowMouseEventDispatched;
        }

        private static void OnWindowMouseEventDispatched(MouseEvent args)
        {
            switch (args.eventType)
            {
                case MouseEventType.MouseDown:
                    Input.nextFrameInputContext.downs.Add(args.keyCode);
                    break;
                case MouseEventType.MouseUp:
                    Input.nextFrameInputContext.ups.Add(args.keyCode);
                    break;
            };
        }

        private static void Uninitialize()
        {
            Window.MouseEvent -= OnWindowMouseEventDispatched;
            Window.KeyboardEvent -= OnWindowKeyboardEventDispatched;
        }

        private static void OnWindowKeyboardEventDispatched(KeyboardEvent args)
        {
            switch (args.eventType)
            {
                case KeyboardEventType.KeyDown:
                    Input.nextFrameInputContext.downs.Add(args.keyCode);
                    break;
                case KeyboardEventType.KeyUp:
                    Input.nextFrameInputContext.ups.Add(args.keyCode);
                    break;
            };
        }

        public static bool GetKey(KeyCode key)
        {
            return Window.GetKey(key);
        }

        public static bool GetKeyDown(KeyCode key)
        {
            return Input.inputContext.downs.Contains(key);
        }

        public static bool GetKeyUp(KeyCode key)
        {
            return Input.inputContext.ups.Contains(key);
        }

        private static bool Update()
        {
            Input.inputContext = Input.nextFrameInputContext;
            Input.nextFrameInputContext = new InputContext();
            return true;
        }
    }
}
