// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.App.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Koromo_Copy.App
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : MasterDetailPage
    {
        Dictionary<int, NavigationPage> MenuPages = new Dictionary<int, NavigationPage>();
        public MainPage()
        {
            InitializeComponent();

            Instance = this;
            MenuPages.Add((int)MenuItemType.Downloader, (NavigationPage)Detail);
        }

        public static MainPage Instance { get; private set; }
        public NavigationPage NaviInstance { get { return Navi; } }

        public async Task NavigateFromMenu(int id)
        {
            if (!MenuPages.ContainsKey(id))
            {
                switch (id)
                {
                    //case (int)MenuItemType.Browse:
                    //    MenuPages.Add(id, new NavigationPage(new ItemsPage()));
                    //    break;
                    case (int)MenuItemType.Search:
                        MenuPages.Add(id, new NavigationPage(new SearchPage()));
                        break;
                    case (int)MenuItemType.Settings:
                        MenuPages.Add(id, new NavigationPage(new SettingsPage()));
                        break;
                    case (int)MenuItemType.About:
                        MenuPages.Add(id, new NavigationPage(new AboutPage()));
                        break;
                    case (int)MenuItemType.Test:
                        MenuPages.Add(id, new NavigationPage(new TestPage()));
                        break;
                    case (int)MenuItemType.Log:
                        MenuPages.Add(id, new NavigationPage(new LogPage()));
                        break;

                }
            }

            var newPage = MenuPages[id];

            if (newPage != null && Detail != newPage)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Detail = newPage;
                });

                if (Device.RuntimePlatform == Device.Android)
                    await Task.Delay(100);

                Device.BeginInvokeOnMainThread(() =>
                {
                    IsPresented = false;
                });
            }
        }


        //protected override void OnSizeAllocated(double width, double height)
        //{
        //    base.OnSizeAllocated(width, height);

        //    if (DeviceInfo.IsOrientationPortrait() && width > height || !DeviceInfo.IsOrientationPortrait() && width < height)
        //    {
        //        MainMenu.SizeChange();
        //    }
        //}
    }
}
