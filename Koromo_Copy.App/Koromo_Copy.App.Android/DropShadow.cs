using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Koromo_Copy.App;
using Koromo_Copy.App.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ShadowFrame), typeof(ShadowFrameRenderer))]
namespace Koromo_Copy.App.Droid
{
    public class ShadowFrameRenderer : FrameRenderer
    {
        public ShadowFrameRenderer(Context context) : base(context)
        {
            //AutoPackage = false;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);
            var element = e.NewElement;

            if (e.NewElement != null)
            {
            }
        }
    }
}