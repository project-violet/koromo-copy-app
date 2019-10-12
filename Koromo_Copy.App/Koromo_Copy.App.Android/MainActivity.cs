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
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.ReadExternalStorage }, 1);
            }
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) == (int)Permission.Granted)
            {
                // We have permission, go ahead and use the camera.
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.WriteExternalStorage }, 1);
            }

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            AppProvider.DefaultSuperPath = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath, "KoromoCopy");

            UserDialogs.Init(this);
            CrossCurrentActivity.Current.Init(Application);
            XamEffects.Droid.Effects.Init();
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            global::Xamarin.Forms.FormsMaterial.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}