using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;

namespace Saffiano.ShaderCompilation
{
    internal class Array : Variable, IEnumerable<Variable>
    {
        public override bool isArray => true;

        public Value elementCount { get; private set; }

        private Variable[] elements;

        public Array(TypeReference type, object name, Value elementCount) : base(type, name)
        {
            this.elementCount = elementCount;
            elements = new Variable[(int)elementCount];
        }

        public void SetElement(int index, Variable element)
        {
            elements[index] = element;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var child in this.elements)
            {
                yield return child;
            }
        }

        IEnumerator<Variable> IEnumerable<Variable>.GetEnumerator()
        {
            foreach (var child in this.elements)
            {
                yield return child;
            }
        }
    }
}
