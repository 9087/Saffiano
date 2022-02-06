using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.ShaderCompilation
{
    internal class VariableAllocator
    {
        private string prefix = string.Empty;

        private Dictionary<uint, Variable> cache = new Dictionary<uint, Variable>();

        public int Count => cache.Count;

        public VariableAllocator(string prefix)
        {
            this.prefix = prefix;
        }

        public Value Allocate(TypeReference type, uint index)
        {
            type = type.Resolve();
            Value value;
            if (!cache.ContainsKey(index))
            {
                value = new Value(type, string.Format("{0}_{1}", prefix, index));
                value.initialized = false;
                cache[index] = value;
            }
            else
            {
                Debug.Assert(cache[index] is Value);
                value = cache[index] as Value;
            }
            return Allocate(value, index) as Value;
        }

        public Variable Allocate(Variable variable, uint index)
        {
            var type = variable.type.Resolve();
            if (!cache.ContainsKey(index))
            {
                cache[index] = variable;
                cache[index].initialized = false;
                if (variable.name == null)
                {
                    variable.name = string.Format("{0}_{1}", prefix, index);
                }
            }
            var cachedType = cache[index].type;
            if (type.Resolve().GetRuntimeType() == typeof(bool) && cachedType.Resolve().GetRuntimeType() == typeof(int)) { ; }
            else
            {
                Debug.Assert(
                    type.FullName == cachedType.FullName,
                    string.Format("the input type ({0}) does not match the cached ({1})", type.FullName, cache[index].type.FullName)
                );
            }
            return cache[index];
        }

        public Variable Get(uint index)
        {
            return cache[index];
        }

        public Value Allocate(TypeReference type)
        {
            uint index = (uint)this.Count;
            Debug.Assert(!cache.ContainsKey(index));
            return Allocate(type, index);
        }
    }

}
