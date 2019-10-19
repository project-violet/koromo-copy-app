// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Network;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static Koromo_Copy.Framework.Extractor.IExtractorOption;

namespace Koromo_Copy.Framework.Extractor
{
    public class ImgurExtractorOption : IExtractorOption
    {
    }

    public class ImgurExtractor : ExtractorModel
    {
        public ImgurExtractor()
        {
            HostName = new Regex(@"imgur\.com");
            ValidUrl = new Regex(@"^https?://imgur\.com/gallery/(?<code>.*?)/?$");
        }

        public override IExtractorOption RecommendOption(string url)
        {
            return new EHentaiExtractorOption { Type = ExtractorType.Images };
        }

        public override string RecommendFormat(IExtractorOption option)
        {
            return "%(extractor)s/%(id)s/%(file)s.%(ext)s";
        }

        public override (List<NetTask>, ExtractedInfo) Extract(string url, IExtractorOption option = null)
        {
            var match = ValidUrl.Match(url).Groups;

            if (option == null)
                option = RecommendOption(url);

            var html = NetTools.DownloadString(url);
            var result = new List<NetTask>();

            foreach (var img in JObject.Parse((new Regex("item: ({.*})")).Match(html).Groups[1].Value)["album_images"]["images"])
            {
                var hash = img["hash"].ToString();
                var ext = img["ext"].ToString();
                var task = NetTask.MakeDefault($"https://i.imgur.com/{hash}{ext}");
                task.SaveFile = true;
                task.Filename = $"{hash}{ext}";
                task.Format = new ExtractorFileNameFormat { Id = hash, Extension = ext, FilenameWithoutExtension = hash, Url = url };
                result.Add(task);
            }

            result.ForEach(task => task.Format.Extractor = GetType().Name.Replace("Extractor", ""));
            return (result, new ExtractedInfo { Type = ExtractedInfo.ExtractedType.Search });
        }
    }
}
