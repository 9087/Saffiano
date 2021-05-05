using Saffiano.UI;

namespace Saffiano.Widgets
{
    internal class InputField : UI.InputField
    {
        private string _header = string.Empty;
        private string _text = string.Empty;

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

        public override string text
        {
            get => _text;
            set
            {
                if (_text == value)
                {
                    return;
                }
                _text = value;
                UpdateText();
            }
        }

        public override UI.Text textComponent
        {
            get => base.textComponent;
            set
            {
                base.textComponent = value;
                UpdateText();
            }
        }

        private void UpdateText()
        {
            if (base.textComponent == null)
            {
                return;
            }
            base.text = _header + _text;
        }
    }

    public class TextField : Text
    {
        private InputField inputField = null;

        public TextField()
        {
            inputField = AddComponent<InputField>();
            inputField.textComponent = this.textComponent;
        }

        public string header
        {
            get => inputField.header;
            set
            {
                inputField.header = value;
            }
        }

        public override string text { get => inputField.text; set => inputField.text = value; }
    }
}
