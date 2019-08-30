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
        : LPDefine
    {
        List<LPOperator> insts;

        public LPBasicBlock()
        {
            insts = new List<LPOperator>();
        }

        public List<LPOperator> Childs { get { return insts; } }

        public void Insert(LPOperator op)
        {
            insts.Add(op);
        }
    }
}
