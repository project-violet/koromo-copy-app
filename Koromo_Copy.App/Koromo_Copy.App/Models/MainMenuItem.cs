// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Text;

namespace Koromo_Copy.App.Models
{
    public enum MenuItemType
    {
        Browse,
        About,
        Settings,
        Log,
        Test,
    }

    public class MainMenuItem
    {
        public MenuItemType Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
    }
}
