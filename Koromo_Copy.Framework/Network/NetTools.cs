// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Koromo_Copy.Framework.Network
{
    public class NetTools
    {
        public static List<string> DownloadStrings(List<string> urls, string cookie = "")
        {
            var interrupt = new ManualResetEvent(false);
            var result = new string[urls.Count];
            var count = urls.Count;
            int iter = 0;

            foreach (var url in urls)
            {
                var itertmp = iter;
                var task = NetTask.MakeDefault(url);
                task.DownloadString = true;
                task.CompleteCallbackString = (str) =>
                {
                    result[itertmp] = str;
                    if (Interlocked.Decrement(ref count) == 0)
                        interrupt.Set();
                };
                task.ErrorCallback = (code) =>
                {
                    if (Interlocked.Decrement(ref count) == 0)
                        interrupt.Set();
                };
                task.Cookie = cookie;
                AppProvider.Scheduler.Add(task);
                iter++;
            }

            interrupt.WaitOne();

            return result.ToList();
        }

        public static string DownloadString(string url)
        {
            return DownloadStringAsync(NetTask.MakeDefault(url)).Result;
        }

        public static string DownloadString(NetTask task)
        {
            return DownloadStringAsync(task).Result;
        }

        public static async Task<string> DownloadStringAsync(NetTask task)
        {
            return await Task.Run(() =>
            {
                var interrupt = new ManualResetEvent(false);
                string result = null;

                task.DownloadString = true;
                task.CompleteCallbackString = (string str) =>
                {
                    result = str;
                    interrupt.Set();
                };

                task.ErrorCallback = (code) =>
                {
                    task.ErrorCallback = null;
                    interrupt.Set();
                };

                AppProvider.Scheduler.Add(task);

                interrupt.WaitOne();

                return result;
            }).ConfigureAwait(false);
        }

        public static void DownloadFile(string url, string filename)
        {
            var task = NetTask.MakeDefault(url);
            task.SaveFile = true;
            task.Priority = new NetPriority { Type = NetPriorityType.Low };
            DownloadFileAsync(task).Wait();
        }

        public static void DownloadFile(NetTask task)
        {
            DownloadFileAsync(task).Wait();
        }

        public static async Task DownloadFileAsync(NetTask task)
        {
            await Task.Run(() =>
            {
                var interrupt = new ManualResetEvent(false);

                task.SaveFile = true;
                task.CompleteCallback = () =>
                {
                    interrupt.Set();
                };

                task.ErrorCallback = (code) =>
                {
                    task.ErrorCallback = null;
                    interrupt.Set();
                };

                AppProvider.Scheduler.Add(task);

                interrupt.WaitOne();
            }).ConfigureAwait(false);
        }

        public static byte[] DownloadData(string url)
        {
            return DownloadDataAsync(NetTask.MakeDefault(url)).Result;
        }

        public static byte[] DownloadData(NetTask task)
        {
            return DownloadDataAsync(task).Result;
        }

        public static async Task<byte[]> DownloadDataAsync(NetTask task)
        {
            return await Task.Run(() =>
            {
                var interrupt = new ManualResetEvent(false);
                byte[] result = null;

                task.MemoryCache = true;
                task.CompleteCallbackBytes = (byte[] bytes) =>
                {
                    result = bytes;
                    interrupt.Set();
                };

                task.ErrorCallback = (code) =>
                {
                    task.ErrorCallback = null;
                    interrupt.Set();
                };

                AppProvider.Scheduler.Add(task);

                interrupt.WaitOne();

                return result;
            }).ConfigureAwait(false);
        }
    }
}
