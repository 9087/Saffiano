using Saffiano.EventSystems;
using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.UI
{
    public class Button : Selectable, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("PointerDown!");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log("PointerUp!");
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("PointerEnter!");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("PointerExit!");
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("PointerClick!");
        }
    }
}
