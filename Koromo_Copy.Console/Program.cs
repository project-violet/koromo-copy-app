// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework;
using Koromo_Copy.Framework.CL;
using System;

namespace Koromo_Copy.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            AppProvider.Initialize();
            Runnable.Start(args);
            AppProvider.Deinitialize();

            Environment.Exit(0);
        }
    }
}
