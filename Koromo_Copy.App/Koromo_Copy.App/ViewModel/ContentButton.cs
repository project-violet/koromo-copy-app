using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace Koromo_Copy.App.ViewModel
{
    public partial class ContentButton : ContentView
    {
        public ContentButton()
            : base()
        {
        }

        public event EventHandler Tapped;

        [Obsolete]
        public static readonly BindableProperty CommandProperty = BindableProperty.Create<ContentButton, ICommand>(c => c.Command, null);

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            if (Tapped != null)
                Tapped(this, new EventArgs());
        }
    }
}
