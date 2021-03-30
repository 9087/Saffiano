using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano
{
    internal enum CharEventType : uint
    {
        Char = 1,
        DeadChar = 2,
        Unicode = 3,
    }

    internal class CharEvent
    {
        public CharEventType eventType { get; private set; }

        public char @char { get; private set; }

        public CharEvent(CharEventType eventType, char @char)
        {
            this.eventType = eventType;
            this.@char = @char;
        }
    }
}
