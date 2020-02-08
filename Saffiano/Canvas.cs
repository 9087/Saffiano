using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano
{
    public enum RenderMode
    {
        ScreenSpaceOverlay = 0,
        ScreenSpaceCamera = 1,
        WorldSpace = 2
    }

    public sealed class Canvas : Behaviour
    {
        public RenderMode renderMode { get; set; } = RenderMode.ScreenSpaceOverlay;

        internal Camera targetCamera => Camera.main;

        internal static List<Canvas> overlayCanvases = new List<Canvas>();
    }
}
