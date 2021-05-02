using System.Collections;
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

        private Text _textComponent = null;

        public Text textComponent
        {
            get => _textComponent;
            set
            {
                if (value == _textComponent)
                {
                    return;
                }
                if (_textComponent != null)
                {
                    _textComponent.EndpointChanged -= OnTextEndpointChanged;
                }
                _textComponent = value;
                _textComponent.EndpointChanged += OnTextEndpointChanged;
            }
        }

        public string text
        {
            get => textComponent.text;
            set
            {
                textComponent.text = value;
                _blinking?.Interrupt();
                _blinking = StartCoroutine(Blinking());
            }
        }

        private Image caret { get; set; }

        private Vector2 caretSize { get; set; } = new Vector2(10, 4);

        private Coroutine _blinking = null;

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

        void Start()
        {
            _blinking = StartCoroutine(Blinking());
        }

        public IEnumerator Blinking()
        {
            while (true)
            {
                caret.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.5f);
                caret.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.5f);
            }
        }

        void OnDestroy()
        {
            if (_textComponent != null)
            {
                _textComponent.EndpointChanged -= OnTextEndpointChanged;
                _textComponent = null;
            }
            Object.Destroy(caret.gameObject);
            if (current == this)
            {
                current = null;
            }
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

        private void OnTextEndpointChanged()
        {
            var transform = caret.transform as RectTransform;
            transform.offsetMin = textComponent.endpoint + new Vector2(0, -caretSize.y * 0.5f);
            transform.offsetMax = textComponent.endpoint + new Vector2(+caretSize.x, caretSize.y * 0.5f);
        }
    }
}
