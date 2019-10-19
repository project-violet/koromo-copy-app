// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.App.DataBase;
using Koromo_Copy.Framework.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Koromo_Copy.App.Viewer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SubdirSelector : ContentPage
    {
        public IList<string> Titles { get; private set; }
        string superpath;
        public SubdirSelector(DownloadDBModel dbm)
        {
            InitializeComponent();

            Titles = new List<string>();

            var comp = new Strings.NaturalComparer();
            var folders = Directory.GetDirectories(dbm.Directory).ToList();
            Title = dbm.ShortInfo;
            superpath = dbm.Directory;
            folders.Sort((x, y) => comp.Compare(x, y));
            folders.ForEach(x => Titles.Add(Path.GetFileName(x)));

            BindingContext = this;
        }

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var title = e.Item as string;
            await (Application.Current.MainPage as MainPage).NaviInstance.PushAsync(new ScrollViewer(title, Path.Combine(superpath, title)));
        }
    }
}