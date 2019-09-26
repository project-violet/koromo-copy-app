// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Text;

namespace Koromo_Copy.Framework.Compiler.CodeGen
{
    public class LPExceptions : Exception
    {
    }

    public class LPOperatorNotFoundException : LPExceptions
    {
        public LPOperator Operator { get; private set; }
        public LPOperatorNotFoundException(LPOperator op)
        {
            Operator = op;
        }
    }
}
