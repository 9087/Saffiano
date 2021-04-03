using System.Collections.Generic;
using System.Linq;

namespace Saffiano.UI
{
    public class InputField : Behaviour
    {
        protected static InputField _current = null;

        protected static InputField current
        {
            get => _current;
            private set
            {
                if (_current == value)
                {
                    return;
                }
                var old = _current;
                _current = value;
                if (old == null && _current != null)
                {
                    Input.InputEvent += OnInputEventDispatched;
                }
                if (old != null && _current == null)
                {
                    Input.InputEvent -= OnInputEventDispatched;
                }
            }
        }

        public Text textComponent { get; set; }

        public string text
        {
            get => textComponent.text;
            set { textComponent.text = value; }
        }

        void Awake()
        {
            current = this;
        }

        private static void OnInputEventDispatched(InputEvent args)
        {
            Debug.Assert(current != null);
            if (current.textComponent == null)
            {
                return;
            }
            var buffer = current.text.ToList();
            switch (args)
            {
                case CharEvent charEvent:
                    switch (charEvent.@char)
                    {
                        case '\b':
                            return;
                    }
                    buffer.Add(charEvent.@char);
                    break;
                case KeyboardEvent keyboardEvent:
                    if (keyboardEvent.eventType == KeyboardEventType.KeyUp)
                    {
                        return;
                    }
                    switch (keyboardEvent.keyCode)
                    {
                        case KeyCode.Backspace:
                            if (buffer.Count == 0)
                            {
                                return;
                            }
                            buffer.RemoveAt(buffer.Count - 1);
                            break;
                        default:
                            return;
                    }
                    break;
                default:
                    break;
            }
            current.text = new string(buffer.ToArray());
        }
    }
}
