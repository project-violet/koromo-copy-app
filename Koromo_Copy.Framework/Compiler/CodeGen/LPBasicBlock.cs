// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Framework.Compiler.CodeGen
{
    public class LPBasicBlock
        : LPValue
    {
        public LPBasicBlock()
        {
            Childs = new List<LPOperator>();
        }

        public List<LPOperator> Childs { get; }
        public LPFunction Function { get; set; }
        public LPModule Module { get; set; }

        public string ShortString => throw new NotImplementedException();

        public void Insert(LPOperator op)
        {
            op.Parent = this;
            op.Function = Function;
            op.Module = Module;
            Childs.Add(op);
        }

        public int Position(LPOperator op)
            => Childs.IndexOf(op);
    }
}
