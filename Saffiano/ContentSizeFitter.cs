using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano
{
    public class ContentSizeFitter : Behaviour, ILayoutController
    {
        public new RectTransform transform => base.transform as RectTransform;

        public void SetLayout()
        {
            ILayoutElement layoutElement = GetComponent<ILayoutElement>();
            transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, layoutElement.preferredWidth);
            transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, layoutElement.preferredHeight);
        }

        public void SetDirty()
        {
            AutoLayout.MarkLayoutForRebuild(this.transform);
        }
    }
}
