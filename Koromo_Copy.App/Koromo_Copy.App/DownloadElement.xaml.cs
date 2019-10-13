// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework;
using Koromo_Copy.Framework.Extractor;
using Koromo_Copy.Framework.Log;
using Koromo_Copy.Framework.Network;
using Koromo_Copy.Framework.Setting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamEffects;

namespace Koromo_Copy.App
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DownloadElement : ContentView
    {
        public static int DownloadAvailable = 0;
        public DownloadElement(string url, bool completed)
        {
            InitializeComponent();

            Info.Text = url;

            int hitomi_id = 0;
            if (int.TryParse(url, out hitomi_id))
                url = "https://hitomi.la/galleries/" + url + ".html";

            Commands.SetTap(Body, new Command(async () =>
            {
                await (Application.Current.MainPage as MainPage).NaviInstance.PushAsync(new DownloadInfoPage());
            }));

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                Status.Text = "옳바른 URL이 아닙니다!";
                Status.TextColor = Color.Red;
                Spinner.IsVisible = false;
                return;
            }

            var uri = new Uri(url);
            var favurl = uri.Scheme + Uri.SchemeDelimiter + uri.Host + "/favicon.ico";

            // Fav Exceptions
            if (favurl.StartsWith("https://hitomi.la/"))
                favurl = "https://ltn.hitomi.la/favicon-32x32.png";
            else if (favurl.StartsWith("https://www.instagram.com/"))
                favurl = "https://www.instagram.com/static/images/ico/favicon-192.png/68d99ba29cc8.png";
            else if (favurl.StartsWith("https://twitter.com/"))
                favurl = "https://abs.twimg.com/icons/apple-touch-icon-192x192.png";

            Fav.IsVisible = true;
            Fav.Source = favurl;

            Task.Run(() =>
            {
                var extractor = ExtractorManager.Instance.GetExtractor(url);
                if (extractor == null)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Status.Text = "적절한 다운로드작업을 찾을 수 없습니다!";
                        Status.TextColor = Color.Red;
                        Spinner.IsVisible = false;
                    });
                    return;
                }

            WAIT_ANOTHER_TASKS:

                if (DownloadAvailable == 4)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Status.Text = $"다른 작업이 끝나길 기다리는 중 입니다...";
                    });
                    while (DownloadAvailable >= 4)
                    {
                        Thread.Sleep(1000);
                    }
                }
                
                if (Interlocked.Increment(ref DownloadAvailable) > 4)
                {
                    goto WAIT_ANOTHER_TASKS;
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    Info.Text = extractor.GetType().Name.Replace("Extractor", "") + " (" + "/" + string.Join("/", url.Split('/').Skip(3)) + ")";
                    Status.Text = "다운로드할 파일들의 정보를 추출 중 입니다...";
                });

                var option = extractor.RecommendOption(url);

                long extracting_progress_max = 0;
                long extracting_cumulative_count = 0;

                option.ProgressMax = (count) =>
                {
                    extracting_progress_max = count;
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ProgressText.IsVisible = true;
                        Progress.IsVisible = true;
                    });
                };

                option.PostStatus = (count) =>
                {
                    var val = Interlocked.Add(ref extracting_cumulative_count, count);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (extracting_progress_max != 0)
                        {
                            Progress.Progress = val / (double)extracting_progress_max;
                            Status.Text = $"추출중...[{val}/{extracting_progress_max}]";
                        }
                        else
                        {
                            Status.Text = $"추출중...[{val}개 항목 추출됨]";
                        }
                    });
                };

                option.SimpleInfoCallback = (info) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Info.Text = extractor.GetType().Name.Replace("Extractor", "") + info;
                    });
                };

                (List<NetTask>, ExtractedInfo) tasks;

                try
                {
                    tasks = extractor.Extract(url, option);
                }
                catch (Exception e)
                {
                    Logs.Instance.PushError(e.Message);
                    Logs.Instance.PushError(e.StackTrace);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ProgressText.IsVisible = false;
                        Progress.IsVisible = false;
                        Spinner.IsVisible = false;
                        Status.Text = "추출 작업 중 오류가 발생했습니다 :(\n" + e.Message;
                        Status.TextColor = Color.Red;
                    });
                    return;
                }

                if (tasks.Item1 == null)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ProgressText.IsVisible = false;
                        Progress.IsVisible = false;
                        Spinner.IsVisible = false;
                        Status.Text = "다운로드할 내용이 없습니다 :(";
                    });
                    return;
                }

                var format = extractor.RecommendFormat(option);

                Device.BeginInvokeOnMainThread(() =>
                {
                    Spinner.IsVisible = false;
                    Status.Text = "다운로드 중...";
                    ProgressText.IsVisible = true;
                    Progress.IsVisible = true;
                });

                int download_count = 0;
                long download_bytes = 0;
                long download_1s = 0;
                int task_count = AppProvider.Scheduler.LatestPriority != null ? 
                                AppProvider.Scheduler.LatestPriority.TaskPriority : 0;

                tasks.Item1.ForEach(task => {
                    task.Priority.TaskPriority = task_count++;
                    task.Filename = Path.Combine(Settings.Instance.Model.SuperPath, task.Format.Formatting(format));
                    if (!Directory.Exists(Path.GetDirectoryName(task.Filename)))
                        Directory.CreateDirectory(Path.GetDirectoryName(task.Filename));
                    task.DownloadCallback = (sz) =>
                    {
                        Interlocked.Add(ref download_1s, sz);
                        Interlocked.Add(ref download_bytes, sz);
                    };
                    task.CompleteCallback = () =>
                    {
                        var cur = Interlocked.Increment(ref download_count);
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Progress.Progress = cur / (double)tasks.Item1.Count;
                        });
                    };
                    AppProvider.Scheduler.Add(task);
                });

                while (tasks.Item1.Count != download_count)
                {
                    Thread.Sleep(1000);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Status.Text = $"다운로드 중...[{download_count}/{tasks.Item1.Count}] ({convert_bytes2string(download_1s)}/S {convert_bytes2string(download_bytes)})";
                        Interlocked.Exchange(ref download_1s, 0);
                    });
                }

                Interlocked.Decrement(ref DownloadAvailable);

                Device.BeginInvokeOnMainThread(() =>
                {
                    Status.Text = "다운로드 완료";
                    ProgressText.IsVisible = false;
                    Progress.IsVisible = false;
                    Plugin.XSnack.CrossXSnack.Current.ShowMessage(url + " 항목의 다운로드가 완료되었습니다.");
                });
            });
        }

        private string convert_bytes2string(long bytes)
        {
            string downloads;
            if (bytes > 1024 * 1024 * 1024)
                downloads = (bytes / (double)(1024 * 1024 * 1024)).ToString("#,0.0") + " GB";
            else if (bytes > 1024 * 1024)
                downloads = (bytes / (double)(1024 * 1024)).ToString("#,0.0") + " MB";
            else if (bytes > 1024)
                downloads = (bytes / (double)(1024)).ToString("#,0.0") + " KB";
            else
                downloads = (bytes).ToString("#,0") + " Byte";
            return downloads;
        }
    }
}