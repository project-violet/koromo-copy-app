// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Framework.Compiler.CodeGen
{
    public class LPArgument
        : LPValue
    {
        public LPType Type { get; set; }

        public string ShortString => throw new NotImplementedException();
    }

    public class LPFunction
    {
        string name;

        public LPFunction(LPModule module, string name, LPType return_type, List<LPArgument> args)
        {
            Childs = new List<LPBasicBlock>();
            Arguments = args;
            this.name = name;
            this.ReturnType = return_type;
            this.Module = module;
        }

        public List<LPBasicBlock> Childs { get; }

        /// <summary>
        /// Check this function is extern function.
        /// If this function is an extern, the body of the function is not defined in module.
        /// </summary>
        public bool IsExtern { get; set; }

        /// <summary>
        /// If the function's name does not exist, it is treated as a lambda function.
        /// </summary>
        public bool IsAnonymous { get { return name == ""; } }
        public string Name { get; set; }
        public LPModule Module { get; private set; }
        public LPBasicBlock Entry { get { return Childs[0]; } }
        public LPType ReturnType { get; private set; }
        public List<LPArgument> Arguments { get; private set; }

        /// <summary>
        /// Create new basicblock
        /// </summary>
        /// <returns></returns>
        public LPBasicBlock CreateBasicBlock()
        {
            var block = new LPBasicBlock();
            block.Module = Module;
            block.Function = this;
            Childs.Add(block);
            return block;
        }
    }
}
