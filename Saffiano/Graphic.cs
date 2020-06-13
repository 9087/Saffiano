namespace Saffiano
{
    public abstract class Graphic : Behaviour
    {
        internal abstract Command CreateCommand(RectTransform rectTransform);
    }
}
