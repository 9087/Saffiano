using Saffiano.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.Widgets
{
    public class TextView : Widget
    {
        private Text textComponent = null;

        public TextView()
        {
            AddComponent<CanvasRenderer>();
            this.textComponent = AddComponent<Text>();
            this.font = Font.CreateDynamicFontFromOSFont("../../../../Resources/JetBrainsMono-Regular.ttf", 22);
            AddComponent<ContentSizeFitter>();
        }

        public string text
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
