// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Console.Component;
using Koromo_Copy.Framework;
using Koromo_Copy.Framework.CL;
using Koromo_Copy.Framework.Crypto;
using Koromo_Copy.Framework.Extractor;
using Koromo_Copy.Framework.Log;
using Koromo_Copy.Framework.Network;
using Koromo_Copy.Framework.Setting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Version = Koromo_Copy.Framework.Version;

namespace Koromo_Copy.Console
{
    public class Options : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION)]
        public bool Help;
        [CommandLine("--version", CommandType.OPTION, ShortOption = "-v", Info = "Show version information.")]
        public bool Version;
        [CommandLine("--dialog-mode", CommandType.OPTION, Info = "Run program with dialog mode.")]
        public bool DialogMode;

        [CommandLine("--net", CommandType.OPTION, Info = "Multi-commands net.", Help = "use --net <Others>")]
        public bool Net;

        [CommandLine("--list-extractor", CommandType.OPTION, Info = "Enumerate all implemented extractor.")]
        public bool ListExtractor;

        [CommandLine("--url", CommandType.ARGUMENTS, ArgumentsCount = 1,
            Info = "Set extracting target.", Help = "use --url <URL>")]
        public string[] Url;
        [CommandLine("--path-format", CommandType.ARGUMENTS, ShortOption = "-o", ArgumentsCount = 1,
            Info = "Set extracting file name format.", Help = "use -o <Output Format>")]
        public string[] PathFormat;

        [CommandLine("--extract-info", CommandType.OPTION, ShortOption = "-i", Info = "Extract information of url.", Help = "use -i")]
        public bool ExtractInformation;
        [CommandLine("--extract-link", CommandType.OPTION, ShortOption = "-l", Info = "Extract just links.", Help = "use -l")]
        public bool ExtractLinks;
        [CommandLine("--print-process", CommandType.OPTION, ShortOption = "-p", Info = "Print download processing.", Help = "use -p")]
        public bool PrintProcess;

        [CommandLine("--disable-download-progress", CommandType.OPTION, Info = "Disable download progress.", Help = "use --disable-download-progress")]
        public bool DisableDownloadProgress;


        [CommandLine("--page-start", CommandType.OPTION, Info = "Specify a start page when crawling a multi-page bulletin board.", Help = "use --page-start <Number>")]
        public string[] PageStart;
        [CommandLine("--page-end", CommandType.OPTION, Info = "Specify a end page when crawling a multi-page bulletin board.", Help = "use --page-end <Number>")]
        public string[] PageEnd;
    }

    public class Runnable
    {
        public static void Start(string[] arguments)
        {
            var origin = arguments;
            arguments = CommandLineUtil.SplitCombinedOptions(arguments);
            arguments = CommandLineUtil.InsertWeirdArguments<Options>(arguments, true, "--url");
            var option = CommandLineParser.Parse<Options>(arguments);

            //
            //  Multi Commands
            //
            if (option.Net)
            {
                NetConsole.Start(origin.Skip(1).ToArray());
            }
            //
            //  Single Commands
            //
            else if (option.Help)
            {
                PrintHelp();
            }
            else if (option.Version)
            {
                PrintVersion();
            }
            else if (option.DialogMode)
            {
                Dialog.StartDialog();
            }
            else if (option.ListExtractor)
            {
                foreach (var extractor in ExtractorManager.Extractors)
                {
                    System.Console.WriteLine($"[{extractor.GetType().Name}]");
                    System.Console.WriteLine($"[HostName] {extractor.HostName}");
                    System.Console.WriteLine($"[Checker] {extractor.ValidUrl}");
                    System.Console.WriteLine($"[Information] {extractor.ExtractorInfo}");
                    var builder = new StringBuilder();
                    CommandLineParser.GetFields(extractor.RecommendOption("").GetType()).ToList().ForEach(
                        x =>
                        {
                            var key = x.Key;
                            if (!key.StartsWith("--"))
                                return;
                            if (!string.IsNullOrEmpty(x.Value.Item2.ShortOption))
                                key = $"{x.Value.Item2.ShortOption}, " + key;
                            var help = "";
                            if (!string.IsNullOrEmpty(x.Value.Item2.Help))
                                help = $"[{x.Value.Item2.Help}]";
                            if (!string.IsNullOrEmpty(x.Value.Item2.Info))
                                builder.Append($"   {key}".PadRight(30) + $" {x.Value.Item2.Info} {help}\r\n");
                            else
                                builder.Append($"   {key}".PadRight(30) + $" {help}\r\n");
                        });
                    if (builder.ToString() != "")
                    {
                        System.Console.WriteLine($"[Options]");
                        System.Console.Write(builder.ToString());
                    }
                    System.Console.WriteLine($"-------------------------------------------------------------");
                }
            }
            else if (option.Url != null)
            {
                if (!(option.Url[0].StartsWith("https://") || option.Url[0].StartsWith("http://")))
                {
                    System.Console.WriteLine($"'{option.Url[0]}' is not correct url format or not supported scheme.");
                }

                var weird = CommandLineUtil.GetWeirdArguments<Options>(arguments);
                var n_args = new List<string>();

                weird.ForEach(x => n_args.Add(arguments[x]));

                ProcessExtract(option.Url[0], n_args.ToArray(), option.PathFormat, option.ExtractInformation, option.ExtractLinks, option.PrintProcess, option.DisableDownloadProgress);
            }
            else if (option.Error)
            {
                System.Console.WriteLine(option.ErrorMessage);
                if (option.HelpMessage != null)
                    System.Console.WriteLine(option.HelpMessage);
                return;
            }
            else
            {
                System.Console.WriteLine("Nothing to work on.");
                System.Console.WriteLine("Enter './Koromo_Copy.Console --help' to get more information");
            }

            return;
        }

        static void PrintHelp()
        {
            PrintVersion();
            System.Console.WriteLine($"Copyright (C) 2019. Koromo Copy Developer");
            System.Console.WriteLine($"E-Mail: koromo.software@gmail.com");
            System.Console.WriteLine($"Source-code: https://github.com/koromo-copy/koromo-copy");
            System.Console.WriteLine($"");
            System.Console.WriteLine("Usage: ./Koromo_Copy.Console [OPTIONS...] <URL> [URL OPTIONS ...]");

            var builder = new StringBuilder();
            CommandLineParser.GetFields(typeof(Options)).ToList().ForEach(
                x =>
                {
                    var key = x.Key;
                    if (!key.StartsWith("--"))
                        return;
                    if (!string.IsNullOrEmpty(x.Value.Item2.ShortOption))
                        key = $"{x.Value.Item2.ShortOption}, " + key;
                    var help = "";
                    if (!string.IsNullOrEmpty(x.Value.Item2.Help))
                        help = $"[{x.Value.Item2.Help}]";
                    if (!string.IsNullOrEmpty(x.Value.Item2.Info))
                        builder.Append($"   {key}".PadRight(30) + $" {x.Value.Item2.Info} {help}\r\n");
                    else
                        builder.Append($"   {key}".PadRight(30) + $" {help}\r\n");
                });
            System.Console.Write(builder.ToString());

            System.Console.WriteLine($"");
            System.Console.WriteLine("Enter './Koromo_Copy.Console --list-extractor' to get more url options.");
        }

        public static void PrintVersion()
        {
            System.Console.WriteLine($"{Version.Name} {Version.Text}");
        }

        static void ProcessExtract(string url, string[] args, string[] PathFormat, bool ExtractInformation, bool ExtractLinks, bool PrintProcess, bool DisableDownloadProgress)
        {
            var extractor = ExtractorManager.Instance.GetExtractor(url);

            if (extractor == null)
            {
                extractor = ExtractorManager.Instance.GetExtractorFromHostName(url);

                if (extractor == null)
                {
                    System.Console.WriteLine($"[Error] Cannot find a suitable extractor for '{url}'.");
                    return;
                }
                else
                {
                    System.Console.WriteLine("[Warning] Found an extractor for that url, but the url is not in the proper format to continue.");
                    System.Console.WriteLine("[Warning] Please refer to the following for proper conversion.");
                    System.Console.WriteLine($"[Input URL] {url}");
                    System.Console.WriteLine($"[Extractor Name] {extractor.GetType().Name}");
                    System.Console.WriteLine(extractor.ExtractorInfo);
                    return;
                }
            }
            else
            {
                try
                {
                    System.Console.WriteLine("Extractor Selected: " + extractor.GetType().Name.Replace("Extractor", ""));
                    System.Console.Write("Extracting urls... ");

                    WaitProgress wp = null;

                    if (PrintProcess)
                    {
                        Logs.Instance.AddLogNotify((s, e) => {
                            var tuple = s as Tuple<DateTime, string, bool>;
                            CultureInfo en = new CultureInfo("en-US");
                            System.Console.WriteLine($"[{tuple.Item1.ToString(en)}] {tuple.Item2}");
                        });
                    }
                    else
                    {
                        if (!DisableDownloadProgress)
                            wp = new WaitProgress();
                    }

                    var option = extractor.RecommendOption(url);
                    option.CLParse(ref option, args);
                    
                    if (option.Error)
                    {
                        if (wp != null) wp.Dispose();
                        System.Console.WriteLine($"[Input URL] {url}");
                        System.Console.WriteLine($"[Extractor Name] {extractor.GetType().Name}");
                        System.Console.WriteLine(option.ErrorMessage);
                        if (option.HelpMessage != null)
                            System.Console.WriteLine(option.HelpMessage);
                        return;
                    }

                    var tasks = extractor.Extract(url, option);

                    if (wp != null)
                    {
                        wp.Dispose();
                        System.Console.WriteLine("Done.");
                    }
                    
                    if (ExtractLinks)
                    {
                        foreach (var uu in tasks.Item1)
                            System.Console.WriteLine(uu.Url);
                        return;
                    }

                    string format;

                    if (PathFormat != null)
                        format = PathFormat[0];
                    else
                        format = extractor.RecommendFormat(option);

                    if (ExtractInformation)
                    {
                        System.Console.WriteLine($"[Input URL] {url}");
                        System.Console.WriteLine($"[Extractor Name] {extractor.GetType().Name}");
                        System.Console.WriteLine($"[Information] {extractor.ExtractorInfo}");
                        System.Console.WriteLine($"[Format] {format}");
                        return;
                    }

                    if (tasks.Item1 == null)
                    {
                        if (tasks.Item2 == null)
                        {
                            System.Console.WriteLine($"[Input URL] {url}");
                            System.Console.WriteLine($"[Extractor Name] {extractor.GetType().Name}");
                            System.Console.WriteLine("Nothing to work on.");
                            return;
                        }

                        System.Console.WriteLine(Logs.SerializeObject(tasks.Item2));
                        return;
                    }

                    int download_count = 0;

                    ProgressBar pb = null;

                    if (!PrintProcess && !DisableDownloadProgress)
                    {
                        System.Console.Write("Download files... ");
                        pb = new ProgressBar();
                    }

                    tasks.Item1.ForEach(task => {
                        task.Filename = Path.Combine(Settings.Instance.Model.SuperPath, task.Format.Formatting(format));
                        if (!Directory.Exists(Path.GetDirectoryName(task.Filename)))
                            Directory.CreateDirectory(Path.GetDirectoryName(task.Filename));
                        if (!PrintProcess && !DisableDownloadProgress)
                        {
                            task.DownloadCallback = (sz) =>
                                pb.Report(tasks.Item1.Count, download_count, sz);
                            task.CompleteCallback = () =>
                                Interlocked.Increment(ref download_count);
                        }
                        AppProvider.Scheduler.Add(task);
                    });

                    while (AppProvider.Scheduler.busy_thread != 0 || AppProvider.PPScheduler.busy_thread != 0)
                    {
                        Thread.Sleep(500);
                    }

                    if (pb != null)
                    { 
                        pb.Dispose();
                        System.Console.WriteLine("Done.");
                    }
                }
                catch (Exception e)
                {
                    Logs.Instance.PushError("[Extractor] Unhandled Exception - " + e.Message + "\r\n" + e.StackTrace);
                }
            }
        }
    }

}
