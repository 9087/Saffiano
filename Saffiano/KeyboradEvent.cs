using System;

namespace Saffiano
{
    public enum KeyboardEventType : uint
    {
        KeyDown = 1,
        KeyUp = 2,
        Char = 3,
        DeadChar = 4,
    }

    class KeyboardEvent
    {
        public KeyboardEventType eventType;
        public KeyCode keyCode;

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
