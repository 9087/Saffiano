namespace Saffiano.UI
{
    public abstract class Graphic : Behaviour
    {
        protected RectTransform rectTransform => transform as RectTransform;

        protected Mesh mesh { get; set; }

        internal abstract Command GenerateCommand();

        protected abstract Mesh OnPopulateMesh(Mesh mesh);

        void LateUpdate()
        {
            mesh = OnPopulateMesh(mesh);
        }
    }
}