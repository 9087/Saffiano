using System.Collections.Generic;
using Mono.Cecil;

namespace Saffiano.ShaderCompilation
{
    internal class EvaluationStack : Stack<Variable>
    {
        public Variable Push(TypeReference type, object name)
        {
            var value = new Value(type, name);
            Push(value);
            return value;
        }

        public Variable Push(ParameterReference parameterReference)
        {
            var value = new Value(parameterReference.ParameterType, parameterReference.Name);
            Push(value);
            return value;
        }

        public Variable Push(PropertyReference propertyReference)
        {
            var value = new Value(propertyReference.PropertyType, propertyReference.Name);
            Push(value);
            return value;
        }

        public List<Variable> Pop(int count)
        {
            List<Variable> list = new List<Variable>();
            for (int i = 0; i < count; i++)
            {
                list.Insert(0, Pop());
            }
            return list;
        }
    }
}
