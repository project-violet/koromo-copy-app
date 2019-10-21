// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Text;

namespace Koromo_Copy.Framework.Network
{
    public enum NetDownloaderSchedulingType
    {
        DownloadCountBase,
        DownloadBytesBase
    }

    public class NetTaskGroup
    {
        
    }

    /// <summary>
    /// Congestion Control Tool of Non-Preemptive Network Scheduler
    /// 
    /// Network Task Roadmap
    /// NetTask -> NetTaskGroup -> NetDownloader -> NetScheduler -> NetField
    /// </summary>
    public class NetDownloader
    {
        public NetScheduler Scheduler { get; private set; }
        public NetDownloaderSchedulingType SchedulerType { get; private set; }

        public NetDownloader(NetScheduler sched, NetDownloaderSchedulingType type = NetDownloaderSchedulingType.DownloadCountBase)
        {
            SchedulerType = type;
            Scheduler = sched;
        }


    }
}
