using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.EventSystems
{
    public interface IPointerClickHandler
    {
        void OnPointerClick(PointerEventData eventData);
    }
}
