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
    public class JmanaExtractorExtractorOption : IExtractorOption
    {
    }

    public class JmanaExtractor : ExtractorModel
    {
        public JmanaExtractor()
        {
            HostName = new Regex(@"jamana.com\d+\.net");
            ValidUrl = new Regex(@"^(?<host>https?://(www\.)?jmana\d*\.com).*/(?<title>.*?)");
        }

        public override IExtractorOption RecommendOption(string url)
        {
            var match = ValidUrl.Match(url).Groups;

            if (match["type"].Value == "board")
                return new NaverExtractorOption { Type = ExtractorType.EpisodeImages };
            else
                return new NaverExtractorOption { Type = ExtractorType.Works };
        }

        public override string RecommendFormat(IExtractorOption option)
        {
            if (option.Type == ExtractorType.EpisodeImages)
                return "%(extractor)s/%(episode)s/%(file)s.%(ext)s";
            else
                return "%(extractor)s/%(title)s/%(episode)s/%(file)s.%(ext)s";
        }

        public override (List<NetTask>, ExtractedInfo) Extract(string url, IExtractorOption option = null)
        {
            if (option == null)
                option = RecommendOption(url);

            var html = NetTools.DownloadString(url);
            var match = ValidUrl.Match(url).Groups;

            var node = html.ToHtmlNode();

            var title = node.SelectSingleNode("/html[1]/body[1]/div[1]/div[3]/div[1]/h1[1]").InnerText.Trim();
            var genre = node.SelectSingleNode("/html[1]/body[1]/div[1]/div[3]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/ul[1]/li[1]/div[1]/div[2]/div[2]/h3[1]/a[1]").InnerText.Trim();
            var artist = node.SelectSingleNode("/html[1]/body[1]/div[1]/div[3]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/ul[1]/li[1]/div[1]/div[2]/div[3]/h3[1]/a[1]").InnerText.Trim();

            var sub_urls = new List<string>();
            var sub_titles = new List<string>();

            foreach (var episode in node.SelectNodes("/html[1]/body[1]/div[1]/div[3]/div[2]/div[1]/div[1]/div[2]/div[1]/div[1]/div"))
            {
                var tag_a = episode.SelectSingleNode("./div[2]/h2[1]/a[1]");
                sub_urls.Add(tag_a.GetAttributeValue("href", ""));
                sub_titles.Add(tag_a.InnerText.Trim());
            }

            option.SimpleInfoCallback?.Invoke(title);
            option.ThumbnailCallback?.Invoke(NetTask.MakeDefault(
                node.SelectSingleNode("/html[1]/body[1]/div[1]/div[3]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/ul[1]/li[1]/div[1]/div[1]/a[1]/img[1]").GetAttributeValue("src", "")));

            option.ProgressMax?.Invoke(sub_urls.Count);

            var sub_htmls = NetTools.DownloadStrings(sub_urls, "", () =>
            {
                option.PostStatus?.Invoke(1);
            });

            var result = new List<NetTask>();

            for (int i = 0; i < sub_urls.Count; i++)
            {
                var snode = sub_htmls[i].ToHtmlNode();
                int count = 1;
                foreach (var img in snode.SelectNodes("/html[1]/body[1]/div[1]/div[3]/div[2]/div[1]/div[2]/ul[1]//li/div[1]/img[1]"))
                {
                    var img_src = img.GetAttributeValue("data-src", "");
                    if (string.IsNullOrWhiteSpace(img_src))
                        img_src = img.GetAttributeValue("src", "");
                    var task = NetTask.MakeDefault(HttpUtility.HtmlDecode(img_src));
                    task.SaveFile = true;
                    task.Filename = count.ToString("000") + ".jpg";
                    task.Format = new ExtractorFileNameFormat
                    {
                        Title = title,
                        Episode = sub_titles[i],
                        FilenameWithoutExtension = count.ToString("000"),
                        Extension = Path.GetExtension(task.Filename).Replace(".", ""),
                    };
                    result.Add(task);
                    count++;
                }
            }

            result.ForEach(task => task.Format.Extractor = GetType().Name.Replace("Extractor", ""));
            return (result, new ExtractedInfo { Type = ExtractedInfo.ExtractedType.WorksComic });
        }
    }
}
