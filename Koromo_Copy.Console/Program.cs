// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework;
using Koromo_Copy.Framework.CL;
using Koromo_Copy.Framework.Log;
using System;
using System.Globalization;
using System.Linq;

namespace Koromo_Copy.Console
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            //args = new string[] { "net", "--download-html", "https://naver.com", /*"--disable-auto-redirection"*/ };
            //args = new string[] { "--test", "pixiv" };
            //args = new string[] { "https://e-hentai.org/g/1491793/45f9e85e48/", "--extract-info", "--print-process" };
            //args = new string[] { "https://e-hentai.org/g/1491793/45f9e85e48/",  };
            //args = new string[] { "https://gall.dcinside.com/board/view/?id=programming&no=1132197&page=1", "--print-process" };
            //args = new string[] { "https://hitomi.la/galleries/1344592.html", "--print-process" };
            //args = new string[] { "https://gall.dcinside.com/mgallery/board/lists?id=purikone_redive", "--gaenyum" };
            args = new string[] { "https://www.instagram.com/parlovetati/" };
            args = new string[] { "https://www.instagram.com/zennyrt/", "-p" };
            args = new string[] { "https://www.instagram.com/haneul__haneul/", "-p" };
#endif
            AppProvider.Initialize();

            Logs.Instance.AddLogErrorNotify((s, e) => {
                var tuple = s as Tuple<DateTime, string, bool>;
                CultureInfo en = new CultureInfo("en-US");
                System.Console.Error.WriteLine($"[{tuple.Item1.ToString(en)}] [Error] {tuple.Item2}");
            });

            try
            {
                Runnable.Start(args);
            }
            catch (Exception e)
            {
                System.Console.WriteLine("An error occured! " + e.Message);
                System.Console.WriteLine(e.StackTrace);
                System.Console.WriteLine("Please, check log.txt file.");
            }

            AppProvider.Deinitialize();

            Environment.Exit(0);
        }
    }
}
