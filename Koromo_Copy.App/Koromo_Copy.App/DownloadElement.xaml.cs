// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using FFImageLoading;
using Koromo_Copy.App.DataBase;
using Koromo_Copy.Framework;
using Koromo_Copy.Framework.Cache;
using Koromo_Copy.Framework.Crypto;
using Koromo_Copy.Framework.Extractor;
using Koromo_Copy.Framework.Log;
using Koromo_Copy.Framework.Network;
using Koromo_Copy.Framework.Setting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        public DownloadInfo DownloadInfo = new DownloadInfo();
        public ExtractedInfo ExtractedInfo;
        public CancellationTokenSource CancelSource = new CancellationTokenSource();

        public DownloadElement(DownloadDBModel dbm)
        {
            InitializeComponent();

            Commands.SetTap(Body, new Command(async () =>
            {
                await (Application.Current.MainPage as MainPage).NaviInstance.PushAsync(new DownloadInfoPage());
            }));

            Spinner.IsVisible = false;

            SetupFavicon(dbm.Url);

            if (!string.IsNullOrWhiteSpace(dbm.ShortInfo))
                Info.Text = dbm.ShortInfo;
            else
                Info.Text = dbm.Url;

            ProgressText.IsVisible = true;
            ProgressText.Text = "날짜";

            ProgressProgressText.IsVisible = true;
            ProgressProgressText.Text = dbm.StartsTime.ToString();

            switch (dbm.State)
            {
                case DownloadDBState.Aborted:
                    Status.Text = "다운로드 취소됨";
                    break;

                case DownloadDBState.Downloaded:
                    Status.Text = "다운로드됨";
                    break;

                case DownloadDBState.ErrorOccured:
                    Status.Text = "다운로드 중 오류발생";
                    break;

                case DownloadDBState.Downloading:
                    Status.Text = "다운로드 중단됨";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(dbm.ThumbnailCahce))
            {
                Task.Run(() =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        var thumbnail = dbm.ThumbnailCahce;
                        Thumbnail.HeightRequest = Height - 8;
                        Thumbnail.IsVisible = true;
                        Thumbnail.Source = thumbnail;
                    });
                });
            }
        }

        public DownloadElement(string url)
        {
            InitializeComponent();

            DownloadInfo.DownloadStarts = DateTime.Now;
            Info.Text = url;

            int hitomi_id = 0;
            if (int.TryParse(url, out hitomi_id))
                url = "https://hitomi.la/galleries/" + url + ".html";

            var dbm = new DownloadDBModel();
            dbm.Url = url;
            dbm.StartsTime = DownloadInfo.DownloadStarts;
            dbm.State = DownloadDBState.Downloading;
            DownloadDBManager.Instance.Add(dbm);

            Commands.SetTap(Body, new Command(async () =>
            {
                await (Application.Current.MainPage as MainPage).NaviInstance.PushAsync(new DownloadInfoPage());
            }));

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                Status.Text = "옳바른 URL이 아닙니다!";
                Status.TextColor = Color.Red;
                Spinner.IsVisible = false;
                dbm.State = DownloadDBState.ErrorOccured;
                DownloadDBManager.Instance.Add(dbm);
                return;
            }

            SetupFavicon(url);

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
                    dbm.State = DownloadDBState.ErrorOccured;
                    DownloadDBManager.Instance.Update(dbm);
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
                    dbm.ShortInfo = Info.Text;
                    DownloadDBManager.Instance.Update(dbm);
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
                        ProgressProgressText.IsVisible = false;
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
                        Info.Text = $"{info}";
                        dbm.ShortInfo = info;
                        DownloadDBManager.Instance.Update(dbm);
                    });
                };

                option.ThumbnailCallback = (thumbnail) =>
                {
                    Task.Run(async () =>
                    {
                        var ttask = NetTask.MakeDefault(thumbnail.Url);
                        ttask.Priority = new NetPriority { Type = NetPriorityType.Trivial };
                        ttask.Filename = Path.Combine(AppProvider.ApplicationPath, (url + "*thumbnail" + dbm.Id).GetHashMD5() + Path.GetExtension(thumbnail.Filename));
                        ttask.Headers = thumbnail.Headers;
                        ttask.Referer = thumbnail.Referer;
                        ttask.Cookie = thumbnail.Cookie;
                        ttask.Accept = thumbnail.Accept;
                        ttask.UserAgent = thumbnail.UserAgent;
                        dbm.ThumbnailCahce = ttask.Filename;
                        await NetTools.DownloadFileAsync(ttask);
                        DownloadDBManager.Instance.Update(dbm);

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Thumbnail.HeightRequest = Height - 8;
                            Thumbnail.IsVisible = true;
                            Thumbnail.Source = ttask.Filename;
                        });
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
                        ProgressProgressText.IsVisible = true;
                        ProgressProgressText.Text = "";
                        ProgressText.Text = "";
                        Progress.IsVisible = false;
                        Spinner.IsVisible = false;
                        Status.Text = "추출 작업 중 오류가 발생했습니다 :(\n" + e.Message;
                        Status.TextColor = Color.Red;
                    });
                    Interlocked.Decrement(ref DownloadAvailable);
                    dbm.State = DownloadDBState.ErrorOccured;
                    DownloadDBManager.Instance.Update(dbm);
                    return;
                }

                if (tasks.Item1 == null)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ProgressProgressText.IsVisible = true;
                        ProgressProgressText.Text = "";
                        ProgressText.Text = "";
                        Progress.IsVisible = false;
                        Spinner.IsVisible = false;
                        Status.Text = "다운로드할 내용이 없습니다 :(";
                    });
                    Interlocked.Decrement(ref DownloadAvailable);
                    dbm.State = DownloadDBState.ErrorOccured;
                    DownloadDBManager.Instance.Update(dbm);
                    return;
                }

                if (tasks.Item2 != null)
                {
                    ExtractedInfo = tasks.Item2;
                    CacheManager.Instance.Append(url + "*info" + dbm.Id, ExtractedInfo);
                    dbm.InfoCache = url + "*info" + dbm.Id;
                    DownloadDBManager.Instance.Update(dbm);
                }

                var format = extractor.RecommendFormat(option);

                Device.BeginInvokeOnMainThread(() =>
                {
                    Spinner.IsVisible = false;
                    Status.Text = "다운로드 중...";
                    ProgressProgressText.IsVisible = false;
                    Progress.IsVisible = true;
                });

                int download_count = 0;
                long download_bytes = 0;
                long download_1s = 0;
                int task_count = AppProvider.Scheduler.LatestPriority != null ? 
                                AppProvider.Scheduler.LatestPriority.TaskPriority : 0;
                int post_process_count = 0;
                int post_process_progress = 0;
                bool canceled = false;

                if (tasks.Item1.Count > 0)
                {
                    dbm.Directory = Path.GetDirectoryName(Path.Combine(Settings.Instance.Model.SuperPath, tasks.Item1[0].Format.Formatting(format)));
                    DownloadDBManager.Instance.Update(dbm);
                }

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
                    task.CancleCallback = () =>
                    {
                        if (!canceled)
                        {
                            dbm.State = DownloadDBState.Aborted;
                            DownloadDBManager.Instance.Update(dbm);
                        }
                        canceled = true;
                    };
                    task.Cancel = CancelSource.Token;
                    if (task.PostProcess != null)
                    {
                        task.StartPostprocessorCallback = () =>
                        {
                            Interlocked.Increment(ref post_process_count);
                        };
                        task.PostProcess.CompletePostprocessor = (index) =>
                        {
                            Interlocked.Increment(ref post_process_progress);
                        };
                    }
                    AppProvider.Scheduler.Add(task);
                });

                while (tasks.Item1.Count != download_count && !canceled)
                {
                    Thread.Sleep(1000);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Status.Text = $"다운로드 중...[{download_count}/{tasks.Item1.Count}] ({convert_bytes2string(download_1s)}/S {convert_bytes2string(download_bytes)})";
                        Interlocked.Exchange(ref download_1s, 0);
                    });
                }

                Interlocked.Decrement(ref DownloadAvailable);

                dbm.State = DownloadDBState.Downloaded;
                dbm.EndsTime = DateTime.Now;
                DownloadDBManager.Instance.Update(dbm);

                while (post_process_progress != post_process_count && !canceled)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Status.Text = $"후처리 작업 중...[{post_process_progress}/{post_process_count}]";
                        Progress.Progress = post_process_progress / (double)post_process_count;
                    });
                    Thread.Sleep(1000);
                }

                if (!canceled)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Status.Text = "다운로드 완료";
                        ProgressProgressText.IsVisible = true;
                        ProgressProgressText.Text = "";
                        ProgressText.Text = "";
                        Progress.IsVisible = false;
                        Plugin.XSnack.CrossXSnack.Current.ShowMessage(Info.Text + " 항목의 다운로드가 완료되었습니다.");
                    });
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Status.Text = "다운로드 취소됨";
                        ProgressProgressText.IsVisible = true;
                        ProgressProgressText.Text = "";
                        ProgressText.Text = "";
                        Progress.IsVisible = false;
                        Plugin.XSnack.CrossXSnack.Current.ShowMessage(Info.Text + " 항목의 다운로드가 취소되었습니다.");
                    });
                }

                DownloadInfo.DownloadEnds = DateTime.Now;
            });
        }

        private void SetupFavicon(string url)
        {
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
        }

        private static HttpClient Task2HC(NetTask task)
        {
            var hc = new HttpClient();
            var uri = new Uri(task.Url);

            hc.DefaultRequestHeaders.Add("Accept", task.Accept);
            hc.DefaultRequestHeaders.Add("User-Agent", task.UserAgent);

            if (task.Referer != null)
                hc.DefaultRequestHeaders.Add("Referer", task.Referer);
            else
                hc.DefaultRequestHeaders.Add("Referer", (task.Url.StartsWith("https://") ? "https://" : (task.Url.Split(':')[0] + "//")) + uri.Host);

            if (task.Cookie != null)
                hc.DefaultRequestHeaders.Add("Cookie", task.Cookie);

            if (task.Headers != null)
                task.Headers.ToList().ForEach(p => hc.DefaultRequestHeaders.Add(p.Key, p.Value));

            return hc;
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