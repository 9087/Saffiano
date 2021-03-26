using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saffiano
{
    internal class AutoLayout
    {
        private static HashSet<RectTransform> dirtyTransforms = new HashSet<RectTransform>();

        private static bool Update()
        {
            while (dirtyTransforms.Count != 0)
            {
                var transform = dirtyTransforms.First();
                PerformCalculateLayoutInput(transform);
                PerformSetLayout(transform);
                FlushSubTreeTransform(transform);
            }
            return true;
        }

        private static void PerformCalculateLayoutInput(Transform transform)
        {
            foreach (var child in transform)
            {
                PerformCalculateLayoutInput(child);
            }
            transform.GetComponent<ILayoutElement>()?.CalculateLayoutInput();
        }

        private static void PerformSetLayout(Transform transform)
        {
            transform.GetComponent<ILayoutController>()?.SetLayout();
            foreach (var child in transform)
            {
                PerformSetLayout(child);
            }
        }

        internal static void MarkLayoutForRebuild(RectTransform transform)
        {
            Debug.Assert(transform != null);
            var parent = transform.parent as RectTransform;
            if (parent != null && parent.GetComponent<LayoutGroup>() != null)
            {
                transform = parent;
            }
            dirtyTransforms.Add(transform);
        }

        private static void FlushSubTreeTransform(Transform transform)
        {
            if (transform is RectTransform)
            {
                RectTransform rectTransform = transform as RectTransform;
                if (dirtyTransforms.Contains(rectTransform))
                {
                    dirtyTransforms.Remove(rectTransform);
                }
            }
            foreach (var child in transform)
            {
                FlushSubTreeTransform(child);
            }
        }
    }
}
