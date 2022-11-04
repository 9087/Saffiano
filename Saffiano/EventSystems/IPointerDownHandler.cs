using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.EventSystems
{
    public interface IPointerDownHandler
    {
        void OnPointerDown(PointerEventData eventData);
    }
}
