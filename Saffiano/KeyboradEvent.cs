using System;

namespace Saffiano
{
    internal enum KeyboardEventType : uint
    {
        KeyDown = 1,
        KeyUp = 2,
    }

    internal class KeyboardEvent : InputEvent
    {
        public KeyboardEventType eventType { get; private set; }

        public KeyCode keyCode { get; private set; }

        public KeyboardEvent(KeyboardEventType eventType, KeyCode keyCode)
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
