using Saffiano.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.Widgets
{
    public class ImageView : Widget
    {
        protected Image imageComponent = null;

        public ImageView()
        {
            AddComponent<CanvasRenderer>();
            imageComponent = AddComponent<Image>();
        }

        public Sprite sprite
        {
            get => imageComponent.sprite;
            set
            {
                if (imageComponent.sprite == value)
                {
                    return;
                }
                imageComponent.sprite = value;
            }
        }
    }
}
