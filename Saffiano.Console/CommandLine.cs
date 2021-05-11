using Saffiano.Widgets;
using System.Collections;
using System.Collections.Generic;

namespace Saffiano.Console
{
    internal class CommandLineInputHandler : Behaviour
    {
        public delegate void TextEnteredHandler();
        public event TextEnteredHandler TextEntered;

        public delegate void HistoryUpHandler();
        public event HistoryUpHandler HistoryUp;

        public delegate void HistoryDownHandler();
        public event HistoryDownHandler HistoryDown;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                TextEntered?.Invoke();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                HistoryDown?.Invoke();
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                HistoryUp?.Invoke();
            }
        }
    }

    public class CommandLine : Widget
    {
        private Font font = Font.CreateDynamicFontFromOSFont("fonts/JetBrainsMono-Regular.ttf", 16);
        internal ListView listView = null;
        internal TextField textField = null;
        internal ImageView cursor = null;
        public delegate void TextEnteredHandler(string text);
        public event TextEnteredHandler TextEntered;
        private List<string> histroy = new List<string>();
        private int histroyIndex = 0;

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
                    itemsMargin = 0,
                }[
                    textField = new TextField()
                    {
                        font = font,
                    }
                ]
            ];
            var inputHandler = this.AddComponent<CommandLineInputHandler>();
            inputHandler.TextEntered += OnCommandLineTextEntered;
            inputHandler.HistoryUp += OnCommandLineHistoryUp;
            inputHandler.HistoryDown += OnCommandLineHistoryDown;
            listView.GetComponent<UI.LinearLayoutGroup>().childControlHeight = false;
            var textComponent = textField.GetComponent<UI.Text>();
            textField.size = new Vector2(0, font.lineHeight);
            textComponent.alignment = UI.TextAnchor.MiddleLeft;
        }

        private void OnCommandLineHistoryDown()
        {
            histroyIndex -= 1;
            histroyIndex = Mathf.Max(1, histroyIndex);
            if (histroy.Count != 0)
            {
                textField.text = histroy[histroy.Count - histroyIndex];
            }
        }

        private void OnCommandLineHistoryUp()
        {
            histroyIndex += 1;
            histroyIndex = Mathf.Min(histroy.Count, histroyIndex);
            if (histroyIndex != 0)
            {
                textField.text = histroy[histroy.Count - histroyIndex];
            }
        }

        private void OnCommandLineTextEntered()
        {
            WriteLine(textField.GetComponent<UI.Text>().text);
            var text = textField.text;
            textField.text = "";
            if (text.Length != 0)
            {
                histroy.Add(text);
            }
            histroyIndex = 0;
            TextEntered?.Invoke(text);
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

        public virtual void SetInputActive(bool active)
        {
            textField.SetActive(active);
        }
    }
}
