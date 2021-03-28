using Saffiano.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.Widgets
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
