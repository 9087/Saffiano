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

        private static void Initialize()
        {
            Window.KeyboardEvent += OnWindowKeyboardEventDispatched;
        }

        private static void Uninitialize()
        {
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
