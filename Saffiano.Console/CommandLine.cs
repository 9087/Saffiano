using Saffiano.Widgets;

namespace Saffiano.Console
{
    internal class CommandLineInputHandler : Behaviour
    {
        public delegate void TextEnteredHandler();
        public event TextEnteredHandler TextEntered;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                TextEntered();
            }
        }
    }

    public class CommandLine : Widget
    {
        private Font font = Font.CreateDynamicFontFromOSFont("../../../../Resources/JetBrainsMono-Regular.ttf", 16);
        internal ListView listView = null;
        internal TextField textField = null;
        internal ImageView cursor = null;

        public CommandLine()
        {
            var _ = this
            [
                listView = new ListView()
                {
                    anchorMin = new Vector2(0, 0),
                    anchorMax = new Vector2(1, 1),
                    offsetMin = new Vector2(0, 0),
                    offsetMax = new Vector2(0, 0),
                    itemsMargin = -2,
                }[
                    textField = new TextField()
                    {
                        font = font,
                    }
                ]
            ];
            this.AddComponent<CommandLineInputHandler>().TextEntered += OnCommandLineTextEntered;
            listView.GetComponent<UI.LinearLayoutGroup>().childControlHeight = false;
            var textComponent = textField.GetComponent<UI.Text>();
            textField.size = new Vector2(0, font.lineHeight);
            textComponent.alignment = UI.TextAnchor.MiddleLeft;

        }

        private void OnCommandLineTextEntered()
        {
            WriteLine(textField.text);
            textField.text = "";
        }

        public void WriteLine(string message, params object[] objects)
        {
            string text = string.Format(message, objects);
            var line = new Text() { text = text, font = font, };
            listView.AddChild(line);
            line.size = new Vector2(0, font.lineHeight);
            line.transform.SetSiblingIndex(listView.GetChildrenCount() - 2);
            var textComponent = line.GetComponent<UI.Text>();
            textComponent.alignment = UI.TextAnchor.MiddleLeft;
        }
    }
}
