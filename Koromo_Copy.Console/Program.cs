// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.CL;
using System;

namespace Koromo_Copy.Console
{
    public class Options : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;
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
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Runnable.Start(args);
        }
    }
}
