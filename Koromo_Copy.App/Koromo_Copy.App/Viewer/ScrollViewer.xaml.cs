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
        class imgs
        {
            public string Path { get; set; }
        }

        IList<imgs> Images { get; set; }

        public ScrollViewer(string shortinfo, string directory)
        {
            InitializeComponent();

            Title = shortinfo;

            Images = new List<imgs>();
            List.ItemsSource = Images;

            var comp = new Strings.NaturalComparer();
            var files = Directory.GetFiles(directory).ToList();
            files.Sort((x, y) => comp.Compare(x, y));

            foreach (var file in files)
            {
                Images.Add(new imgs { Path = file });
            }
        }

        private void CachedImage_Success(object sender, CachedImageEvents.SuccessEventArgs e)
        {
            var h = e.ImageInformation.OriginalHeight;
            var w = e.ImageInformation.OriginalWidth;

            var view = sender as CachedImage;

            if (h < 200 && w < 200)
            {
                view.HeightRequest = h;
                view.WidthRequest = w;
            }
            else
            {
                view.HeightRequest = (List.Width - 8) * h / w;
                view.WidthRequest = List.Width - 8;
            }
        }
    }
}