using Saffiano.UI;
using System.Linq;

namespace Saffiano.Widgets
{
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
            set => inputField.header = value;
        }

        public override string text
        {
            get => inputField.text;
            set => inputField.text = value;
        }

        protected override void UpdateCascadeColor()
        {
            base.UpdateCascadeColor();
            inputField?.caret?.GetComponents<UI.Graphic>()
                .ToList()
                .ForEach((x) => { x.color = _finalColor; });
        }
    }
}
