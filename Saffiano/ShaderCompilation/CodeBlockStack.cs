using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.ShaderCompilation
{
    internal enum CodeBlockType
    {
        Unknown = 0,
        If = 1,
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
    }

    internal class CodeBlockStack : Stack<CodeBlock>
    {
    }
}
