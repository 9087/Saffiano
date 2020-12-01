using Mono.Cecil;
using System.Collections.Generic;

namespace Saffiano.ShaderCompilation
{
    internal class Value
    {
        public object name { get; private set; }

        public TypeReference type { get; private set; }

        public bool initialized { get; set; } = true;

        public bool isAddress { get; set; } = false;

        public Value(TypeReference type, object name)
        {
            this.type = type;
            this.name = name;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Value))
            {
                return false;
            }
            var value = obj as Value;
            return type == value.type && name == value.name;
        }

        public override string ToString()
        {
            return name.ToString();
        }

        public override int GetHashCode()
        {
            var hashCode = 1725085987;
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + EqualityComparer<TypeReference>.Default.GetHashCode(type);
            return hashCode;
        }
    }

}
