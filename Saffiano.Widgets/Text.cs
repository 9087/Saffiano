using Saffiano.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.Widgets
{
    public class Text : Widget
    {
        protected UI.Text textComponent = null;

        public Text()
        {
            AddComponent<CanvasRenderer>();
            this.textComponent = AddComponent<UI.Text>();
            this.font = Font.CreateDynamicFontFromOSFont("../../../../Resources/JetBrainsMono-Regular.ttf", 22);
        }

        public virtual string text
        {
            get => textComponent.text;
            set { textComponent.text = value; }
        }

        public Font font
        {
            get => textComponent.font;
            set { textComponent.font = value; }
        }
    }
}
