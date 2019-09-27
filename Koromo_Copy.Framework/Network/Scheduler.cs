// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

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

    public abstract class IField<T, P>
        where T : ISchedulerContents<T, P>
        where P : IComparable<P>
    {
        public abstract void Main(T content);
        public ManualResetEvent interrupt = new ManualResetEvent(true);
    }

    /// <summary>
    /// Scheduler Interface
    /// </summary>
    /// <typeparam name="T">Task type</typeparam>
    /// <typeparam name="P">Priority type</typeparam>
    /// <typeparam name="F">Field type</typeparam>
    public class Scheduler<T, P, F>
        : IScheduler<T>
        where T : ISchedulerContents<T, P>
        where P : IComparable<P>
        where F : IField<T, P>, new()
    {
        UpdatableHeap<T> queue = new UpdatableHeap<T>();

        public void update(UpdatableHeapElements<T> elem)
        {
            queue.Update(elem);
        }

        public int thread_count = 0;
        public int busy_thread = 0;
        public int capacity = 0;

        public List<Thread> threads = new List<Thread>();
        public List<ManualResetEvent> interrupt = new List<ManualResetEvent>();
        public List<F> field = new List<F>();

        object notify_lock = new object();

        public Scheduler(int capacity = 0, bool use_emergency_thread = false)
        {
            this.capacity = capacity;

            if (this.capacity == 0)
                this.capacity = Environment.ProcessorCount;

            thread_count = this.capacity;

            if (use_emergency_thread)
                thread_count += 1;

            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            for (int i = 0; i < this.capacity; i++)
            {
                interrupt.Add(new ManualResetEvent(false));
                threads.Add(new Thread(new ParameterizedThreadStart(remote_thread_handler)));
                threads.Last().Start(i);
            }

            for (int i = 0; i < this.capacity; i++)
            {
                field.Add(new F());
            }
        }

        private void remote_thread_handler(object i)
        {
            int index = (int)i;

            while (true)
            {
                interrupt[index].WaitOne();

                T task;

                lock(queue)
                {
                    if (queue.Count > 0)
                    {
                        task = queue.Front;
                        queue.Pop();
                    }
                    else
                    {
                        interrupt[index].Reset();
                        continue;
                    }
                }

                Interlocked.Increment(ref busy_thread);

                field[index].Main(task);

                Interlocked.Decrement(ref busy_thread);
            }
        }

        public void Pause()
        {
            field.ForEach(x => x.interrupt.Reset());
        }

        public void Resume()
        {
            field.ForEach(x => x.interrupt.Set());
        }

        public void Notify()
        {
            interrupt.ForEach(x => x.Set());
        }

        public void Add(T task)
        {
            task.scheduler = this;
            lock (queue) queue.Push(task);
            lock (notify_lock) Notify();
        }
    }

    public class NetScheduler : Scheduler<NetTask, NetPriority, NetField>
    {
        public NetScheduler(int capacity = 0, bool use_emergency_thread = false) 
            : base(capacity, use_emergency_thread) { }
    }
}
