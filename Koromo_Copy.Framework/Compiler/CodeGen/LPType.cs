// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.Framework.Compiler.CodeGen
{
    public class LPType
    {
        public enum TypeOption
        {
            t_notdetermine,
            t_void,
            t_integer,
            t_bool,
            t_float,
            t_double,
            t_string,
            t_array,
            t_function,
        }
        
        public TypeOption Type { get; set; }
        public bool IsArray { get; set; }
        public LPType ArrayType { get; set; }
        public List<LPType> FunctionArgumentType { get; set; }
    }
}
