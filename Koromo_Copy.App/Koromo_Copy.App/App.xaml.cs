// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework;
using Koromo_Copy.Framework.Log;
using System;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Koromo_Copy.App
{
    public partial class App : Application
    {
        public static StringBuilder log_recored = new StringBuilder();
        public App()
        {
            Logs.Instance.AddLogNotify((s, e) =>
            {
                var ss = s as Tuple<DateTime, string, bool>;
                var en = new CultureInfo("en-US");
                lock (log_recored)
                    log_recored.Append($"[{ss.Item1.ToString(en)}] {ss.Item2}\r\n");
            });
            Logs.Instance.AddLogErrorNotify((s, e) =>
            {
                var ss = s as Tuple<DateTime, string, bool>;
                var en = new CultureInfo("en-US");
                lock (log_recored)
                    log_recored.Append($"[{ss.Item1.ToString(en)}] [Error] {ss.Item2}\r\n");
            });

            AppProvider.ApplicationPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            AppProvider.Initialize();

            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            AppProvider.Deinitialize();
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
