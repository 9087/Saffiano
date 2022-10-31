using System;
using System.Collections.Generic;
using System.Text;
using Saffiano.UI;

namespace Saffiano
{
    public class EventSystem : Behaviour
    {
        internal static EventSystem singleton { get; set; } = null;

        internal static Selectable selectable { get; set; }

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

        internal void ProcessMouseEvent(MouseEvent mouseEvent)
        {
            var old = selectable;
            var current = Traverse(Transform.scene, mouseEvent) as Selectable;
            if (old == current)
            {
                return;
            }
            if (old != null)
            {
                (old as Button)?.OnPointerExit(mouseEvent);
            }
            selectable = current;
            if (selectable != null)
            {
                (selectable as Button)?.OnPointerEnter(mouseEvent);
            }
        }

        private static Button Traverse(Transform transform, MouseEvent mouseEvent)
        {
            Button button;
            foreach (Transform child in transform)
            {
                button = Traverse(child, mouseEvent);
                if (button != null)
                {
                    return button;
                }
            }

            button = transform.GetComponent<Saffiano.UI.Button>();
            if (button == null || !(transform is RectTransform))
            {
                return null;
            }

            var rectTransform = transform as RectTransform;
            var viewportPosition = (mouseEvent.position.xy / Window.GetSize() * 2 - Vector2.one) * new Vector2(1, -1);

            var Pf = new Vector3(viewportPosition.x, viewportPosition.y, -1);
            var Pn = new Vector3(viewportPosition.x, viewportPosition.y, +1);

            Pf = ((Camera.main.worldToCanvasMatrix * transform.localToWorldMatrix).inverse * new Vector4(Pf, 1)).xyz;
            Pn = ((Camera.main.worldToCanvasMatrix * transform.localToWorldMatrix).inverse * new Vector4(Pn, 1)).xyz;

            var E = Pf - Pn;

            var rect = rectTransform.rect;
            Vector3 A = new Vector3(rect.x, rect.y, 0);
            Vector3 B = new Vector3(rect.x + rect.width, rect.y, 0);
            Vector3 C = new Vector3(rect.x, rect.y + rect.height, 0);
            B = B - A;
            C = C - A;
            Vector3 BxC = Vector3.Cross(B, C);
            float BxCdotE = Vector3.Dot(BxC, E);
            if (BxCdotE == 0)
            {
                return null;
            }
            var t = (Vector3.Dot(BxC, A) - Vector3.Dot(BxC, Pn)) / BxCdotE;
            Vector3 P = Pn + t * E;
            if (P.x <= rect.left || P.x >= rect.right || P.y >= rect.top || P.y <= rect.bottom)
            {
                return null;
            }
            switch (mouseEvent.eventType)
            {
                case MouseEventType.MouseDown:
                    button.OnPointerDown(mouseEvent);
                    break;
                case MouseEventType.MouseUp:
                    button.OnPointerUp(mouseEvent);
                    break;
                case MouseEventType.MouseMove:
                    break;
                default:
                    throw new NotImplementedException();
            }
            return button;
        }
    }
}
