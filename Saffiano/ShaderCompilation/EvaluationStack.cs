using System.Collections.Generic;
using Mono.Cecil;

namespace Saffiano.ShaderCompilation
{
    internal class EvaluationStack : Stack<Value>
    {
        public Value Push(TypeReference type, object name)
        {
            var value = new Value(type, name);
            Push(value);
            return value;
        }

        public Value Push(ParameterReference parameterReference)
        {
            var value = new Value(parameterReference.ParameterType, parameterReference.Name);
            Push(value);
            return value;
        }

        public Value Push(PropertyReference propertyReference)
        {
            var value = new Value(propertyReference.PropertyType, propertyReference.Name);
            Push(value);
            return value;
        }

        public List<Value> Pop(int count)
        {
            List<Value> list = new List<Value>();
            for (int i = 0; i < count; i++)
            {
                list.Insert(0, Pop());
            }
            return list;
        }
    }
}
