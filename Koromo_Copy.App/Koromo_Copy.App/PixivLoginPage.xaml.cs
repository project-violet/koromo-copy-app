// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Extractor;
using Koromo_Copy.Framework.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Koromo_Copy.App
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PixivLoginPage : ContentPage
    {
        public PixivLoginPage()
        {
            InitializeComponent();

            Id.Text = Settings.Instance.Model.PixivSettings.Id ?? "";
            Password.Text = Settings.Instance.Model.PixivSettings.Password ?? "";
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            PixivExtractor.PixivAPI.AccessToken = null;
            if (!PixivExtractor.PixivAPI.Auth(Id.Text, Password.Text))
            {
                Fail.IsVisible = true;
                return;
            }

            Settings.Instance.Model.PixivSettings.Id = Id.Text;
            Settings.Instance.Model.PixivSettings.Password = Password.Text;
            Settings.Instance.Save();

            Plugin.XSnack.CrossXSnack.Current.ShowMessage("픽시브 로그인을 성공했습니다!\n 이제 픽시브 다운로더를 이용할 수 있습니다.");
            Navigation.RemovePage(this);
        }
    }
}