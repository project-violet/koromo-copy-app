// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Koromo_Copy.Framework.Network
{
    public class NetTools
    {
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

                task.ErrorCallback = (int code) =>
                {
                    task.ErrorCallback = null;
                    interrupt.Set();
                };

                AppProvider.Scheduler.Add(task);

                interrupt.WaitOne();

                return result;
            });
        }
    }
}
