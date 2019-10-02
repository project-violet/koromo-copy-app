// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace Koromo_Copy.Framework.Postprocessor
{
    /// <summary>
    /// Postprocessor interface
    /// </summary>
    public abstract class IPostprocessor
    {
        public abstract void Run(NetTask task);
    }
}
