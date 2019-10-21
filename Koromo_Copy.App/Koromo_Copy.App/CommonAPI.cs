// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Text;

namespace Koromo_Copy.App
{
    public abstract class CommonAPI
    {
        public static CommonAPI Instance { get; set; }
        public abstract void OpenUri(string uri, string type = "");
    }

    public class DeviceInfo
    {
        protected static DeviceInfo _instance;
        double width;
        double height;

        static DeviceInfo()
        {
            _instance = new DeviceInfo();
        }
        protected DeviceInfo()
        {
        }

        public static bool IsOrientationPortrait()
        {
            return _instance.height > _instance.width;
        }

        public static void SetSize(double width, double height)
        {
            _instance.width = width;
            _instance.height = height;
        }
    }
}
