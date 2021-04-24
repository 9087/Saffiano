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

        private Image caret { get; set; }

        private Vector2 caretSize { get; set; } = new Vector2(10, 3);

        void Awake()
        {
            current = this;
            var g = new GameObject();
            g.AddComponent<RectTransform>().parent = this.transform;
            g.AddComponent<CanvasRenderer>();
            caret = g.AddComponent<Image>();
            caret.sprite = Sprite.Create(Texture.white);
            var transform = caret.transform as RectTransform;
            transform.anchorMin = new Vector2(0, 1);
            transform.anchorMax = new Vector2(0, 1);
            transform.pivot = new Vector2(0, 0);
        }

        void OnDestroy()
        {
            Object.Destroy(caret.gameObject);
            if (current == this)
            {
                current = null;
            }
        }

        void Update()
        {
            var transform = caret.transform as RectTransform;
            transform.offsetMin = textComponent.endpoint;
            transform.offsetMax = caretSize + textComponent.endpoint;
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
