// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Framework.Compiler.CodeGen
{
    public abstract class LPOperator
        : LPUser
    {
        protected List<LPUser> operand;
        protected int opcode;
        
        public LPBasicBlock Parent { get; set; }
        public LPFunction Function { get; set; }
        public LPModule Module { get; set; }

        public void RemoveFromParent()
        {
            var pos = Parent.Position(this);

            Parent.Childs.RemoveAt(pos);
        }

        public void InsertBefore(LPOperator op)
        {
            op.Parent = Parent;
            op.Function = Function;
            op.Module = Module;

            var pos = Parent.Position(this);
            Parent.Childs.Insert(pos, op);
        }

        public void InsertAfter(LPOperator op)
        {
            op.Parent = Parent;
            op.Function = Function;
            op.Module = Module;

            var pos = Parent.Position(this);
            Parent.Childs.Insert(pos + 1, op);
        }

        /// <summary>
        /// Unlink from current basic-bloc and insert before this operator.
        /// </summary>
        /// <param name="op"></param>
        public void MoveBefore(LPOperator op)
        {
            var pos = op.Parent.Position(op);

            if (pos < 0)
                throw new LPOperatorNotFoundException(op);

            RemoveFromParent();
            op.InsertBefore(this);
        }

        /// <summary>
        /// Unlink from current basic-bloc and insert after this operator.
        /// </summary>
        /// <param name="op"></param>
        public void MoveAfter(LPOperator op)
        {
            var pos = op.Parent.Position(op);

            if (pos < 0)
                throw new LPOperatorNotFoundException(op);

            RemoveFromParent();
            op.InsertAfter(this);
        }

        public LPUser GetOperand(int index) => operand[index];

        public abstract override string ToString();
    }

    public class LPUnaryOperator
        : LPOperator
    {
        public enum UnaryOption
        {
            inc,   // ++
            dec,   // --
            index, // [~]
            not,   // ~
        }

        public UnaryOption Option { get; set; }
        public static LPUnaryOperator Create(UnaryOption option, LPUser operand1)
        {
            var lpuo = new LPUnaryOperator
            {
                Option = option,
                Type = operand1.Type
            };
            lpuo.operand.Add(operand1);
            return lpuo;
        }

        public override string ToString()
            => $"{ShortString} = {Option} {Type} {operand[0]}";
    }

    public class LPBinaryOperator
        : LPOperator
    {
        public enum BinaryOption
        {
            plus,
            minus,
            multiple,
            divide,
            modular,
            and,
            or,
            xor,
        }

        public BinaryOption Option { get; set; }
        public static LPBinaryOperator Create(BinaryOption option, LPUser operand1, LPUser operand2)
        {
            var lpbo = new LPBinaryOperator
            {
                Option = option,
                Type = operand1.Type,
            };
            lpbo.operand.Add(operand1);
            lpbo.operand.Add(operand2);
            return lpbo;
        }

        public override string ToString()
            => $"{ShortString} = {Option.ToString()} {operand[0].ShortString}, {operand[1].ShortString}";
    }

    public abstract class LPCompareOperator
        : LPOperator
    {
        public enum CompareOption
        {
            not,  // !
            zr,   // == 0
            nz,   // != 0

            eq,   // ==
            neq,  // !=
            less, // <
            leq,  // <=
            gret, // >
            geq,  // >=
        }

        public CompareOption Option { get; set; }
    }

    public class LPUnaryCompareOperator
        : LPCompareOperator
    {
        public LPUser Operand { get { return operand[0]; } set { operand[0] = value; } }

        public static LPUnaryCompareOperator Create(CompareOption option, LPUser operand1)
        {
            var lpuco = new LPUnaryCompareOperator
            {
                Option = option,
                Type = operand1.Type
            };
            lpuco.operand.Add(operand1);
            return lpuco;
        }

        public override string ToString()
            => $"{ShortString} = cmp {Type} {Option.ToString()} {Operand.ShortString}";
    }

    public class LPBinaryCompareOperator
        : LPCompareOperator
    {
        public static LPBinaryCompareOperator Create(CompareOption option, LPUser operand1, LPUser operand2)
        {
            var lpbco = new LPBinaryCompareOperator
            {
                Option = option,
                Type = operand1.Type
            };
            lpbco.operand.Add(operand1);
            lpbco.operand.Add(operand2);
            return lpbco;
        }

        public override string ToString()
            => $"{ShortString} = cmp {Type} {Option.ToString()} {operand[0].ShortString}, {operand[1].ShortString}";
    }

    public class LPBranchOperator
        : LPOperator
    {
        public LPCompareOperator Comparator { get; set; }

        public LPBasicBlock TrueBlock { get; set; }
        public LPBasicBlock FalseBlock { get; set; }

        public bool IsJump { get; set; }

        public static LPBranchOperator Create(LPCompareOperator comp, LPBasicBlock true_block, LPBasicBlock false_block)
            => new LPBranchOperator { Type = comp.Type, Comparator = comp, TrueBlock = true_block, FalseBlock = false_block };
        public static LPBranchOperator Create(LPBasicBlock jump_block)
            => new LPBranchOperator { TrueBlock = jump_block, IsJump = true };

        public override string ToString()
            => $"br {(IsJump ? TrueBlock.ShortString : $"{Type} {Comparator.ShortString}, {TrueBlock.ShortString}, {FalseBlock.ShortString}")}";
    }

    public class LPCallOperator
        : LPOperator
    {
        public LPFunction Caller { get; set; }
        public List<LPUser> Arguments { get; set; }

        public static LPCallOperator Create(LPFunction function, List<LPUser> args)
            => new LPCallOperator { Type = function.ReturnType, Caller = function, Arguments = args };

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }

    public class LPAllocOperator
        : LPOperator
    {
        public static LPAllocOperator Create(LPType type)
            => new LPAllocOperator { Type = type };

        public override string ToString()
            => $"{ShortString} = alloca {Type}";
    }

    public class LPStoreOperator
        : LPOperator
    {
        public LPUser Value { get; set; }
        public LPUser Pointer { get; set; }

        public static LPStoreOperator Create(LPUser value, LPUser pointer)
            => new LPStoreOperator { Value = value, Pointer = pointer };

        public override string ToString()
            => $"store {Value.Type} {Value.ShortString}, {Pointer.Type} {Pointer.ShortString}";
    }

    public class LPLoadOperator
        : LPOperator
    {
        public LPUser Value { get; set; }

        public static LPStoreOperator Create(LPUser value)
            => new LPStoreOperator { Type = value.Type, Value = value };

        public override string ToString()
            => $"{ShortString} = load {Type} {Value}";
    }
}
