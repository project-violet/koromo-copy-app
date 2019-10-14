// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Koromo_Copy.Framework.Network;

namespace Koromo_Copy.Framework.Postprocessor
{
    public class M3u8Postprocessor : IPostprocessor
    {
        public string FolderName;
        public int Wait;

        public override void Run(NetTask task)
        {
            if (Interlocked.Decrement(ref Wait) != 0)
                return;
        }
    }
}
