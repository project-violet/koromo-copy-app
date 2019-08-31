// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework;
using Koromo_Copy.Framework.CL;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Koromo_Copy.Console
{
    public class Options : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;
        [CommandLine("-v", CommandType.OPTION, Default = true, Help = "Show version information.")]
        public bool Version;
        [CommandLine("--dialog-mode", CommandType.OPTION, Default = true, Help = "Run program with dialog mode.")]
        public bool DialogMode;
    }

    public class Runnable
    {
        public static void Start(string[] arguments)
        {
            var option = CommandLineParser<Options>.Parse(arguments);

            if (option.Error)
            {
                System.Console.WriteLine(option.ErrorMessage);
                if (option.HelpMessage != null)
                    System.Console.WriteLine(option.HelpMessage);
                return;
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

            return;
        }

        static void PrintHelp()
        {
            PrintVersion();

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

        static void PrintVersion()
        {
            System.Console.WriteLine($"{Version.Name} {Version.Text}\r\n");
        }
    }

}
