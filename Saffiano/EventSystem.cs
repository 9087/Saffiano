using System;
using System.Collections.Generic;
using System.Text;
using Saffiano.EventSystems;
using Saffiano.UI;

namespace Saffiano
{
    public class EventSystem : Behaviour
    {
        internal static EventSystem singleton { get; set; } = null;

        internal static Selectable selectable { get; set; }

        internal static HashSet<Raycastable> downs = new HashSet<Raycastable>();

        void Awake()
        {
            Debug.Assert(singleton == null);
            singleton = this;
        }

        void OnDestroy()
        {
            Debug.Assert(singleton != null);
            singleton = null;
        }

        private static PointerEventData PopulatePointerEventData(MouseEvent mouseEvent)
        {
            PointerEventData.InputButton? button = null;
            switch (mouseEvent.keyCode)
            {
                case KeyCode.Mouse0:
                    button = PointerEventData.InputButton.Left;
                    break;
                case KeyCode.Mouse1:
                    button = PointerEventData.InputButton.Right;
                    break;
                case KeyCode.Mouse2:
                    button = PointerEventData.InputButton.Middle;
                    break;
            }
            return new PointerEventData() { _button = button };
        }

        internal void ProcessMouseEvent(MouseEvent mouseEvent)
        {
            var old = selectable;
            var current = Traverse(Transform.scene, mouseEvent) as Selectable;
            if (mouseEvent.eventType == MouseEventType.MouseUp)
            {
                downs.Clear();
            }
            if (old == current)
            {
                return;
            }
            if (old != null)
            {
                (old as IPointerExitHandler)?.OnPointerExit(PopulatePointerEventData(mouseEvent));
            }
            selectable = current;
            if (selectable != null)
            {
                (selectable as IPointerEnterHandler)?.OnPointerEnter(PopulatePointerEventData(mouseEvent));
            }
        }

        private static Raycastable Traverse(Transform transform, MouseEvent mouseEvent)
        {
            Raycastable raycastable;
            foreach (Transform child in transform)
            {
                raycastable = Traverse(child, mouseEvent);
                if (raycastable != null)
                {
                    return raycastable;
                }
            }
            raycastable = transform.GetComponent<Raycastable>();
            if (raycastable == null || !(transform is RectTransform))
            {
                return null;
            }

            var rectTransform = transform as RectTransform;
            var viewportPosition = (mouseEvent.position.xy / Window.GetSize() * 2 - Vector2.one) * new Vector2(1, -1);

            var pf = new Vector3(viewportPosition.x, viewportPosition.y, -1);
            var pn = new Vector3(viewportPosition.x, viewportPosition.y, +1);

            pf = ((Camera.main.worldToCanvasMatrix).inverse * new Vector4(pf, 1)).xyz;
            pn = ((Camera.main.worldToCanvasMatrix).inverse * new Vector4(pn, 1)).xyz;

            if (!raycastable.Raycast(pf, pn, rectTransform))
            {
                return null;
            }

            switch (mouseEvent.eventType)
            {
                case MouseEventType.MouseDown:
                    (raycastable as IPointerDownHandler)?.OnPointerDown(PopulatePointerEventData(mouseEvent));
                    downs.Add(raycastable);
                    break;
                case MouseEventType.MouseUp:
                    (raycastable as IPointerUpHandler)?.OnPointerUp(PopulatePointerEventData(mouseEvent));
                    if (downs.Contains(raycastable))
                    {
                        (raycastable as IPointerClickHandler)?.OnPointerClick(PopulatePointerEventData(mouseEvent));
                    }
                    break;
                case MouseEventType.MouseMove:
                    break;
                default:
                    throw new NotImplementedException();
            }
            return raycastable;
        }
    }
}
