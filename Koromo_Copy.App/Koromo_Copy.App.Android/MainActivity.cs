using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.IO;
using Koromo_Copy.Framework;
using Acr.UserDialogs;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android;
using Plugin.CurrentActivity;
using Java.Lang;
using FFImageLoading.Forms.Platform;
using Android.Content;

namespace Koromo_Copy.App.Droid
{
    [Activity(Label = "Koromo Copy", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) == (int)Permission.Granted)
            {
                // We have permission, go ahead and use the camera.
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new System.String[] { Manifest.Permission.ReadExternalStorage }, 1);
            }
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) == (int)Permission.Granted)
            {
                // We have permission, go ahead and use the camera.
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new System.String[] { Manifest.Permission.WriteExternalStorage }, 1);
            }

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            CommonAPI.Instance = new DroidCommonAPI();

            base.OnCreate(savedInstanceState);

            AppProvider.DefaultSuperPath = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath, "KoromoCopy");
            
            UserDialogs.Init(this);
            CrossCurrentActivity.Current.Init(Application);
            CachedImageRenderer.Init(true);
            XamEffects.Droid.Effects.Init();
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            Xamarin.Forms.Forms.SetFlags("UseLegacyRenderers");
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            global::Xamarin.Forms.FormsMaterial.Init(this, savedInstanceState);
            LoadApplication(new App());

            //YoutubeDL.test_run();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        bool doubleBackToExitPressedOnce = false;

        public override void OnBackPressed()
        {
            if (MainPage.Instance.NaviInstance.CurrentPage == MainPage.Instance.NaviInstance.RootPage)
            {
                if (doubleBackToExitPressedOnce)
                {
                    base.OnBackPressed();
                    return;
                }
                this.doubleBackToExitPressedOnce = true;
                Toast.MakeText(this, "한 번더 누르면 종료합니다", ToastLength.Short).Show();
                new Handler().PostDelayed(() =>
                {
                    doubleBackToExitPressedOnce = false;
                }, 2000);
            }
            else
                base.OnBackPressed();
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            FFImageLoading.ImageService.Instance.InvalidateMemoryCache();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            base.OnTrimMemory(level);
        }

        public override void OnLowMemory()
        {
            FFImageLoading.ImageService.Instance.InvalidateMemoryCache();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            base.OnLowMemory();
        }
    }
}