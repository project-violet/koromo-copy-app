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

        [CommandLine("--download-html", CommandType.ARGUMENTS, ArgumentsCount = 1, 
            Help = "Download html file.", Info = "use net --download-html <URL>")]
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
                ProcessDownloadHtml(option.DownloadHtml);
            }
        }

        static void PrintHelp()
        {
            Runnable.PrintVersion();

            var builder = new StringBuilder();
            CommandLineParser<Options>.GetFields().ToList().ForEach(
                x =>
                {
                    if (!string.IsNullOrEmpty(x.Value.Item2.Help))
                        builder.Append($" {x.Key} : {x.Value.Item2.Help}\r\n");
                    else
                        builder.Append($" {x.Key}\r\n");
                });
            System.Console.WriteLine(builder.ToString());
        }

        static void ProcessDownloadHtml(string[] args)
        {
            var html = NetTools.DownloadStringAsync(NetTask.MakeDefault(args[0])).Result;
            System.Console.WriteLine(html);
        }
    }
}
