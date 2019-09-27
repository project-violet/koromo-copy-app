// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Cache;
using Koromo_Copy.Framework.Log;
using Koromo_Copy.Framework.Network;
using Koromo_Copy.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;

namespace Koromo_Copy.Framework
{
    /// <summary>
    /// Koromo Copy App-Provider
    /// </summary>
    public class AppProvider
    {
        public static Dictionary<string, object> Instance =>
            InstanceMonitor.Instances;

        public static NetScheduler Scheduler { get; set; }

        public static bool Initialize()
        {
            // Initialize logs instance
            Logs.Instance.Push("App provider initializing...");

            // If locking fails, then cannot use koromo-copy.
            if (!ProgramLock.Lock())
                return false;

            // Check program crashed.
            if (ProgramLock.ProgramCrashed)
                ;

            // Check exists instances.
            if (Instance.Count > 1)
                throw new Exception("You must wait for app-provider initialization procedure before using instance-lazy!\n" +
                    "For more informations, see the development documents.");

            // GC Setting
            GCLatencyMode oldMode = GCSettings.LatencyMode;
            RuntimeHelpers.PrepareConstrainedRegions();
            GCSettings.LatencyMode = GCLatencyMode.Batch;

            Scheduler = new NetScheduler(0);


            Logs.Instance.Push("App provider starts.");

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            return true;
        }

        public static void Deinitialize()
        {
            Logs.Instance.Push("App provider de-initialized.");

            ProgramLock.UnLock();
        }
    }
}
