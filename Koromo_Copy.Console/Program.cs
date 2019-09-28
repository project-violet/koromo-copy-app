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
            args = new string[] { "--test", "naver" };
#endif
            AppProvider.Initialize();

            Logs.Instance.AddLogErrorNotify((s, e) => {
                lock (Logs.Instance.LogError)
                {
                    CultureInfo en = new CultureInfo("en-US");
                    System.Console.Error.WriteLine($"[{Logs.Instance.LogError.Last().Item1.ToString(en)}] [Error] {Logs.Instance.LogError.Last().Item2}");
                }
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
