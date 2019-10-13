// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Acr.UserDialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

            //var url = "https://gall.dcinside.com/mgallery/board/view/?id=purikone_redive&no=1651065&exception_mode=recommend&page=1";
            //var elem = new DownloadElement(url, false);
            //List.Children.Add(elem);
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
                var elem = new DownloadElement( result.Text, false);
                List.Children.Insert(0, elem);
            }
        }
    }
}