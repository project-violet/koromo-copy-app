// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using HtmlAgilityPack;
using Koromo_Copy.Framework.Network;
using Koromo_Copy.Framework.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using static Koromo_Copy.Framework.Extractor.IExtractorOption;

namespace Koromo_Copy.Framework.Extractor
{
    public class NaverExtractorOption : IExtractorOption
    {
    }

    public class NaverExtractor : ExtractorModel
    {
        public NaverExtractor()
        {
            HostName = new Regex(@"(comic|blog)\.naver\.com");
            ValidUrl = new Regex(@"^https?://(comic|blog)\.naver\.com/(webtoon|.*?)/((list|detail)\.nhn\?|.*)\??(titleId\=(?<id>\d+))?(.*?)$");
            //IsForbidden = true;
        }
        
        public override IExtractorOption RecommendOption(string url)
        {
            var match = ValidUrl.Match(url).Groups;

            if (match[1].Value == "comic")
            {
                if (match[4].Value == "detail")
                {
                    return new NaverExtractorOption { Type = NaverExtractorOption.ExtractorType.EpisodeImages };
                }
                else if (match[4].Value == "list")
                {
                    return new NaverExtractorOption { Type = NaverExtractorOption.ExtractorType.ComicIndex };
                }
            }
            else if (match[1].Value == "blog")
            {
                return new NaverExtractorOption { Type = NaverExtractorOption.ExtractorType.Images };
            }

            return new NaverExtractorOption { Type = NaverExtractorOption.ExtractorType.Images };
        }

        public override string RecommendFormat(IExtractorOption option)
        {
            return "%(extractor)s/%(title)s/%(episode)s/%(file)s.%(ext)s";
        }

        public override (List<NetTask>, ExtractedInfo) Extract(string url, IExtractorOption option = null)
        {
            if (option == null)
                option = RecommendOption(url);

            var html = NetTools.DownloadString(url);

            //
            //  Extract Webtoon
            //

            if (option.Type == ExtractorType.EpisodeImages)
            {
                return (extract_episode_page(html), null);
            }
            else if (option.Type == ExtractorType.ComicIndex)
            {
                var match = ValidUrl.Match(url).Groups;
                var max_no = Regex.Match(html, @"/webtoon/detail\.nhn\?titleId=\d+&no=(\d+)").Groups[1].Value.ToInt();
                var urls = new List<string>();
                for (int i = 1; i <= max_no; i++)
                    urls.Add($"https://comic.naver.com/webtoon/detail.nhn?titleId={match["id"]}&no={i}");

                var htmls = NetTools.DownloadStrings(urls);
                var result = new List<NetTask>();

                foreach (var shtml in htmls)
                    result.AddRange(extract_episode_page(shtml));

                return (result, new ExtractedInfo { Type = ExtractedInfo.ExtractedType.WorksComic });
            }

            return (null, null);
        }

        private List<NetTask> extract_episode_page(string html)
        {
            var node = html.ToHtmlNode();

            var title = node.SelectSingleNode("/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[2]/h2[1]").MyText();
            var sub_title = node.SelectSingleNode("/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[2]/div[1]/h3[1]").InnerText.Trim();
            var author = node.SelectSingleNode("/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[2]/h2[1]/span[1]").InnerText.Trim();

            var imgs = node.SelectNodes("/html[1]/body[1]/div[1]/div[2]/div[1]/div[1]/div[3]/div[1]/img");
            var result = new List<NetTask>();

            int count = 1;
            foreach (var img in imgs)
            {
                var durl = img.GetAttributeValue("src", "");
                var task = NetTask.MakeDefault(durl);
                task.SaveFile = true;
                task.Filename = durl.Split('/').Last();
                task.Format = new ExtractorFileNameFormat
                {
                    Title = title,
                    Episode = sub_title,
                    FilenameWithoutExtension = count++.ToString("000"),
                    Extension = Path.GetExtension(task.Filename).Replace(".", "")
                };
                result.Add(task);
            }

            result.ForEach(task => task.Format.Extractor = GetType().Name.Replace("Extractor", ""));
            return result;
        }
    }
}
