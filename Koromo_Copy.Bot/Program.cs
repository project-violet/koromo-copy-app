// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework;
using Koromo_Copy.Framework.Log;
using System;
using System.Globalization;

namespace Koromo_Copy.Bot
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            //args = new string[] { "--telegram", "--api-key", "1064203680:AAGB9o2QGQLljkaBYC36Wxx3sVEb8MRO9aU" };
#endif

            AppProvider.Initialize();

            Logs.Instance.AddLogErrorNotify((s, e) => {
                var tuple = s as Tuple<DateTime, string, bool>;
                CultureInfo en = new CultureInfo("en-US");
                Console.Error.WriteLine($"[{tuple.Item1.ToString(en)}] [Error] {tuple.Item2}");
            });

            try
            {
                Runnable.Start(args);
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occured! " + e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Please, check log.txt file.");
            }

            AppProvider.Deinitialize();

            Environment.Exit(0);
        }
    }
}
