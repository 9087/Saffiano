using System;

namespace Saffiano
{
    internal enum MouseEventType
    {
        MouseDown = 0,
        MouseUp = 1,
    }

    internal class MouseEvent : InputEvent
    {
        public MouseEventType eventType;
        public KeyCode keyCode;

        public MouseEvent(MouseEventType eventType, KeyCode keyCode)
        {
            this.eventType = eventType;
            this.keyCode = keyCode;
        }

        public override String ToString()
        {
            return String.Format("(eventType: {0}, keyCode: {1})", this.eventType, this.keyCode);
        }
    }
}
