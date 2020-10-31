namespace Saffiano
{
    public sealed class CanvasRenderer : Renderer
    {
        private RectTransform rectTransform => gameObject.GetComponent<RectTransform>();

        protected override void OnRender()
        {
            foreach (var graphic in gameObject.GetComponents<Graphic>())
            {
                Command command = graphic.CreateCommand(rectTransform);
                if (command == null)
                {
                    continue;
                }
                Rendering.Draw(command);
            }
        }

        internal override void OnComponentAdded(GameObject gameObject)
        {
            gameObject.layer = LayerMask.NameToLayer("UI");
            base.OnComponentAdded(gameObject);
        }
    }
}
