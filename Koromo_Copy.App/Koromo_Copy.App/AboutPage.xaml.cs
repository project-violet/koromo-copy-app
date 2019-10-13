// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xam.Forms.Markdown;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Koromo_Copy.App
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();

            Image.Source = "https://dcimg4.dcinside.co.kr/viewimage.php?id=3da8c22feedd36a351add3b11fc721&no=24b0d769e1d32ca73ded87fa11d02831b24d3c2d27291c406c42f02d19543e4910e0b1cccc006ba62bb47af266c3f1e34dd7d851e8e82408c8e9f76da0e568733af602750a97";

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => {
                Device.OpenUri(new Uri("mailto:koromo.software@gmail.com"));
            };
            Mail.GestureRecognizers.Add(tapGestureRecognizer);

            var view = new MarkdownView();
            view.Markdown = @"
# About Koromo Copy

`Koromo Copy`는 모두를 위한 강력한 다운로더입니다.

Github: [https://github.com/dc-koromo/koromo-copy](https://github.com/dc-koromo/koromo-copy)

# Developers

### dc-koromo

`Hitomi Copy` 및 데스크탑용 `Koromo Copy`를 제작했던 개발자다.
현재는 모든 플랫폼을 지원하는 것을 목표로 새로운 `Koromo Copy`를 개발하고있다.

# Contributors

# Licenses

### Koromo Copy Core Library

#### NETStandard.Library

 * [https://github.com/dotnet/standard](https://github.com/dotnet/standard)
 * Licensed under the MIT Licence.

#### HtmlAgilityPack

 * [https://github.com/zzzprojects/html-agility-pack](https://github.com/zzzprojects/html-agility-pack)
 * Licensed under the MIT Licence.

#### MessagePack

 * [https://github.com/neuecc/MessagePack-CSharp/](https://github.com/neuecc/MessagePack-CSharp/)
 * Licensed under the MIT Licence.

#### Newtonsoft.Json

 * [https://www.newtonsoft.com/json](https://www.newtonsoft.com/json)
 * Licensed under the MIT Licence.

#### SixLabors.ImageSharp

 * [https://github.com/SixLabors/ImageSharp/](https://github.com/SixLabors/ImageSharp/)
 * Licensed under the Apache License 2.0

#### Pixeez

 * [https://github.com/cucmberium/Pixeez](https://github.com/cucmberium/Pixeez)
 * Licensed under the MIT Licence.

### Koromo Copy Mobile App

#### Xamarin.Essentials

 * [https://github.com/xamarin/Essentials](https://github.com/xamarin/Essentials)
 * Licensed under the MIT Licence.

#### Xamarin.Forms, Xamarin.Forms.Visual.Material

 * [https://github.com/xamarin/Xamarin.Forms](https://github.com/xamarin/Xamarin.Forms)
 * Licensed under the MIT Licence.

#### Acr.UserDialogs

 * [https://github.com/aritchie/userdialogs](https://github.com/aritchie/userdialogs)
 * Licensed under the MIT Licence.

#### Plugin.XSnack

 * [https://github.com/ice-j/Plugin.XSnack/](https://github.com/ice-j/Plugin.XSnack/)
 * Licensed under the MIT Licence.

#### Xam.Forms.MarkdownView

 * [https://github.com/dotnet-ad/MarkdownView](https://github.com/dotnet-ad/MarkdownView)
 * Licensed under the MIT Licence.

#### Xamarin.Plugin.FilePicker

 * [https://github.com/jfversluis/FilePicker-Plugin-for-Xamarin-and-Windows](https://github.com/jfversluis/FilePicker-Plugin-for-Xamarin-and-Windows)
 * Licensed under the MIT Licence.

#### XamarinForms.CardView

 * [https://github.com/tiger4589/Xamarin.Forms-CardView](https://github.com/tiger4589/Xamarin.Forms-CardView)
 * Licensed under the MIT Licence.

#### XamEffects

 * [https://github.com/mrxten/XamEffects](https://github.com/mrxten/XamEffects)
 * Licensed under the MIT Licence.

#### FFImageLoading

 * [https://github.com/luberda-molinet/FFImageLoading](https://github.com/luberda-molinet/FFImageLoading)
 * Licensed under the MIT Licence.

### Reference Library

#### youtube-dl

 * [https://github.com/ytdl-org/youtube-dl](https://github.com/ytdl-org/youtube-dl)
 * Licensed under the Unlicence.

#### pixivpy

 * [https://github.com/upbit/pixivpy](https://github.com/upbit/pixivpy)
 * Licensed under the Unlicence.

#### FFmpeg

 * [https://www.ffmpeg.org/](https://www.ffmpeg.org/)
 * Licensed under the GNU Lesser Gereneal Public License 2.1

";
            License.Content = view;
        }

        public ICommand ClickCommand => new Command<string>((url) =>
        {
            Device.OpenUri(new System.Uri(url));
        });
    }
}