using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.EventSystems
{
    public interface IPointerExitHandler
    {
        void OnPointerExit(PointerEventData eventData);
    }
}
