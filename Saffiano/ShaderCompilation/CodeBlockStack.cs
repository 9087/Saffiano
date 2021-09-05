using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.ShaderCompilation
{
    internal class Block
    {
        public Instruction start { get; private set; }

        public Instruction end { get; private set; }

        public Block(Instruction start, Instruction end)
        {
            this.start = start;
            this.end = end;
        }
    }

    internal class CodeBlockStack : Stack<Block>
    {
    }
}
