using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.ShaderCompilation
{
    internal class VariableAllocator
    {
        private string prefix = string.Empty;

        private Dictionary<uint, Value> cache = new Dictionary<uint, Value>();

        public int Count => cache.Count;

        public VariableAllocator(string prefix)
        {
            this.prefix = prefix;
        }

        public Value Allocate(TypeReference type, uint index)
        {
            type = type.Resolve();
            if (!cache.ContainsKey(index))
            {
                cache[index] = new Value(type, string.Format("{0}_{1}", prefix, index));
                cache[index].initialized = false;
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

        public Value Get(uint index)
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
