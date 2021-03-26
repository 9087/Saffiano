using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano
{
    public interface ILayoutElement
    {
        float flexibleHeight { get; }

        float flexibleWidth { get; }

        float minWidth { get; }

        float minHeight { get; }

        float preferredHeight { get; }

        float preferredWidth { get; }

        void CalculateLayoutInput();
    }
}
