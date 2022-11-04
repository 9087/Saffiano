using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.EventSystems
{
    public interface IPointerEnterHandler
    {
        void OnPointerEnter(PointerEventData eventData);
    }
}
