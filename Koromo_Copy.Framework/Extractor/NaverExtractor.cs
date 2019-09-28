// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Network;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Koromo_Copy.Framework.Extractor
{
    public class NaverExtractorOption : IExtractorOption
    {
        public enum ExtratorType
        {
            Images = 0, // Default
        }

    }

    public class NaverExtractor : ExtractorModel<NaverExtractorOption>
    {
        static NaverExtractor()
            => ValidUrl = new Regex(@"^https?://(comic|blog)\.naver\.com/(webtoon|.*?)/((list|detail)\.nhn\?|.*)\??(titleId\=(\d+)\&no=(\d+))?(.*?)$");

        public override Tuple<List<NetTask>, object> Extract(string url, NaverExtractorOption option = null)
        {
            var match = ValidUrl.Match(url).Groups;

            return null;
        }
    }
}
