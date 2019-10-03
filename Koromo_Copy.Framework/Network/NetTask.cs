// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Extractor;
using Koromo_Copy.Framework.Postprocessor;
using Koromo_Copy.Framework.Setting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Koromo_Copy.Framework.Network
{
    /// <summary>
    /// Information of what download for
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class NetTask : ISchedulerContents<NetTask, NetPriority>
    {
        public static NetTask MakeDefault(string url)
            => new NetTask
            {
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36",
                TimeoutInfinite = Settings.Instance.Network.TimeoutInfinite,
                TimeoutMillisecond = Settings.Instance.Network.TimeoutMillisecond,
                RetryWhenFail = true,
                RetryCount = Settings.Instance.Network.RetryCount,
                DownloadBufferSize = Settings.Instance.Network.DownloadBufferSize,
                Priority = new NetPriority() { Type = NetPriorityType.Trivial },
                Url = url
            };

        public static NetTask MakeDefaultMobile(string url)
            => new NetTask
            {
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                UserAgent = "Mozilla/5.0 (Android 7.0; Mobile; rv:54.0) Gecko/54.0 Firefox/54.0 AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.125 Mobile Safari/603.2.4",
                TimeoutInfinite = Settings.Instance.Network.TimeoutInfinite,
                TimeoutMillisecond = Settings.Instance.Network.TimeoutMillisecond,
                RetryWhenFail = true,
                RetryCount = Settings.Instance.Network.RetryCount,
                DownloadBufferSize = Settings.Instance.Network.DownloadBufferSize,
                Priority = new NetPriority() { Type = NetPriorityType.Trivial },
                Url = url
            };

        /* Task Information */

        [JsonProperty]
        public int Index { get; set; }

        /* Http Information */

        [JsonProperty]
        public string Url { get; set; }
        [JsonProperty]
        public string Accept { get; set; }
        [JsonProperty]
        public string Referer { get; set; }
        [JsonProperty]
        public string UserAgent { get; set; }
        [JsonProperty]
        public string Cookie { get; set; }
        [JsonProperty]
        public Dictionary<string, string> Headers { get; set; }
        [JsonProperty]
        public Dictionary<string, string> Query { get; set; }
        public IWebProxy Proxy { get; set; }

        /* Detail Information */

        /// <summary>
        /// Set if you want to download and save file to your own device.
        /// </summary>
        [JsonProperty]
        public bool SaveFile { get; set; }
        [JsonProperty]
        public string Filename { get; set; }
        [JsonProperty]
        public ExtractorFileNameFormat Format { get; set; }

        /// <summary>
        /// Set if needing only string datas.
        /// </summary>
        [JsonProperty]
        public bool DownloadString { get; set; }

        /// <summary>
        /// Download data to temporary directory on your device.
        /// </summary>
        [JsonProperty]
        public bool DriveCache { get; set; }

        /// <summary>
        /// Download data to memory.
        /// </summary>
        [JsonProperty]
        public bool MemoryCache { get; set; }

        /// <summary>
        /// Retry download when fail to download.
        /// </summary>
        [JsonProperty]
        public bool RetryWhenFail { get; set; }
        [JsonProperty]
        public int RetryCount { get; set; }

        /// <summary>
        /// Timeout settings
        /// </summary>
        [JsonProperty]
        public bool TimeoutInfinite { get; set; }
        [JsonProperty]
        public int TimeoutMillisecond { get; set; }

        [JsonProperty]
        public int DownloadBufferSize { get; set; }

        [JsonProperty]
        public bool AutoRedirection { get; set; }

        [JsonProperty]
        public bool NotifyOnlySize { get; set; }

        /* Callback Functions */

        public Action<long> SizeCallback;
        public Action<long> DownloadCallback;
        public Action StartCallback;
        public Action CompleteCallback;
        public Action<string> CompleteCallbackString;
        public Action<byte[]> CompleteCallbackBytes;

        /// <summary>
        /// Return total downloaded size
        /// </summary>
        public Action<int> RetryCallback;
        public Action<int> ErrorCallback;

        /* For NetField */

        public bool Aborted;
        public HttpWebRequest Request;

        /* Post Processor */

        public PostprocessorTask PostProcess;
        public Action StartPostprocessorCallback;
    }
}
