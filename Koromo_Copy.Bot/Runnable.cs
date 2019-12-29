// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.CL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Version = Koromo_Copy.Framework.Version;

namespace Koromo_Copy.Bot
{
    public class Options : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION)]
        public bool Help;
        [CommandLine("--version", CommandType.OPTION, ShortOption = "-v", Info = "Show version information.")]
        public bool Version;

        [CommandLine("--telegram", CommandType.OPTION, Info = "Run with telegram bot.", Help = "use --telegram")]
        public bool Telegram;
        [CommandLine("--discord", CommandType.OPTION, Info = "Run with discord bot.", Help = "use --discord")]
        public bool Discord;
        [CommandLine("--kakaotalk", CommandType.OPTION, Info = "Run with kakaotalk bot.", Help = "use --kakaotalk")]
        public bool Kakaotalk;

        [CommandLine("--api-key", CommandType.ARGUMENTS, ArgumentsCount = 1,
            Info = "Set api key if required.", Help = "use --api-key <API Key>")]
        public string[] APIKey;
        [CommandLine("--user-token", CommandType.ARGUMENTS, ArgumentsCount = 1,
            Info = "Set user token if required.", Help = "use --user-token <User Token>")]
        public string[] UserToken;
    }

    public class Runnable
    {
        public static void Start(string[] arguments)
        {
            arguments = CommandLineUtil.SplitCombinedOptions(arguments);
            arguments = CommandLineUtil.InsertWeirdArguments<Options>(arguments, true, "--url");
            var option = CommandLineParser.Parse<Options>(arguments);

            if (option.Help)
            {
                PrintHelp();
            }
            else if (option.Version)
            {
                PrintVersion();
            }
            else if (option.Error)
            {
                Console.WriteLine(option.ErrorMessage);
                if (option.HelpMessage != null)
                    Console.WriteLine(option.HelpMessage);
                return;
            }
            else
            {
                Console.WriteLine("Nothing to work on.");
                Console.WriteLine("Enter './Koromo_Copy.Bot --help' to get more information");
            }

            return;
        }

        static void PrintHelp()
        {
            PrintVersion();
            Console.WriteLine($"Copyright (C) 2019. Koromo Copy Developer");
            Console.WriteLine($"E-Mail: koromo.software@gmail.com");
            Console.WriteLine($"Source-code: https://github.com/koromo-copy/koromo-copy");
            Console.WriteLine($"");
            Console.WriteLine("Usage: ./Koromo_Copy.Bot [OPTIONS...] <URL> [URL OPTIONS ...]");

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
            Console.Write(builder.ToString());

            Console.WriteLine($"");
            Console.WriteLine("Enter './Koromo_Copy.Bot --list-extractor' to get more url options.");
        }

        public static void PrintVersion()
        {
            Console.WriteLine($"{Version.Name} {Version.Text}");
        }

    }
}
