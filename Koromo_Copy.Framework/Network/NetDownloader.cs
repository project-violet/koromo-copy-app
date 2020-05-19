// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Linq;
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
        public int Index { get; set; }
        public string Name { get; set; }
        public List<NetTask> Tasks { get; set; }
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
        public int Capacity { get; private set; }
        public int AvailableGroup { get; private set; }

        Queue<int> available_index;
        NetTaskGroup[] managed_ntg;

        public NetDownloader(NetScheduler sched, int capacity = 4, NetDownloaderSchedulingType type = NetDownloaderSchedulingType.DownloadCountBase)
        {
            SchedulerType = type;
            Scheduler = sched;
            Capacity = capacity;
            AvailableGroup = 0;
            managed_ntg = new NetTaskGroup[capacity];
            available_index = new Queue<int>();
            Enumerable.Range(0, 4).ToList().ForEach(x => available_index.Enqueue(x));
        }

        private bool check_sched_full() => Capacity == AvailableGroup;
        private void attach_to_sched(NetTaskGroup ntg)
        {
            ntg.Index = available_index.Dequeue();
            managed_ntg[ntg.Index] = ntg;

        }
    }
}
