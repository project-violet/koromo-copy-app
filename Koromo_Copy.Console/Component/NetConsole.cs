// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.CL;
using Koromo_Copy.Framework.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Koromo_Copy.Console.Component
{
    public class NetConsoleOption : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;

        [CommandLine("--disable-auto-redirection", CommandType.OPTION,
            Info = "Disable auto redirection when http-request.", Help = "use net --disable-auto-redirection")]
        public bool DisableAutoRedirection;

        [CommandLine("--download-html", CommandType.ARGUMENTS, ArgumentsCount = 1, 
            Info = "Download html file.", Help = "use net --download-html <URL>")]
        public string[] DownloadHtml;
    }

    public class NetConsole
    {
        public static void Start(string[] arguments)
        {
            var option = CommandLineParser<NetConsoleOption>.Parse(arguments);

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
            else if (option.DownloadHtml != null)
            {
                ProcessDownloadHtml(option.DownloadHtml, option.DisableAutoRedirection);
            }
        }

        static void PrintHelp()
        {
            Runnable.PrintVersion();
            System.Console.WriteLine("Net - Network component\r\n");

            var builder = new StringBuilder();
            CommandLineParser<NetConsoleOption>.GetFields().ToList().ForEach(
                x =>
                {
                    if (!string.IsNullOrEmpty(x.Value.Item2.Info))
                        builder.Append($" {x.Key} : {x.Value.Item2.Info} [{x.Value.Item2.Help}]\r\n");
                    else
                        builder.Append($" {x.Key} [{x.Value.Item2.Help}]\r\n");
                });
            System.Console.WriteLine(builder.ToString());
        }

        static void ProcessDownloadHtml(string[] args, bool DisableAutoRedirection)
        {
            var task = NetTask.MakeDefault(args[0]);
            task.AutoRedirection = !DisableAutoRedirection;
            var html = NetTools.DownloadStringAsync(task).Result;
            if (html != null)
                System.Console.WriteLine(html);
            else
                System.Console.WriteLine("Internal error! Please, check log.txt file.");
        }
    }
}
