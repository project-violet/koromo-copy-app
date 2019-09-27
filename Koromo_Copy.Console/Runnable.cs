// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Console.Component;
using Koromo_Copy.Framework;
using Koromo_Copy.Framework.CL;
using Koromo_Copy.Framework.Crypto;
using Koromo_Copy.Framework.Network;
using System.Collections.Generic;
using System.IO;
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
        [CommandLine("--dialog-mode", CommandType.OPTION, Help = "Run program with dialog mode.")]
        public bool DialogMode;

#if DEBUG
        [CommandLine("--test", CommandType.ARGUMENTS, ArgumentsCount = 1, Help = "For test.", Info = "use --test <What test for>")]
        public string[] Test;
#endif

        [CommandLine("net", CommandType.OPTION, Help = "Multi-commands net.", Info = "use net <Others>")]
        public bool Net;
    }

    public class Runnable
    {
        public static void Start(string[] arguments)
        {
            var option = CommandLineParser<Options>.Parse(arguments);

            //
            //  Multi Commands
            //
            if (option.Net)
            {
                NetConsole.Start(arguments.Skip(1).ToArray());
            }
            //
            //  Single Commands
            //
            else if (option.Error)
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
            else if (option.Version)
            {
                PrintVersion();
            }
            else if (option.DialogMode)
            {
                Dialog.StartDialog();
            }
#if DEBUG
            //
            //  Test
            //
            else if (option.Test != null)
            {
                ProcessTest(option.Test);
            }
#endif

            return;
        }

        static void PrintHelp()
        {
            PrintVersion();
            System.Console.WriteLine($"\r\n");

            var builder = new StringBuilder();
            CommandLineParser<Options>.GetFields().ToList().ForEach(
                x =>
                {
                    if (!string.IsNullOrEmpty(x.Value.Item2.Help))
                        builder.Append($" {x.Key} : {x.Value.Item2.Help} [{x.Value.Item2.Info}]\r\n");
                    else
                        builder.Append($" {x.Key} [{x.Value.Item2.Info}]\r\n");
                });
            System.Console.WriteLine(builder.ToString());
        }

        public static void PrintVersion()
        {
            System.Console.WriteLine($"{Version.Name} {Version.Text}");
        }

#if DEBUG
        static void ProcessTest(string[] args)
        {
            switch (args[0])
            {
                case "rsa":
                    {
                        var vpkp = RSAHelper.CreateKey();

                        System.Console.WriteLine(vpkp.Item1);
                        System.Console.WriteLine(vpkp.Item2);

                        var rsa_test_text = "ABCD";
                        var bb = Encoding.UTF8.GetBytes(rsa_test_text);

                        var enc = RSAHelper.Encrypt(bb, vpkp.Item2);
                        var dec = RSAHelper.Decrypt(enc, vpkp.Item1);

                        System.Console.WriteLine(Encoding.UTF8.GetString(dec));
                    }
                    break;

                case "dcinside":
                    {
                        Framework.Extractor.DCInsideExtractor extractor = new Framework.Extractor.DCInsideExtractor();
                        extractor.Extract("https://gall.dcinside.com/board/view/?id=superidea&no=194789");
                    }
                    break;
            }
        }
#endif
    }

}
