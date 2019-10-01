// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Network;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Koromo_Copy.Framework.Extractor
{
    public class HitomiArticle : EHentaiArticle
    {
    }

    public class HitomiExtractorOption : IExtractorOption
    {
    }

    public class HitomiExtractor : ExtractorModel
    {
        public HitomiExtractor()
        {
            HostName = new Regex(@"hitomi\.la");
            ValidUrl = new Regex(@"^https?://hitomi\.la/(?:galleries|reader)/(?<id>\d+).*?$");
        }

        public override IExtractorOption RecommendOption(string url)
        {
            return new HitomiExtractorOption { Type = HitomiExtractorOption.ExtractorType.Images };
        }

        public override Tuple<List<NetTask>, object> Extract(string url, IExtractorOption option = null)
        {
            var match = ValidUrl.Match(url).Groups;

            if (option == null)
                option = RecommendOption(url);

            if ((option as HitomiExtractorOption).Type == HitomiExtractorOption.ExtractorType.Images)
            {
                var imgs = $"https://ltn.hitomi.la/galleries/{match["id"].Value}.js";

                // download.js
                var number_of_frontends = 2;
                var subdomain = Convert.ToChar(97 + (Convert.ToInt32(match["id"].Value.Last()) % number_of_frontends));
                if (match["id"].Value.Last() == '1')
                    subdomain = 'a';

                var arr = JArray.Parse(imgs.Substring(imgs.IndexOf('[')));
                var img_urls = new List<string>();
                foreach (var obj in arr)
                    img_urls.Add($"https://{subdomain}a.hitomi.la/galleries/{match["id"].Value}/{obj.Value<string>("name")}");

                var result = new List<NetTask>();
                foreach (var img in img_urls)
                {
                    var task = NetTask.MakeDefault(img);
                    task.SaveFile = true;
                    task.Filename = img.Split('/').Last();
                }

                return new Tuple<List<NetTask>, object>(result, null);
            }

            return null;
        }
    }
}
