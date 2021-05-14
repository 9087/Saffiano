using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Saffiano.Widgets
{
    class WidgetTransform : RectTransform
    {
        public delegate void ChildAddedHandler(Transform child);
        public ChildAddedHandler ChildAdded;

        public delegate void ChildRemovedHandler(Transform child);
        public ChildRemovedHandler ChildRemoved;

        protected override void OnChildAdded(Transform child)
        {
            base.OnChildAdded(child);
            ChildAdded?.Invoke(child);
        }

        protected override void OnChildRemoved(Transform child)
        {
            base.OnChildRemoved(child);
            ChildRemoved?.Invoke(child);
        }
    }

    public class Widget : GameObject, IEnumerable<Widget>
    {
        internal new WidgetTransform transform => base.transform as WidgetTransform;

        public Widget()
        {
            AddComponent<WidgetTransform>();
            transform.ChildAdded += OnChildAdded;
            transform.ChildRemoved += OnChildRemoved;
            UpdateCascadeColor();
        }

        protected virtual void OnChildAdded(Transform child)
        {
            (child.gameObject as Widget)?.UpdateCascadeColor();
        }

        protected virtual void OnChildRemoved(Transform child)
        {
            (child.gameObject as Widget)?.UpdateCascadeColor();
        }

        public Vector3 GetPosition()
        {
            return transform.localPosition;
        }

        public void SetPosition(Vector3 value)
        {
            transform.localPosition = value;
        }

        public Vector3 localPosition
        {
            get => GetPosition();
            set { SetPosition(value); }
        }

        public Quaternion GetRotation()
        {
            return transform.localRotation;
        }

        public void SetRotation(Quaternion value)
        {
            transform.localRotation = value;
        }

        public Quaternion localRotation
        {
            get => GetRotation();
            set { SetRotation(value); }
        }

        public Vector3 GetScale()
        {
            return transform.localScale;
        }

        public void SetScale(Vector3 value)
        {
            transform.localScale = value;
        }

        public Vector3 localScale
        {
            get => GetScale();
            set { SetScale(value); }
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

        public Vector2 size
        {
            get => transform.rect.size;

            set
            {
                transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
                transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.y);
            }
        }

        public IEnumerable<Widget> GetChildren()
        {
            return this.ToList();
        }

        public int GetChildrenCount()
        {
            return transform.childCount;
        }

        public void AddChild(Widget widget)
        {
            widget.transform.parent = this.transform;
        }

        public Widget GetParent()
        {
            return transform.parent?.gameObject as Widget;
        }

        public IEnumerator<Widget> GetEnumerator()
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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public dynamic this[params Widget[] widgets]
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

        private Color _color = Color.white;
        private Color _finalColor = Color.white;

        public Color GetColor()
        {
            return _color;
        }

        public void SetColor(Color value)
        {
            if (_color == value)
            {
                return;
            }
            _color = value;
            UpdateCascadeColor();
        }

        public Color color
        {
            get => GetColor();
            set { SetColor(value); }
        }

        protected virtual void UpdateCascadeColor()
        {
            var parent = GetParent();
            if (parent == null)
            {
                _finalColor = _color;
            }
            else
            {
                _finalColor = (Color)((Vector4)_color * (Vector4)parent._finalColor);
            }
            foreach (var graphic in GetComponents<UI.Graphic>())
            {
                graphic.color = _finalColor;
            }
            foreach (var child in this)
            {
                child.UpdateCascadeColor();
            }
        }

        protected override void AddComponent(Component component)
        {
            base.AddComponent(component);
            var graphic = component as UI.Graphic;
            if (graphic != null)
            {
                graphic.color = _finalColor;
            }
        }
    }
}
