// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Text;

namespace Koromo_Copy.Framework
{
    public class Version
    {
        public const int MajorVersion = 0;
        public const int MinorVersion = 1;
        public const int BuildVersion = 0;

        public const string Name = "Koromo Copy";
        public static string Text { get; } = $"{MajorVersion}.{MinorVersion}.{BuildVersion}";
    }
}
