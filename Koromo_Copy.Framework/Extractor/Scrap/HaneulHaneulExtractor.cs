// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Network;
using Koromo_Copy.Framework.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static Koromo_Copy.Framework.Extractor.IExtractorOption;

namespace Koromo_Copy.Framework.Extractor.Scrap
{
    public class HaneulHaneulExtractorOption : IExtractorOption
    {
    }

    public class HaneulHaneulExtractor : ExtractorModel
    {
        public HaneulHaneulExtractor()
        {
            HostName = new Regex(@"hn-hn\.co\.kr");
            ValidUrl = new Regex(@"^https?://(www\.)?hn-hn\.co\.kr/shop/(?<menu>shopbrand|shopdetail|bestseller)\.html.*(xcode=(?<xcode>\d+|BEST)).*$");
        }

        public override IExtractorOption RecommendOption(string url)
        {
            return new EHentaiExtractorOption { Type = ExtractorType.Images };
        }

        public override string RecommendFormat(IExtractorOption option)
        {
            return "%(extractor)s/%(gallery)s/%(title)s/%(file)s.%(ext)s";
        }

        public override (List<NetTask>, ExtractedInfo) Extract(string url, IExtractorOption option = null)
        {
            var match = ValidUrl.Match(url).Groups;

            if (option == null)
                option = RecommendOption(url);

            var mtask = NetTask.MakeDefault(url);
            mtask.Encoding = Encoding.GetEncoding(51949);
            var html = NetTools.DownloadString(mtask);
            var node = html.ToHtmlNode();
            var result = new List<NetTask>();

            var xcode = match["xcode"].Value;

            if (match["menu"].Value == "shopbrand" || match["menu"].Value == "bestseller")
            {
                var filtering_filename = new string[] 
                {
                    "HN_Copyright2.jpg", 
                    "next_product.gif", 
                    "prev_product.gif",
                    "btn_h8_spin_dw.gif",
                    "btn_h8_spin_up.gif",
                    "Review.jpg", 
                    "shoppingguide2.jpg", 
                    "sizetip-2.jpg"
                };

                var gallery = node.SelectSingleNode("/html[1]/head[1]/title[1]").InnerText.Trim();
                option.SimpleInfoCallback?.Invoke(gallery);

                var last_page_node = node.SelectSingleNode("/html[1]/body[1]/div[3]/div[3]/div[1]/div[2]/div[3]/div[1]/div[5]/ol[1]/li[@class='last']/a");
                var last_page = 1;
                if (last_page_node != null)
                    last_page = node.SelectSingleNode("/html[1]/body[1]/div[3]/div[3]/div[1]/div[2]/div[3]/div[1]/div[5]/ol[1]/li[@class='last']/a").GetAttributeValue("href", "").Split('=').Last().ToInt();
                var page_urls = Enumerable.Range(1, last_page).Select(page => $"{url}&page={page}").ToList();

                var htmls = NetTools.DownloadStrings(page_urls);
                var sub_urls = new List<string>();

                foreach (var shtml in htmls)
                {
                    var snode = shtml.ToHtmlNode();
                    sub_urls.AddRange(snode.SelectNodes("/html[1]/body[1]/div[3]/div[3]/div[1]/div[2]/div[3]/div[1]/div[5]/table[1]/tbody[1]//a").Select(x => "http://www.hn-hn.co.kr" + x.GetAttributeValue("href", "")));
                }

                option.ProgressMax?.Invoke(sub_urls.Count);

                var sub_htmls = new List<string>();
                foreach (var surl in sub_urls)
                {
                    var task = NetTask.MakeDefault(surl);
                    task.Encoding = Encoding.GetEncoding(51949);
                    sub_htmls.Add(NetTools.DownloadString(task));
                    option.PostStatus?.Invoke(1);
                }

                foreach (var shtml in sub_htmls)
                {
                    var snode = shtml.ToHtmlNode();
                    var title = snode.SelectSingleNode("/html[1]/body[1]/div[3]/div[3]/div[1]/div[2]/div[1]/div[2]/div[1]/form[1]/div[1]/div[1]/h3[1]").InnerText.Trim();
                    var thumbnail = "http://www.hn-hn.co.kr" + snode.SelectSingleNode("/html[1]/body[1]/div[3]/div[3]/div[1]/div[2]/div[1]/div[2]/div[1]/div[3]/div[1]/a[1]/img[1]").GetAttributeValue("src", "").Split('?')[0];
                    var imgs = snode.SelectNodes("/html[1]/body[1]/div[3]/div[3]/div[1]/div[2]/div[1]/div[2]//img").Select(img => 
                    {
                        if (img.GetAttributeValue("src", "").StartsWith("http"))
                            return img.GetAttributeValue("src", "");
                        else
                            return "http://www.hn-hn.co.kr" + img.GetAttributeValue("src", "").Split('?')[0];
                    }).ToList();

                    foreach (var img in imgs)
                    {
                        var task = NetTask.MakeDefault(img);
                        task.SaveFile = true;
                        task.Filename = img.Split('/').Last();
                        if (filtering_filename.Contains(task.Filename))
                            continue;
                        task.Format = new ExtractorFileNameFormat
                        {
                            Gallery = gallery,
                            Title = title,
                            FilenameWithoutExtension = Path.GetFileNameWithoutExtension(task.Filename),
                            Extension = Path.GetExtension(task.Filename).Replace(".", "")
                        };
                        result.Add(task);
                    }
                }

                option.ThumbnailCallback?.Invoke(result[0]);
            }

            result.ForEach(task => task.Format.Extractor = GetType().Name.Replace("Extractor", ""));
            return (result, new ExtractedInfo { Type = ExtractedInfo.ExtractedType.Search });
        }
    }
}
