// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Koromo_Copy.Framework.Network;

namespace Koromo_Copy.Framework.Extractor
{
    public class DCInsideExtractorOption : ExtractorOption
    {

    }

    public class DCInsideExtractor : ExtractorModel
    {
        public new static string ValidUrl()
            => @"https?://(?:gall|m)\.dcinside\.com/(?:mgallery/)?board/(?:lists|view/)\?(?:.*?)";

        public override List<NetTask> Extract(string url)
        {
            var regex = new Regex(ValidUrl());
            var match = regex.Matches(url);

            var result = new List<NetTask>();

            var nt = NetTask.MakeDefault(url);
            nt.DownloadBufferSize = 10241024;
            nt.TimeoutMillisecond = 3000;
            var html = NetTools.DownloadStringAsync(nt).Result;

            if (match[1].Value == "gall")
            {

            }

            return result;
        }
    }
}
