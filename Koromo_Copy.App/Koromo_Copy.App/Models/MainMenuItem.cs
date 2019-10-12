using System;
using System.Collections.Generic;
using System.Text;

namespace Koromo_Copy.App.Models
{
    public enum MenuItemType
    {
        Browse,
        About,
        Settings
    }

    public class MainMenuItem
    {
        public MenuItemType Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
    }
}
