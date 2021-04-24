using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.UI
{
    public class ContentSizeFitter : Behaviour, ILayoutController
    {
        public enum FitMode
        {
            Unconstrained,
            PreferredSize,
        }

        public new RectTransform transform => base.transform as RectTransform;

        public FitMode horizontalFit { get; set; } = FitMode.PreferredSize;

        public FitMode verticalFit { get; set; } = FitMode.PreferredSize;

        public void SetLayout()
        {
            ILayoutElement layoutElement = GetComponent<ILayoutElement>();
            switch (horizontalFit)
            {
                case FitMode.Unconstrained:
                    break;
                case FitMode.PreferredSize:
                    transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, layoutElement.preferredWidth);
                    break;
                default:
                    throw new NotImplementedException();
            }
            switch (verticalFit)
            {
                case FitMode.Unconstrained:
                    break;
                case FitMode.PreferredSize:
                    transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, layoutElement.preferredHeight);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void SetDirty()
        {
            AutoLayout.MarkLayoutForRebuild(this.transform);
        }
    }
}
