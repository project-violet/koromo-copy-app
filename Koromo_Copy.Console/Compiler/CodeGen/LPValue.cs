// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Framework.Compiler.CodeGen
{
    public interface LPValue
    {
        string ShortString { get; }
    }

    public abstract class LPUser
        : LPValue
    {
        public LPDebugInfo DebugInfo { get; set; }

        public string ShortString => throw new NotImplementedException();

        public LPType Type { get; set; }
    }
}
