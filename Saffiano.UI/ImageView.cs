using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.UI
{
    public class ImageView : Widget
    {
        public ImageView()
        {
            AddComponent<CanvasRenderer>();
            AddComponent<Image>();
        }
    }
}
