﻿using Saffiano.Rendering;
using Saffiano.UI;

namespace Saffiano
{
    public sealed class CanvasRenderer : Renderer
    {
        private RectTransform rectTransform => gameObject.GetComponent<RectTransform>();

        protected override void OnRender()
        {
            foreach (var graphic in gameObject.GetComponents<Graphic>())
            {
                Command command = graphic.GenerateCommand();
                if (command == null)
                {
                    continue;
                }
                RenderPipeline.Draw(command);
            }
        }

        internal override void OnComponentAdded(GameObject gameObject)
        {
            gameObject.layer = LayerMask.NameToLayer("UI");
            base.OnComponentAdded(gameObject);
        }
    }
}
