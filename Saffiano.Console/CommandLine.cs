using Saffiano.Widgets;

namespace Saffiano.Console
{
    internal class CommandLineInputHandler : Behaviour
    {
        public CommandLine commandLine { get; set; }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                commandLine.WriteLine(commandLine.textField.text);
                commandLine.textField.text = "";
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
            this.transform.offsetMax = new Vector2(20, 3);
        }
    }

    public class CommandLine : Widget
    {
        private Font _font = Font.CreateDynamicFontFromOSFont("../../../../Resources/JetBrainsMono-Regular.ttf", 16);
        internal ListView listView = null;
        internal TextField textField = null;
        internal ImageView cursor = null; 

        public Font font
        {
            get => _font;
            set
            {
                if (_font == value)
                {
                    return;
                }
                _font = value;
            }
        }

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
                    ]
                ]
            ];
            this.AddComponent<CommandLineInputHandler>().commandLine = this;
            cursor.AddComponent<CursorController>();
            cursor.sprite = Sprite.Create(Texture.white);
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
