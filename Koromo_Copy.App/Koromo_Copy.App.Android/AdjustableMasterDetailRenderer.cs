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
using Android.Views;
using Android.Widget;
using Koromo_Copy.App;
using Koromo_Copy.App.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppCompat;

[assembly: Xamarin.Forms.ExportRenderer(typeof(MasterDetailPage), typeof(AdjustableMasterDetailRenderer))]
namespace Koromo_Copy.App.Droid
{
    public class AdjustableMasterDetailRenderer : MasterDetailPageRenderer
    {
        bool firstDone;

        public AdjustableMasterDetailRenderer(Context context) : base(context) { }

        public override void AddView(Android.Views.View child)
        {
            if (firstDone)
            {
                var p = (LayoutParams)child.LayoutParameters;
                p.Width = 600;
                base.AddView(child, p);
            }
            else
            {
                firstDone = true;
                base.AddView(child);
            }
        }
    }
}