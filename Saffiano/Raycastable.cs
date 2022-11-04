using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano
{
    public class Raycastable : Behaviour
    {
        internal bool Raycast(Vector3 pf, Vector3 pn, RectTransform rectTransform)
        {
            var Pf = (rectTransform.worldToLocalMatrix * new Vector4(pf, 1)).xyz;
            var Pn = (rectTransform.worldToLocalMatrix * new Vector4(pn, 1)).xyz;

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
                return false;
            }
            var t = (Vector3.Dot(BxC, A) - Vector3.Dot(BxC, Pn)) / BxCdotE;
            Vector3 P = Pn + t * E;
            if (P.x <= rect.left || P.x >= rect.right || P.y >= rect.top || P.y <= rect.bottom)
            {
                return false;
            }
            return true;
        }
    }
}
