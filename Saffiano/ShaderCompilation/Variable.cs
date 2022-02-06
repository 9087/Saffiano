using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.ShaderCompilation
{
    internal class Variable
    {
        public TypeReference type { get; private set; }

        public object name { get; internal set; }

        public bool initialized { get; set; } = true;

        public bool isAddress { get; set; } = false;

        public virtual bool isArray => false;

        public Variable(TypeReference type, object name)
        {
            this.type = type;
            this.name = name;
        }

        public override string ToString()
        {
            return name.ToString();
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

        public override int GetHashCode()
        {
            int hashCode = -1890651077;
            hashCode = hashCode * -1521134295 + EqualityComparer<TypeReference>.Default.GetHashCode(type);
            hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(name);
            return hashCode;
        }
    }
}
