// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework;
using Koromo_Copy.Framework.CL;
using Koromo_Copy.Framework.Extractor;
using Koromo_Copy.Framework.Log;
using Koromo_Copy.Framework.Setting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Koromo_Copy.Console.Component
{
    public class ExtractConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

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
    }

    /// <summary>
    /// Selects extractor and extract something.
    /// </summary>
    public class ExtractConsole
    {
        public static void Start(string[] arguments)
        {
            arguments = CommandLineUtil.SplitCombinedOptions(arguments);
            arguments = CommandLineUtil.InsertWeirdArguments<ExtractConsoleOption>(arguments, true, "--url");
            var option = CommandLineParser<ExtractConsoleOption>.Parse(arguments);

            if (option.Error)
            {
                System.Console.WriteLine(option.ErrorMessage);
                if (option.HelpMessage != null)
                    System.Console.WriteLine(option.HelpMessage);
                return;
            }
            else if (option.Help)
            {
                PrintHelp();
            }
            else if (option.Url != null)
            {
                if (!(option.Url[0].StartsWith("https://") || option.Url[0].StartsWith("http://")))
                {
                    System.Console.WriteLine($"'{option.Url[0]}' is not correct url format or not supported scheme.");
                }

                ProcessExtract(option.Url[0], option.ExtractInformation, option.ExtractLinks, option.PrintProcess);
            }
        }

        static void PrintHelp()
        {
            Runnable.PrintVersion();
            System.Console.WriteLine("Extract - Extractor component\r\n");

            var builder = new StringBuilder();
            CommandLineParser<ExtractConsoleOption>.GetFields().ToList().ForEach(
                x =>
                {
                    var key = x.Key;
                    if (x.Value.Item2.ShortOption != "")
                        key += $" ({x.Value.Item2.ShortOption})";
                    if (!string.IsNullOrEmpty(x.Value.Item2.Info))
                        builder.Append($" {key} : {x.Value.Item2.Info} [{x.Value.Item2.Help}]\r\n");
                    else
                        builder.Append($" {key} [{x.Value.Item2.Help}]\r\n");
                });
            System.Console.WriteLine(builder.ToString());
        }

        static void ProcessExtract(string url, bool ExtractInformation, bool ExtractLinks, bool PrintProcess)
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
                if (ExtractInformation)
                {
                    var extractor_name = extractor.GetType().Name;
                    return;
                }

                try
                {
                    if (PrintProcess)
                    {
                        Logs.Instance.AddLogNotify((s, e) => {
                            lock (Logs.Instance.Log)
                            {
                                CultureInfo en = new CultureInfo("en-US");
                                System.Console.Error.WriteLine($"[{Logs.Instance.Log.Last().Item1.ToString(en)}] {Logs.Instance.Log.Last().Item2}");
                            }
                        });
                    }

                    var tasks = extractor.Extract(url, null);

                    if (ExtractLinks)
                    {
                        foreach (var uu in tasks.Item1)
                            System.Console.WriteLine(uu.Url);
                        return;
                    }

                    var option = extractor.RecommendOption(url);
                    var format = extractor.RecommendFormat(option);

                    int task_count = tasks.Item1.Count;
                    tasks.Item1.ForEach(task => {
                        task.Filename = Path.Combine(Settings.Instance.Model.SuperPath, task.Format.Formatting(format));
                        if (!Directory.Exists(Path.GetDirectoryName(task.Filename)))
                            Directory.CreateDirectory(Path.GetDirectoryName(task.Filename));
                        task.CompleteCallback = () =>
                        {
                            Interlocked.Decrement(ref task_count);
                        };
                    });
                    tasks.Item1.ForEach(task => AppProvider.Scheduler.Add(task));

                    while (task_count != 0)
                    {
                        Thread.Sleep(500);
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
