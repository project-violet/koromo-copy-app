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
}
