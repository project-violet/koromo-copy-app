// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using HtmlAgilityPack;
using Koromo_Copy.Framework.Network;
using Koromo_Copy.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Koromo_Copy.Framework.Extractor
{
    public class NaverExtractorOption : IExtractorOption
    {
        public enum ExtractorType
        {
            Images = 0, // Default
            EpisodeImages = 1,
            ComicIndex = 2,
        }

        public ExtractorType Type;
    }

    public class NaverExtractor : ExtractorModel<NaverExtractorOption>
    {
        static NaverExtractor()
            => ValidUrl = new Regex(@"^https?://(comic|blog)\.naver\.com/(webtoon|.*?)/((list|detail)\.nhn\?|.*)\??(titleId\=(\d+)\&no=(\d+))?(.*?)$");

        public class ComicInformation
        {
            public string Title;
            public string Author;
        }

        public new static NaverExtractorOption RecommendOption(string url)
        {
            var match = ValidUrl.Match(url).Groups;

            //
            //  Extract Webtoon
            //

            if (match[1].Value == "comic")
            {
                //
                //  Just One Episode
                //

                if (match[4].Value == "detail")
                {
                    return new NaverExtractorOption { Type = NaverExtractorOption.ExtractorType.EpisodeImages };
                }

                //
                //  All Episode
                //

                else if (match[4].Value == "list")
                {
                    return new NaverExtractorOption { Type = NaverExtractorOption.ExtractorType.ComicIndex };
                }
            }

            //
            //  Extract Blog
            //

            else if (match[1].Value == "blog")
            {
                return new NaverExtractorOption { Type = NaverExtractorOption.ExtractorType.Images };
            }

            return new NaverExtractorOption { Type = NaverExtractorOption.ExtractorType.Images };
        }

        public new static Tuple<List<NetTask>, object> Extract(string url, NaverExtractorOption option = null)
        {
            if (option == null)
                option = RecommendOption(url);

            //
            //  Extract Webtoon
            //

            if (option.Type == NaverExtractorOption.ExtractorType.EpisodeImages)
            {
                var html = NetTools.DownloadString(url);

                var document = new HtmlDocument();
                document.LoadHtml(html);
                var node = document.DocumentNode;

                var cinfo = new ComicInformation
                {
                    Title = node.SelectSingleNode("/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[2]/h2[1]").MyText() +
                            " - " +
                            node.SelectSingleNode("/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[2]/div[1]/h3[1]").InnerText.Trim(),
                    Author = node.SelectSingleNode("/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[2]/h2[1]/span[1]").InnerText.Trim()
                };

                var imgs = node.SelectNodes("/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[3]/div[1]/img");
                var result = new List<NetTask>();

                foreach (var img in imgs)
                {
                    var durl = img.GetAttributeValue("src", "");
                    var task = NetTask.MakeDefault(durl);
                    task.SaveFile = true;
                    task.Filename = durl.Split('/').Last();
                    result.Add(task);
                }

                return new Tuple<List<NetTask>, object>(result, cinfo);
            }

            return null;
        }
    }
}
