// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Network;
using Koromo_Copy.Framework.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static Koromo_Copy.Framework.Extractor.IExtractorOption;

namespace Koromo_Copy.Framework.Extractor
{
    public class FunbeExtractorOption : IExtractorOption
    {
    }

    public class FunbeExtractor : ExtractorModel
    {
        public FunbeExtractor()
        {
            HostName = new Regex(@"funbe\d+\.com");
            ValidUrl = new Regex(@"^https?://(?<host>funbe\d+.com)/(?<title>.*)$");
        }

        public override IExtractorOption RecommendOption(string url)
        {
            return new FunbeExtractorOption { Type = ExtractorType.Works };
        }

        public override string RecommendFormat(IExtractorOption option)
        {
            return "%(title)s/%(episode)s/%(file)s.%(ext)s";
        }

        public override (List<NetTask>, ExtractedInfo) Extract(string url, IExtractorOption option = null)
        {
            if (option == null)
                option = RecommendOption(url);

            var match = ValidUrl.Match(url).Groups;
            var host = "https://" + match["host"];
            var html = NetTools.DownloadString(url);
            var node = html.ToHtmlNode();

            var title = node.SelectSingleNode("/html[1]/body[1]/div[2]/div[1]/div[1]/div[3]/div[1]/table[2]/tbody[1]/tr[2]/td[1]/table[1]/tr[1]/td[1]").InnerText;
            var sub_datas = node.SelectNodes("/html[1]/body[1]/div[2]/div[1]/div[1]/div[3]/div[1]/div[1]/form[1]/table[1]//tr/td[2]");

            option.SimpleInfoCallback?.Invoke($"{title}");

            var sub_urls = new List<string>();
            var sub_titles = new List<string>();
            foreach (var sub_data in sub_datas)
            {
                sub_urls.Add(host + sub_data.GetAttributeValue("data-role", ""));
                sub_titles.Add(sub_data.InnerText.Trim());
            }

            var htmls = NetTools.DownloadStrings(sub_urls);

            var result = new List<NetTask>();
            for (int i = 0; i < htmls.Count; i++)
            {
                var base64encoded = Regex.Match(htmls[i], "var toon_img = '(.*)'").Groups[1].Value;
                string rhtml;
                Strings.TryParseBase64(base64encoded, out rhtml);

                var snode = rhtml.ToHtmlNode();

                int count = 1;
                foreach (var img in snode.SelectNodes("/img"))
                {
                    var task = NetTask.MakeDefault(host + img.GetAttributeValue("src", ""));
                    task.SaveFile = true;
                    task.Filename = task.Url.Split('/').Last();
                    task.Format = new ExtractorFileNameFormat
                    {
                        Title = title,
                        Episode = sub_titles[i],
                        FilenameWithoutExtension = count.ToString("000"),
                        Extension = Path.GetExtension(task.Filename).Replace(".", "")
                    };
                    result.Add(task);
                    count++;
                }
            }

            return (result, new ExtractedInfo { Type = ExtractedInfo.ExtractedType.WorksComic });
        }
    }
}
