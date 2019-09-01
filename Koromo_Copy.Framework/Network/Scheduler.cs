// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Koromo_Copy.Framework.Network
{
    public interface IScheduler<T>
        where T : IComparable<T>
    {
        void update(UpdatableHeapElements<T> elem);
    }

    public class ISchedulerContents<T, P>
        : IComparable<ISchedulerContents<T, P>>
        where T : IComparable<T>
        where P : IComparable<P>
    {
        /* Scheduler Information */
        P priority;

        public P Priority { get { return priority; } set { priority = value; if (scheduler != null) scheduler.update(heap_elements); } }
        public int CompareTo(ISchedulerContents<T, P> other)
            => Priority.CompareTo(other.Priority);

        public UpdatableHeapElements<T> heap_elements;
        public IScheduler<T> scheduler;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Task type</typeparam>
    /// <typeparam name="P">Priority type</typeparam>
    /// <typeparam name="F">Field type</typeparam>
    public class Scheduler<T, P, F>
        : IScheduler<T>
        where T : ISchedulerContents<T, P>
        where P : IComparable<P>
    {
        UpdatableHeap<T> queue = new UpdatableHeap<T>();

        public void update(UpdatableHeapElements<T> elem)
        {
            queue.Update(elem);
        }
    }

    public class NetScheduler : Scheduler<NetTask, NetPriority, int> { }
}
