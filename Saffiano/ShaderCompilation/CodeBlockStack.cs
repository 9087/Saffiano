using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.ShaderCompilation
{
    internal enum CodeBlockType
    {
        Unknown = 0,
    }

    internal class CodeBlock
    {
        public CodeBlockType type { get; private set; }

        public Instruction first { get; private set; }

        public Instruction last { get; private set; }

        public CodeBlock(Instruction first, Instruction last, CodeBlockType type = CodeBlockType.Unknown)
        {
            this.first = first;
            this.last = last;
            this.type = type;
        }

        public override string ToString()
        {
            return string.Format("[{0:x4},{1:x4}]", this.first.Offset, this.last.Offset);
        }
    }

    internal class CodeBlockStack : Stack<CodeBlock>
    {
    }
}
