using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.EventSystems
{
    public interface IPointerUpHandler
    {
        void OnPointerUp(PointerEventData eventData);
    }
}
