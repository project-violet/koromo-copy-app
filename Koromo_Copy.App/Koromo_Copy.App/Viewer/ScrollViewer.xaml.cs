// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using FFImageLoading.Forms;
using Koromo_Copy.App.DataBase;
using Koromo_Copy.Framework.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Koromo_Copy.App.Viewer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScrollViewer : ContentPage
    {
        public ScrollViewer(DownloadDBModel dbm)
        {
            InitializeComponent();

            Title = dbm.ShortInfo;

            Task.Run(() =>
            {
                Thread.Sleep(100);
                int cnt = 0;

                var comp = new Strings.NaturalComparer();
                var files = Directory.GetFiles(dbm.Directory).ToList();
                files.Sort((x, y) => comp.Compare(x, y));

                foreach (var file in files)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        var image = new CachedImage();
                        ViewStack.Children.Add(image);
                        image.Margin = new Thickness(4, 4, 4, 4);
                        image.DownsampleToViewSize = true;
                        image.Success += (s, e) =>
                        {
                            var h = e.ImageInformation.OriginalHeight;
                            var w = e.ImageInformation.OriginalWidth;

                            if (Width > w)
                            {
                                image.HeightRequest = h;
                                image.WidthRequest = w;
                            }
                            else
                            {
                                image.WidthRequest = Width;
                                image.HeightRequest = Width * h / w;
                            }
                        };
                        image.IsVisible = true;
                        image.Source = file;
                    });
                    Thread.Sleep(500);

                    if (++cnt >= 20)
                        break;
                }
            });
        }
    }
}