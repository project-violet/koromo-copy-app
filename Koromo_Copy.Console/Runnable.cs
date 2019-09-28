// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Console.Component;
using Koromo_Copy.Framework;
using Koromo_Copy.Framework.CL;
using Koromo_Copy.Framework.Crypto;
using Koromo_Copy.Framework.Extractor;
using Koromo_Copy.Framework.Network;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Koromo_Copy.Console
{
    public class Options : IConsoleOption
    {
        [CommandLine("--help", CommandType.OPTION, Default = true)]
        public bool Help;
        [CommandLine("-v", CommandType.OPTION, Default = true, Info = "Show version information.")]
        public bool Version;
        [CommandLine("--dialog-mode", CommandType.OPTION, Info = "Run program with dialog mode.")]
        public bool DialogMode;

#if DEBUG
        [CommandLine("--test", CommandType.ARGUMENTS, ArgumentsCount = 1, Info = "For test.", Help = "use --test <What test for>")]
        public string[] Test;
#endif

        [CommandLine("net", CommandType.OPTION, Info = "Multi-commands net.", Help = "use net <Others>")]
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
            System.Console.WriteLine($"Copyright (C) 2019. Koromo Copy Developer");
            System.Console.WriteLine($"E-Mail: koromo.software@gmail.com");
            System.Console.WriteLine($"Source-code: https://github.com/dc-koromo/koromo-copy2");
            System.Console.WriteLine($"");

            var builder = new StringBuilder();
            CommandLineParser<Options>.GetFields().ToList().ForEach(
                x =>
                {
                    if (!string.IsNullOrEmpty(x.Value.Item2.Info))
                        builder.Append($" {x.Key} : {x.Value.Item2.Info} [{x.Value.Item2.Help}]\r\n");
                    else
                        builder.Append($" {x.Key} [{x.Value.Item2.Help}]\r\n");
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
                        var imgs = DCInsideExtractor.Extract("https://gall.dcinside.com/mgallery/board/view?id=plamodels&no=22155", null).Item1;
                        int count = imgs.Count;
                        imgs.ForEach(x => {
                            x.Filename = Path.Combine(Directory.GetCurrentDirectory(), x.Filename);
                            x.CompleteCallback = () =>
                            {
                                Interlocked.Decrement(ref count);
                            };
                        });
                        imgs.ForEach(x => AppProvider.Scheduler.Add(x));

                        while (count != 0)
                        {
                            Thread.Sleep(500);
                        }
                    }
                    break;

                case "pixiv":
                    {
                        var ext = PixivExtractor.Extract("https://www.pixiv.net/member.php?id=4462");
                        var imgs = ext.Item1;
                        var uinfo = $"{(ext.Item2 as List<PixivExtractor.PixivAPI.User>)[0].Name} ({(ext.Item2 as List<PixivExtractor.PixivAPI.User>)[0].Account})";
                        int count = imgs.Count;
                        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), uinfo));
                        imgs.ForEach(x => {
                            x.Filename = Path.Combine(Directory.GetCurrentDirectory(), uinfo, x.Filename);
                            x.CompleteCallback = () =>
                            {
                                Interlocked.Decrement(ref count);
                            };
                        });
                        imgs.ForEach(x => AppProvider.Scheduler.Add(x));

                        while (count != 0)
                        {
                            Thread.Sleep(500);
                        }
                    }
                    break;

                case "gelbooru":
                    {
                        var ext = GelbooruExtractor.Extract("https://gelbooru.com/index.php?page=post&s=list&tags=kokkoro_%28princess_connect%21%29");
                        var imgs = ext.Item1;
                        int count = imgs.Count;
                        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), ext.Item2 as string));
                        imgs.ForEach(x => {
                            x.Filename = Path.Combine(Directory.GetCurrentDirectory(), ext.Item2 as string, x.Filename);
                            x.CompleteCallback = () =>
                            {
                                Interlocked.Decrement(ref count);
                            };
                        });
                        imgs.ForEach(x => AppProvider.Scheduler.Add(x));

                        while (count != 0)
                        {
                            Thread.Sleep(500);
                        }
                    }
                    break;

                case "naver":
                    {
                        var ext = NaverExtractor.Extract("https://comic.naver.com/webtoon/detail.nhn?titleId=318995&no=434&weekday=fri");
                        var imgs = ext.Item1;
                        int count = imgs.Count;
                        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), (ext.Item2 as NaverExtractor.ComicInformation).Title));
                        imgs.ForEach(x => {
                            x.Filename = Path.Combine(Directory.GetCurrentDirectory(), (ext.Item2 as NaverExtractor.ComicInformation).Title, x.Filename);
                            x.CompleteCallback = () =>
                            {
                                Interlocked.Decrement(ref count);
                            };
                        });
                        imgs.ForEach(x => AppProvider.Scheduler.Add(x));

                        while (count != 0)
                        {
                            Thread.Sleep(500);
                        }
                    }
                    break;

                case "eh":
                    {
                        var ext = EHentaiExtractor.Extract("https://e-hentai.org/g/1491793/45f9e85e48/");
                        var imgs = ext.Item1;
                        int count = imgs.Count;
                        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), (ext.Item2 as EHentaiArticle).Title));
                        imgs.ForEach(x =>
                        {
                            x.Filename = Path.Combine(Directory.GetCurrentDirectory(), (ext.Item2 as EHentaiArticle).Title, x.Filename);
                            x.CompleteCallback = () =>
                            {
                                Interlocked.Decrement(ref count);
                            };
                        });
                        imgs.ForEach(x => AppProvider.Scheduler.Add(x));

                        while (count != 0)
                        {
                            Thread.Sleep(500);
                        }
                    }
                    break;
            }
        }
#endif
    }

}
