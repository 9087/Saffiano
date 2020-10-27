﻿namespace Saffiano
{
    public abstract class Graphic : Behaviour
    {
        protected Mesh mesh = null;

        internal abstract Command CreateCommand(RectTransform rectTransform);
    }
}