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

    internal class CursorController : Behaviour
    {
        public new RectTransform transform => base.transform as RectTransform;

        void Awake()
        {
            this.transform.anchorMin = new Vector2(1.0f, 0);
            this.transform.anchorMax = new Vector2(1.0f, 0);
            this.transform.pivot = new Vector2(0, 0);
            this.transform.offsetMin = new Vector2(0, 0);
            this.transform.offsetMax = new Vector2(10, 3);
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
                    }[
                        cursor = new ImageView()
                        {
                            sprite = Sprite.Create(Texture.white)
                        }
                    ]
                ]
            ];
            this.AddComponent<CommandLineInputHandler>().TextEntered += OnCommandLineTextEntered;
            cursor.AddComponent<CursorController>();
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
            line.transform.SetSiblingIndex(listView.GetChildrenCount() - 2);
        }
    }
}
