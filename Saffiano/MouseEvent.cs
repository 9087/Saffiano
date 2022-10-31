using System;

namespace Saffiano
{
    internal enum MouseEventType
    {
        MouseDown = 0,
        MouseUp = 1,
        MouseMove = 2,
    }

    internal class MouseEvent : InputEvent
    {
        public MouseEventType eventType;
        public KeyCode keyCode;
        public Vector3 position;

        public MouseEvent(MouseEventType eventType, KeyCode keyCode, Vector3 position)
        {
            this.eventType = eventType;
            this.keyCode = keyCode;
            this.position = position;
        }

        public MouseEvent(MouseEventType eventType, Vector3 position)
        {
            this.eventType = eventType;
            this.keyCode = KeyCode.None;
            this.position = position;
        }

        public override String ToString()
        {
            return String.Format("(eventType: {0}, keyCode: {1}, position: {2})", this.eventType, this.keyCode, this.position);
        }
    }
}
