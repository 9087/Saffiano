using System.Collections.Generic;
using System.Linq;

namespace Saffiano.UI
{
    public class InputField : Behaviour
    {
        protected static InputField current { get; private set; } = null;

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

        void Update()
        {
            if (current != this || textComponent == null)
            {
                return;
            }
            text = text + Input.GetChars();
        }
    }
}
