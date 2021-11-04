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


    internal class StatementStructure
    {
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

        static public StatementStructureType Recognize(Instruction instruction, out StatementStructure statementStructure)
        {
            if (LoopStatementStructure.Detect(instruction, out statementStructure))
            {
                return statementStructure.type;
            }
            statementStructure = null;
            return StatementStructureType.Unknown;
        }

        public virtual string Generate(CompileContext outer)
        {
            throw new NotImplementedException();
        }
    }

    internal class LoopStatementStructure : StatementStructure
    {
        // Branch instructions (ECMA-335, VI.C.4.7)
        // -------------------------------------------------------------
        //                     | beq    | beq.s    | bge    | bge.s    |
        // bge.un  | bge.un.s  | bgt    | bgt.s    | bgt.un | bgt.un.s |
        // ble     | ble.s     | ble.un | ble.un.s | blt    | blt.s    |
        // blt.un  | blt.un.s  | bne.un | bne.un.s | br     | br.s     |
        // brfalse | brfalse.s | brtrue | brtrue.s | leave  | leave.s

        static HashSet<OpCode> ConditionalBranch_OpCodes = new HashSet<OpCode>
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

        static public bool Detect(Instruction instruction, out StatementStructure statementStructure)
        {
            statementStructure = null;
            var opCode = instruction.OpCode;
            if (opCode != OpCodes.Br)
            {
                return false;
            }

            // maybe loop condition
            var target = instruction.Operand as Instruction;

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
            if ((last.Operand as Instruction).Previous != instruction)
            {
                return false;
            }

            CodeBlock all = new CodeBlock(instruction, last);
            CodeBlock init = null;
            CodeBlock condition = new CodeBlock(target, last);
            CodeBlock step = null;
            CodeBlock body = new CodeBlock(instruction.Next, target.Previous);
            statementStructure = new LoopStatementStructure(all, init, condition, step, body);
            return true;
        }
    
        public override string Generate(CompileContext outer)
        {
            // Condition
            EvaluationStack unhandled = null;
            string nothing = outer.GenerateWithoutWriting(this.condition.first, this.condition.last.Previous, ref unhandled);
            Debug.Assert(unhandled != null && unhandled.Count == 1 && string.IsNullOrEmpty(nothing));
            string condition;
            var last = this.condition.last;
            if (last.OpCode == OpCodes.Brtrue)
            {
                condition = CompileContext.Format("int({0}) != 0", unhandled.Pop());
            }
            else if (last.OpCode == OpCodes.Ble)
            {
                var b = unhandled.Pop();
                var a = unhandled.Pop();
                condition = CompileContext.Format("int({0} <= {1}) != 0", a, b);
            }
            else
            {
                throw new NotImplementedException(string.Format("{0} operation code is not implemented!", last.OpCode));
            }

            string body = outer.GenerateWithoutWriting(this.body.first, this.body.last, ref unhandled);
            Debug.Assert(unhandled == null);
            return string.Format("while({0}) {{\n{1}\n}}\n", condition, body);
        }
    }
}
