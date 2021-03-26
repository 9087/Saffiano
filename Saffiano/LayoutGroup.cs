using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano
{
    public abstract class LayoutGroup : Behaviour, ILayoutController, ILayoutElement
    {
        public new RectTransform transform => base.transform as RectTransform;

        public float flexibleWidth => throw new NotImplementedException();

        public float flexibleHeight => throw new NotImplementedException();

        public float minWidth => throw new NotImplementedException();

        public float minHeight => throw new NotImplementedException();

        public float preferredHeight { get; protected set; }

        public float preferredWidth { get; protected set; }

        public abstract void CalculateLayoutInput();

        public abstract void SetLayout();

        public void SetDirty()
        {
            AutoLayout.MarkLayoutForRebuild(this.transform);
        }

        protected void OnRectTransformDimensionsChange()
        {
            AutoLayout.MarkLayoutForRebuild(this.transform);
        }
    }
}
