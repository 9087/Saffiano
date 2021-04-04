using System;
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
        private static Queue<InputEvent> inputEventQueue = new Queue<InputEvent>();

        internal static event InputEventHandler InputEvent;

        public static Vector3 mousePosition
        {
            get;
            private set;
        }

        private static void Initialize()
        {
            Window.KeyboardEvent += OnWindowKeyboardEventDispatched;
            Window.MouseEvent += OnWindowMouseEventDispatched;
            Window.CharEvent += OnWindowCharEventDispatched;
            mousePosition = Window.GetMousePosition();
        }

        private static void Uninitialize()
        {
            Window.CharEvent -= OnWindowCharEventDispatched;
            Window.MouseEvent -= OnWindowMouseEventDispatched;
            Window.KeyboardEvent -= OnWindowKeyboardEventDispatched;
        }

        private static void OnWindowCharEventDispatched(CharEvent args)
        {
            inputEventQueue.Enqueue(args);
        }

        private static void OnWindowMouseEventDispatched(MouseEvent args)
        {
            inputEventQueue.Enqueue(args);
        }

        private static void OnWindowKeyboardEventDispatched(KeyboardEvent args)
        {
            inputEventQueue.Enqueue(args);
        }

        private static void ProcessInputEventQueue()
        {
            while (inputEventQueue.Count != 0)
            {
                var inputEvent = inputEventQueue.Dequeue();
                switch (inputEvent)
                {
                    case CharEvent charEvent:
                        break;
                    case MouseEvent mouseEvent:
                        switch (mouseEvent.eventType)
                        {
                            case MouseEventType.MouseDown:
                                Input.nextFrameInputContext.downs.Add(mouseEvent.keyCode);
                                break;
                            case MouseEventType.MouseUp:
                                Input.nextFrameInputContext.ups.Add(mouseEvent.keyCode);
                                break;
                        };
                        break;
                    case KeyboardEvent keyboardEvent:
                        switch (keyboardEvent.eventType)
                        {
                            case KeyboardEventType.KeyDown:
                                Input.nextFrameInputContext.downs.Add(keyboardEvent.keyCode);
                                break;
                            case KeyboardEventType.KeyUp:
                                Input.nextFrameInputContext.ups.Add(keyboardEvent.keyCode);
                                break;
                        };
                        break;
                    default:
                        throw new NotImplementedException();
                }
                InputEvent?.Invoke(inputEvent);
            }
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
            ProcessInputEventQueue();
            inputContext = nextFrameInputContext;
            nextFrameInputContext = new InputContext();
            mousePosition = Window.GetMousePosition();
            return true;
        }

        public static bool GetMouseButton(int button)
        {
            if (button < 0 || button > KeyCode.Mouse6 - KeyCode.Mouse0)
            {
                throw new Exception();
            }
            return Window.GetKey(KeyCode.Mouse0 + button);
        }

        public static bool GetMouseButtonDown(int button)
        {
            if (button < 0 || button > KeyCode.Mouse6 - KeyCode.Mouse0)
            {
                throw new Exception();
            }
            return Input.inputContext.downs.Contains(KeyCode.Mouse0 + button);
        }

        public static bool GetMouseButtonUp(int button)
        {
            if (button < 0 || button > KeyCode.Mouse6 - KeyCode.Mouse0)
            {
                throw new Exception();
            }
            return Input.inputContext.ups.Contains(KeyCode.Mouse0 + button);
        }
    }
}
