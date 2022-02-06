using Mono.Cecil;
using System;

namespace Saffiano.ShaderCompilation
{
    internal class Value : Variable
    {
        public Value(TypeReference type, object name) : base(type, name)
        {
            Debug.Assert(type.IsArray == false);
        }

        public static implicit operator int(Value value)
        {
            if (value.name is int)
            {
                return (int)value.name;
            }
            else if (value.name is string)
            {
                return int.Parse(value.name as string);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
