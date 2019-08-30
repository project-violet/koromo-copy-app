// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Text;

namespace Koromo_Copy.Framework.DataBase.ClassTree
{
    public class ClassTreeModel
    {
        public List<string> root_classes;

        // parent class, class name
        public List<Tuple<string, string>> sub_classes;
    }
}
