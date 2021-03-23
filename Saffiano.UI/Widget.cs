using System;
using System.Collections;
using System.Collections.Generic;

namespace Saffiano.UI
{
    public class Widget : GameObject, IEnumerable<Widget>
    {
        internal new RectTransform transform => base.transform as RectTransform;

        public Widget()
        {
            AddComponent<RectTransform>();
        }

        public Vector3 localPosition
        {
            get => transform.localPosition;
            set { transform.localPosition = value; }
        }

        public Quaternion localRotation
        {
            get => transform.localRotation;
            set { transform.localRotation = value; }
        }

        public Vector3 localScale
        {
            get => transform.localScale;
            set { transform.localScale = value; }
        }

        public Vector2 anchorMax
        {
            get => transform.anchorMax;
            set { transform.anchorMax = value; }
        }

        public Vector2 anchorMin
        {
            get => transform.anchorMin;
            set { transform.anchorMin = value; }
        }

        public Vector2 offsetMax
        {
            get => transform.offsetMax;
            set { transform.offsetMax = value; }
        }

        public Vector2 offsetMin
        {
            get => transform.offsetMin;
            set { transform.offsetMin = value; }
        }

        public Vector2 pivot
        {
            get => transform.pivot;
            set { transform.pivot = value; }
        }

        public IEnumerator<Widget> GetChildren()
        {
            foreach (var child in transform)
            {
                var gameObject = child.gameObject;
                if (!(gameObject is Widget))
                {
                    continue;
                }
                yield return gameObject as Widget;
            }
        }

        public IEnumerator<Widget> GetEnumerator()
        {
            return GetChildren();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Widget this[params Widget[] widgets]
        {
            get
            {
                foreach (var widget in widgets)
                {
                    widget.transform.parent = this.transform;
                }
                return this;
            }
        }
    }
}
