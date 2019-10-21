// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Koromo_Copy.Framework.Component.Hitomi
{
    public struct HitomiTagdata
    {
        public string Tag { get; set; }
        public int Count { get; set; }
    }

    public struct HitomiTagdataCollection
    {
        public List<HitomiTagdata> language { get; set; }
        public List<HitomiTagdata> female { get; set; }
        public List<HitomiTagdata> series { get; set; }
        public List<HitomiTagdata> character { get; set; }
        public List<HitomiTagdata> artist { get; set; }
        public List<HitomiTagdata> group { get; set; }
        public List<HitomiTagdata> tag { get; set; }
        public List<HitomiTagdata> male { get; set; }
        public List<HitomiTagdata> type { get; set; }
    }
}
