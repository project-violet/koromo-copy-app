// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Koromo_Copy.Framework.Postprocessor
{
    public enum PostprocessorPriorityType
    {
        // ex) Zip, FFmpeg, Image merging
        Low = 0,
        // ex) ?
        Trivial = 1,
        // Not use
        Emergency = 2,
    }

    public class PostprocessorPriority : IComparable<PostprocessorPriority>
    {
        [JsonProperty]
        public PostprocessorPriorityType Type { get; set; }
        [JsonProperty]
        public int TaskPriority { get; set; }

        public int CompareTo(PostprocessorPriority pp)
        {
            if (Type > pp.Type) return 1;
            else if (Type < pp.Type) return -1;

            return pp.TaskPriority.CompareTo(TaskPriority);
        }
    }

    public class PostprocessorTask : ISchedulerContents<PostprocessorTask, PostprocessorPriority>
    {
        public PostprocessorTask()
        {
            Priority = new PostprocessorPriority { Type = PostprocessorPriorityType.Low };
        }

        public NetTask DownloadTask { get; set; }

        public IPostprocessor Postprocessor { get; set; }

        public Action<int> StartPostprocessor { get; set; }
        public Action<int> CompletePostprocessor { get; set; }
    }

    public class PostprocessorField : IField<PostprocessorTask, PostprocessorPriority>
    {
        public override void Main(PostprocessorTask content)
        {
            content.StartPostprocessor?.Invoke(content.DownloadTask.Index);

            content.Postprocessor.Run(content.DownloadTask);

            content.CompletePostprocessor?.Invoke(content.DownloadTask.Index);
        }
    }

    public class PostprocessorScheduler : Scheduler<PostprocessorTask, PostprocessorPriority, PostprocessorField>
    {
        public PostprocessorScheduler(int capacity = 0, bool use_emergency_thread = false)
            : base(capacity, use_emergency_thread) { }
    }
}
