using Saffiano.UI;

namespace Saffiano.Widgets
{
    public class TextField : Text
    {
        public TextField()
        {
            AddComponent<UI.InputField>().textComponent = this.textComponent;
        }
    }
}
