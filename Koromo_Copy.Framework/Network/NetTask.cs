// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Koromo_Copy.Framework.Network
{
    /// <summary>
    /// Information of what download for
    /// </summary>
    public class NetTask
    {
        public static NetTask MakeDefault()
            => new NetTask
            {
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36"
            };

        public static NetTask MakeDefaultMobile()
            => new NetTask
            {
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
                UserAgent = "Mozilla/5.0 (Android 7.0; Mobile; rv:54.0) Gecko/54.0 Firefox/54.0 AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.125 Mobile Safari/603.2.4"
            };

        public string Url { get; set; }
        public string Filename { get; set; }
        public string Accept { get; set; }
        public string Referer { get; set; }
        public string UserAgent { get; set; }
        public string Cookie { get; set; }
    }
}
