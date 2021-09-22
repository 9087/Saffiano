using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Saffiano.ShaderCompilation
{
    internal enum BlockType
    {
        If = 1,
    }

    internal class Block
    {
        public BlockType blockType { get; private set; }

        public Instruction start { get; private set; }

        public Instruction end { get; private set; }

        public Block(BlockType blockType, Instruction start, Instruction end)
        {
            this.blockType = blockType;
            this.start = start;
            this.end = end;
        }
    }

    internal class CodeBlockStack : Stack<Block>
    {
    }
}
