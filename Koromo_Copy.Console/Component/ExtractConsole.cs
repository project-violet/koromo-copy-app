// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.CL;
using Koromo_Copy.Framework.Extractor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        [CommandLine("-o", CommandType.ARGUMENTS, ArgumentsCount = 1,
            Info = "Set extracting file name format.", Help = "use -o <Output Format>")]
        public string[] PathFormat;

        [CommandLine("-i", CommandType.OPTION, Info = "Extract information of url.", Help = "use -i")]
        public bool ExtractInformation;
        [CommandLine("-l", CommandType.OPTION, Info = "Extract just links.", Help = "use -l")]
        public bool ExtractLinks;
        [CommandLine("-p", CommandType.OPTION, Info = "Print download processing.", Help = "use -p")]
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

                ProcessExtract(option.Url[0]);
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
                    if (!string.IsNullOrEmpty(x.Value.Item2.Info))
                        builder.Append($" {x.Key} : {x.Value.Item2.Info} [{x.Value.Item2.Help}]\r\n");
                    else
                        builder.Append($" {x.Key} [{x.Value.Item2.Help}]\r\n");
                });
            System.Console.WriteLine(builder.ToString());
        }

        static void ProcessExtract(string url)
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
                    System.Console.WriteLine($"[Extractor Name] {t.GetType().Name}");
                    System.Console.WriteLine(t.ExtractorInfo);
                    return;
                }
            }
        }
    }
}
