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

            //var url = "https://manamoa13.net/bbs/page.php?hid=manga_detail&manga_id=10953";
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
                List.Children.Add(elem);
            }
        }
    }
}