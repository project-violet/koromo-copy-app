using Koromo_Copy.Framework;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Koromo_Copy.App
{
    public partial class App : Application
    {
        public App()
        {
            AppProvider.ApplicationPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            AppProvider.Initialize();

            InitializeComponent();

            MainPage = new MainPage();

        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            AppProvider.Deinitialize();
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
