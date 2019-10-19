// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Acr.UserDialogs;
using Koromo_Copy.App.DataBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Koromo_Copy.App
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DownloadPage : ContentPage
    {
        public DownloadPage()
        {
            InitializeComponent();

            Appearing += DownloadPage_Appearing;
        }

        bool loaded = false;
        private void DownloadPage_Appearing(object sender, EventArgs e)
        {
            if (loaded) return;
            loaded = true;
            Task.Run(() =>
            {
                foreach (var dbm in DownloadDBManager.Instance.QueryAll())
                {
                    var elem = new DownloadElement(dbm);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        List.Children.Insert(0, elem);
                    });
                    Thread.Sleep(200);
                }
            });
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            var promptConfig = new PromptConfig();
            promptConfig.InputType = InputType.Url;
            promptConfig.IsCancellable = true;
            promptConfig.Message = "URL을 입력해주세요.";
            var result = await UserDialogs.Instance.PromptAsync(promptConfig);
            if (result.Ok)
            {
                var elem = new DownloadElement(result.Text);
                List.Children.Insert(0, elem);
            }
        }
    }
}