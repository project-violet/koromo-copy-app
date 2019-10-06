// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Text;
using System.Threading;

namespace Koromo_Copy.Console
{
    /// <summary>
    /// An ASCII progress bar
    /// 
    /// Reference[MIT]: DanielSWolf - https://gist.github.com/DanielSWolf/0ab6a96899cc5377bf54
    /// </summary>

    public class WaitProgress : IDisposable
    {
        private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / 8);
        private const string animation = @"|/-\";
        private readonly Timer timer;

        private string currentText = string.Empty;
        private bool disposed = false;
        private int animationIndex = 0;

        public WaitProgress()
        {
            timer = new Timer(TimerHandler);

            if (!System.Console.IsOutputRedirected)
            {
                ResetTimer();
            }
        }

        private void TimerHandler(object state)
        {
            lock (timer)
            {
                if (disposed) return;

                UpdateText(animation[animationIndex++ % animation.Length].ToString());

                ResetTimer();
            }
        }

        private void UpdateText(string text)
        {
            int commonPrefixLength = 0;
            int commonLength = Math.Min(currentText.Length, text.Length);
            while (commonPrefixLength < commonLength && text[commonPrefixLength] == currentText[commonPrefixLength])
            {
                commonPrefixLength++;
            }

            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.Append('\b', currentText.Length - commonPrefixLength);

            outputBuilder.Append(text.Substring(commonPrefixLength));

            int overlapCount = currentText.Length - text.Length;
            if (overlapCount > 0)
            {
                outputBuilder.Append(' ', overlapCount);
                outputBuilder.Append('\b', overlapCount);
            }

            System.Console.Write(outputBuilder);
            currentText = text;
        }

        private void ResetTimer()
        {
            timer.Change(animationInterval, TimeSpan.FromMilliseconds(-1));
        }

        public void Dispose()
        {
            lock (timer)
            {
                disposed = true;
                UpdateText(string.Empty);
            }
        }
    }

    public class ProgressBar : IDisposable
    {
        private const int blockCount = 20;
        private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / 8);

        private readonly Timer timer;

        private double currentProgress = 0;
        private string currentText = string.Empty;
        private bool disposed = false;
        private long total_read_bytes = 0;
        private long read_bytes_cycle = 0;
        private long current_speed = 0;
        private long cycle_count = 0;
        private object report_lock = new object();

        public ProgressBar()
        {
            timer = new Timer(TimerHandler);

            if (!System.Console.IsOutputRedirected)
            {
                ResetTimer();
            }
        }

        public void Report(long total, long complete, long read_bytes)
        {
            var value = Math.Max(0, Math.Min(1, complete / (double)total));
            Interlocked.Exchange(ref currentProgress, value);
            lock (report_lock)
            {
                total_read_bytes += read_bytes;
                current_speed += read_bytes;
            }
        }

        private void TimerHandler(object state)
        {
            lock (timer)
            {
                if (disposed) return;
                long cs = current_speed * 8;
                lock (report_lock) { cycle_count++; current_speed = 0; }

                int progressBlockCount = (int)(currentProgress * blockCount);
                int percent = (int)(currentProgress * 100);

                string speed;
                if (cs > 1024 * 1024)
                    speed = (cs / (double)(1024 * 1024)).ToString("#,0.0") + " MB/S";
                else if (cs > 1024)
                    speed = (cs / (double)1024).ToString("#,0.0") + " KB/S";
                else
                    speed = cs.ToString("#,0") + " Byte/S";

                string downloads;
                if (total_read_bytes > 1024 * 1024 * 1024)
                    downloads = (total_read_bytes / (double)(1024 * 1024 * 1024)).ToString("#,0.0") + " GB";
                else if (total_read_bytes > 1024 * 1024)
                    downloads = (total_read_bytes / (double)(1024 * 1024)).ToString("#,0.0") + " MB";
                else if (total_read_bytes > 1024)
                    downloads = (total_read_bytes / (double)(1024)).ToString("#,0.0") + " KB";
                else
                    downloads = (total_read_bytes).ToString("#,0") + " Byte";

                string text = string.Format("[{0}{1}] {2,3}% ({3} {4})",
                    new string('#', progressBlockCount), new string('-', blockCount - progressBlockCount),
                    percent,
                    speed, downloads);
                UpdateText(text);

                ResetTimer();
            }
        }

        private void UpdateText(string text)
        {
            int commonPrefixLength = 0;
            int commonLength = Math.Min(currentText.Length, text.Length);
            while (commonPrefixLength < commonLength && text[commonPrefixLength] == currentText[commonPrefixLength])
            {
                commonPrefixLength++;
            }

            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.Append('\b', currentText.Length - commonPrefixLength);

            outputBuilder.Append(text.Substring(commonPrefixLength));

            int overlapCount = currentText.Length - text.Length;
            if (overlapCount > 0)
            {
                outputBuilder.Append(' ', overlapCount);
                outputBuilder.Append('\b', overlapCount);
            }

            System.Console.Write(outputBuilder);
            currentText = text;
        }

        private void ResetTimer()
        {
            timer.Change(animationInterval, TimeSpan.FromMilliseconds(-1));
        }

        public void Dispose()
        {
            lock (timer)
            {
                disposed = true;
                UpdateText(string.Empty);
            }
        }
    }
}
