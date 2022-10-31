using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.UI
{
    public class Button : Selectable
    {
        internal void OnPointerDown(MouseEvent args)
        {
            Debug.Log("PointerDown!");
        }

        internal void OnPointerUp(MouseEvent args)
        {
            Debug.Log("PointerUp!");
        }

        internal void OnPointerEnter(MouseEvent args)
        {
            Debug.Log("PointerEnter!");
        }

        internal void OnPointerExit(MouseEvent args)
        {
            Debug.Log("PointerExit!");
        }
    }
}
