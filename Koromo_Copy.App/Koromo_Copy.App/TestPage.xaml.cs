// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Koromo_Copy.App
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestPage : ContentPage
    {
        public TestPage()
        {
            InitializeComponent();

            Web.Navigated += Web_Navigated;
            Web.Source = "https://manamoa16.net/bbs/board.php?bo_table=manga&wr_id=2074046";
        }

        private async void Web_Navigated(object sender, Cookies.CookieNavigatedEventArgs args)
        {
            var x = await Web.EvaluateJavaScriptAsync("document.cookie");
            Externals.ManamoaPHPSESSID = Regex.Match(x, "PHPSESSID=(.*?);").Groups[1].Value;
        }
    }
}