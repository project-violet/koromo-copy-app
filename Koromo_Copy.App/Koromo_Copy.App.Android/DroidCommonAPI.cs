// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Java.IO;
using Xamarin.Forms;

namespace Koromo_Copy.App.Droid
{
    public class DroidCommonAPI : CommonAPI
    {
        public override void OpenUri(string suri, string type = "*/*")
        {
            Intent intent = new Intent(Intent.ActionView);
            Android.Net.Uri uri = GetURI_FileProvider(new File(suri));
            if (uri != null)
            {
                intent.SetFlags(Android.Content.ActivityFlags.GrantReadUriPermission);
            }
            else
            {
                uri = GetUriSimple(new File(suri));
            }

            intent.SetDataAndType(uri, type);
            intent.SetFlags(ActivityFlags.ClearWhenTaskReset | ActivityFlags.NewTask);

            Intent intentChooser = Intent.CreateChooser(intent, "파일 열기");
            Forms.Context.StartActivity(intentChooser);
        }

        private Android.Net.Uri GetURI_FileProvider(File file)
        {
            Android.Net.Uri uri = null;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop || uri == null) //api >= 21
            {
                try
                {
                    uri = Android.Support.V4.Content.FileProvider.GetUriForFile(Android.App.Application.Context, "com.koromo_project.koromo_copy.fileprovider", file);
                }
                catch (Exception ex)
                {
                    //trace
                }
            }

            return uri;
        }

        private static Android.Net.Uri GetUriSimple(File file)
        {
            Android.Net.Uri uri = null;

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop) //api < 21
            {
                try
                {
                    // work with api 22
                    uri = Android.Net.Uri.FromFile(file);
                }
                catch (Exception ex)
                {
                    //trace
                }
            }

            return uri;
        }
    }
}