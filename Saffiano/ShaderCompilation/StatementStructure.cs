using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Saffiano.ShaderCompilation
{
    internal enum StatementStructureType
    {
        Unknown = 0,
        Condition = 1,
        Loop = 2,
    }


    internal abstract class StatementStructure
    {
        // Branch instructions (ECMA-335, VI.C.4.7)
        // -------------------------------------------------------------
        //                     | beq    | beq.s    | bge    | bge.s    |
        // bge.un  | bge.un.s  | bgt    | bgt.s    | bgt.un | bgt.un.s |
        // ble     | ble.s     | ble.un | ble.un.s | blt    | blt.s    |
        // blt.un  | blt.un.s  | bne.un | bne.un.s | br     | br.s     |
        // brfalse | brfalse.s | brtrue | brtrue.s | leave  | leave.s

        protected static HashSet<OpCode> ConditionalBranch_OpCodes = new HashSet<OpCode>
        {
            OpCodes.Brtrue, OpCodes.Brtrue_S,
            OpCodes.Brfalse, OpCodes.Brfalse_S,
            OpCodes.Beq, OpCodes.Beq_S,
            OpCodes.Bge, OpCodes.Bge_S, OpCodes.Bge_Un, OpCodes.Bge_Un_S,
            OpCodes.Bgt, OpCodes.Bgt_S, OpCodes.Bgt_Un, OpCodes.Bgt_Un_S,
            OpCodes.Ble, OpCodes.Ble_S, OpCodes.Ble_Un, OpCodes.Ble_Un_S,
            OpCodes.Blt, OpCodes.Blt_S, OpCodes.Blt_Un, OpCodes.Blt_Un_S,
            OpCodes.Bne_Un, OpCodes.Bne_Un_S,
        };

        public StatementStructureType type { get; private set; }

        public CodeBlock all { get; private set; }

        protected StatementStructure(StatementStructureType type, CodeBlock all)
        {
            this.type = type;
            this.all = all;
        }

        public override string ToString()
        {
            return string.Format("{0}(type({1}))", this.GetType().Name, type);
        }

        static public StatementStructureType Recognize(CodeBlock block, out StatementStructure statementStructure)
        {
            if (LoopStatementStructure.Detect(block, out statementStructure) ||
                ConditionStatementStructure.Detect(block, out statementStructure))
            {
                return statementStructure.type;
            }
            statementStructure = null;
            return StatementStructureType.Unknown;
        }

        public abstract string Generate(CompileContext outer);
    }

    internal class LoopStatementStructure : StatementStructure
    {
        // for(init, condition, step) { body }

        public CodeBlock init { get; private set; }

        public CodeBlock condition { get; private set; }

        public CodeBlock step { get; private set; }

        public CodeBlock body { get; private set; }

        public LoopStatementStructure(CodeBlock all, CodeBlock init, CodeBlock condition, CodeBlock step, CodeBlock body) : base(StatementStructureType.Loop, all)
        {
            this.init = init;
            this.condition = condition;
            this.step = step;
            this.body = body;
        }

        static public bool Detect(CodeBlock block, out StatementStructure statementStructure)
        {
            statementStructure = null;
            var opCode = block.first.OpCode;
            if (opCode != OpCodes.Br)
            {
                return false;
            }

            // maybe loop condition
            var target = block.first.Operand as Instruction;

            // find next branch instruction (Brtrue, Brtrue_S), which may be a loop tag and an end
            Instruction last = null;
            {
                var tmp = target;
                while (tmp != null && !ConditionalBranch_OpCodes.Contains(tmp.OpCode))
                {
                    tmp = tmp.Next;
                }
                if (tmp == null)
                {
                    return false;
                }
                last = tmp;
            }

            // loop body
            if ((last.Operand as Instruction).Previous != block.first)
            {
                return false;
            }

            CodeBlock all = new CodeBlock(block.first, last);
            CodeBlock init = null;
            CodeBlock condition = new CodeBlock(target, last);
            CodeBlock step = null;
            CodeBlock body = new CodeBlock(block.first.Next, target.Previous);
            statementStructure = new LoopStatementStructure(all, init, condition, step, body);
            return true;
        }
    
        public override string Generate(CompileContext outer)
        {
            // Condition
            EvaluationStack unhandled = null;
            string nothing = outer.GenerateWithoutWriting(this.condition.first, this.condition.last, ref unhandled);
            Debug.Assert(string.IsNullOrEmpty(nothing));
            if (unhandled == null)
            {
                unhandled = outer.GetEvaluationStack();
            }
            var condition = unhandled.Pop();
            string body = outer.GenerateWithoutWriting(this.body.first, this.body.last, ref unhandled);
            Debug.Assert(unhandled == null);
            return CompileContext.Format("while({0} != 0) {{\n{1}\n}}\n", condition, body);
        }
    }

    internal class ConditionStatementBlock
    {
        public CodeBlock condition { get; private set; }

        public CodeBlock body { get; private set; }

        public ConditionStatementBlock(CodeBlock condition, CodeBlock body)
        {
            this.condition = condition;
            this.body = body;
        }

        public override string ToString()
        {
            return string.Format("condition:{0},body:{1}", this.condition, this.body);
        }
    }

    internal class ConditionStatementStructure : StatementStructure
    {
        public List<ConditionStatementBlock> blocks { get; protected set; }

        protected ConditionStatementStructure(CodeBlock all, List<ConditionStatementBlock> blocks) : base(StatementStructureType.Condition, all)
        {
            this.blocks = blocks;
        }

        static public bool Detect(CodeBlock block, out StatementStructure statementStructure)
        {
            List<OpCode> brs = new List<OpCode> { OpCodes.Br_S, OpCodes.Br };
            var lastOffset = block.last.Offset;

            statementStructure = null;
            var opCode = block.first.OpCode;
            if (!ConditionalBranch_OpCodes.Contains(opCode))
            {
                return false;
            }
            List<ConditionStatementBlock> blocks = new List<ConditionStatementBlock>();
            var current = block.first;
            Instruction next = current.Operand as Instruction;

            if (lastOffset < current.Offset ||
                lastOffset < current.Next.Offset ||
                lastOffset < next.Previous.Offset)
            {
                return false;
            }

            blocks.Add(
                new ConditionStatementBlock
                (
                    new CodeBlock(current, current),
                    new CodeBlock(current.Next, next.Previous)
                )
            );

            // there is an else statement
            bool found = true;
            while (brs.Contains((next = (current.Operand as Instruction)).Previous.OpCode))
            {
                var br = next.Previous;
                found = false;
                for (var i = next; i != br.Operand as Instruction; i = i.Next)
                {
                    if (ConditionalBranch_OpCodes.Contains(i.OpCode))
                    {
                        current = i;
                        var condition = new CodeBlock(next, current);
                        next = current.Operand as Instruction;
                        var body = new CodeBlock(current.Next, next.Previous);
                        blocks.Add(
                            new ConditionStatementBlock(condition, body)
                        );
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    blocks.Add(
                        new ConditionStatementBlock
                        (
                            null,
                            new CodeBlock(next, (br.Operand as Instruction).Previous)
                        )
                    );
                    break;
                }
            }
            CodeBlock all = new CodeBlock(block.first, blocks[blocks.Count - 1].body.last);
            statementStructure = new ConditionStatementStructure(all, blocks);
            return true;
        }

        public override string Generate(CompileContext outer)
        {
            using (TextWriter writer = new StringWriter())
            {
                bool first = true;
                foreach (var block in blocks)
                {
                    EvaluationStack unhandled = null;
                    if (block.condition != null)
                    {
                        string nothing = outer.GenerateWithoutWriting(block.condition.first, block.condition.last, ref unhandled);
                        Debug.Assert(string.IsNullOrEmpty(nothing));
                        if (unhandled == null)
                        {
                            unhandled = outer.GetEvaluationStack();
                        }
                        writer.WriteLine(CompileContext.Format(first ? "if({0} == 0)" : "else if({0} == 0)", unhandled.Pop()));
                    }
                    else
                    {
                        writer.WriteLine("else");
                    }
                    writer.WriteLine(CompileContext.Format("{{{0}}}", outer.GenerateWithoutWriting(block.body.first, block.body.last, ref unhandled)));
                    first = false;
                }
                return writer.ToString();
            }
        }
    }
}
