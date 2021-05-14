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

        public virtual Text textComponent
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
                    _textComponent.MeshPopulatedChanged -= OnMeshPopulatedChanged;
                }
                _textComponent = value;
                _textComponent.MeshPopulatedChanged += OnMeshPopulatedChanged;
            }
        }

        private string _text = string.Empty;

        public virtual string text
        {
            get => _text;
            set
            {
                if (_text == value)
                {
                    return;
                }
                _text = value;
                caretPosition = _text.Length;
                UpdateText();
            }
        }

        private string _header = string.Empty;

        public string header
        {
            get => _header;
            set
            {
                if (_header == value)
                {
                    return;
                }
                _header = value;
                UpdateText();
            }
        }

        private void UpdateText()
        {
            textComponent.text = _header + _text;
        }

        protected Image caret { get; set; }

        protected Vector2 caretSize { get; set; } = new Vector2(10, 4);

        protected int caretPosition { get; set; }

        protected Coroutine _blinking = null;

        void Awake()
        {
            current = this;
            var g = new GameObject();
            g.AddComponent<RectTransform>().parent = this.transform;
            g.AddComponent<CanvasRenderer>();
            caret = g.AddComponent<Image>();
            caret.sprite = Sprite.Create(Texture.whiteTexture);
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
                _textComponent.MeshPopulatedChanged -= OnMeshPopulatedChanged;
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
            if (current.textComponent == null || !current.isActiveAndEnabled)
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
                        case '\n':
                        case '\r':
                            return;
                    }
                    buffer.Insert(current.caretPosition, charEvent.@char);
                    current.caretPosition += 1;
                    break;
                case KeyboardEvent keyboardEvent:
                    if (keyboardEvent.eventType == KeyboardEventType.KeyUp)
                    {
                        return;
                    }
                    switch (keyboardEvent.keyCode)
                    {
                        case KeyCode.Backspace:
                            if (buffer.Count != 0 && current.caretPosition >= 1)
                            {
                                buffer.RemoveAt(current.caretPosition - 1);
                                current.caretPosition -= 1;
                            }
                            break;
                        case KeyCode.LeftArrow:
                            current.caretPosition = Mathf.Max(current.caretPosition - 1, 0);
                            current.UpdateCaret();
                            break;
                        case KeyCode.RightArrow:
                            current.caretPosition = Mathf.Min(current.caretPosition + 1, buffer.Count);
                            current.UpdateCaret();
                            break;
                        case KeyCode.Home:
                            current.caretPosition = 0;
                            current.UpdateCaret();
                            break;
                        case KeyCode.End:
                            current.caretPosition = buffer.Count;
                            current.UpdateCaret();
                            break;
                        default:
                            return;
                    }
                    break;
                default:
                    break;
            }
            current._text = new string(buffer.ToArray());
            current.UpdateText();
        }

        protected virtual void OnMeshPopulatedChanged(Mesh mesh)
        {
            UpdateCaret();
        }

        protected virtual void UpdateCaret()
        {
            var transform = caret.transform as RectTransform;
            Vector2 endpoint = textComponent.carets[caretPosition + _header.Length];
            transform.offsetMin = endpoint + new Vector2(0, -caretSize.y * 0.5f - textComponent.font.descender);
            transform.offsetMax = endpoint + new Vector2(+caretSize.x, caretSize.y * 0.5f - textComponent.font.descender);
            _blinking?.Interrupt();
            _blinking = StartCoroutine(Blinking());
        }
    }
}
