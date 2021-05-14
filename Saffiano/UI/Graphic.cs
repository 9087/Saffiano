namespace Saffiano.UI
{
    public abstract class Graphic : Behaviour
    {
        protected RectTransform rectTransform => transform as RectTransform;

        protected Mesh mesh { get; set; }

        public virtual Color color { get; set; }

        internal abstract Command GenerateCommand();

        protected abstract Mesh OnPopulateMesh(Mesh mesh);
    }
}