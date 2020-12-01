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
            if (!cache.ContainsKey(index))
            {
                cache[index] = new Value(type, string.Format("{0}_{1}", prefix, index));
                cache[index].initialized = false;
            }
            Debug.Assert(type == cache[index].type);
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
