using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.EventSystems
{
    public class PointerEventData
    {
        public enum InputButton
        {
            Left,
            Right,
            Middle,
        }

        internal InputButton? _button = null;

        public InputButton button
        {
            get
            {
                if (_button == null)
                {
                    throw new Exception();
                }
                return _button.Value;
            }
        }

        public PointerEventData()
        {

        }
    }
}
