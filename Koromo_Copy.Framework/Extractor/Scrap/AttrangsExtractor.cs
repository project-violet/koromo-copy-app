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
using System.Threading;
using static Koromo_Copy.Framework.Extractor.IExtractorOption;

namespace Koromo_Copy.Framework.Extractor.Scrap
{
    public class AttrangsExtractorOption : IExtractorOption
    {
    }

    public class AttrangsExtractor : ExtractorModel
    {
        public AttrangsExtractor()
        {
            HostName = new Regex(@"attrangs\.co\.kr");
            ValidUrl = new Regex(@"^https?://attrangs\.co\.kr/shop/(?<type>list|view)\.php\?(cate|index_no)=(?<id>\d+)$");
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

            var html = NetTools.DownloadString(url);
            var node = html.ToHtmlNode();
            var result = new List<NetTask>();

            var id = match["id"].Value;

            if (match["type"].Value == "list")
            {
                var title = node.SelectSingleNode("/html[1]/head[1]/title[1]").InnerText.Replace("아뜨랑스 - ", "").Trim();
                var sub_urls = get_first_urls(html);
                var page = 1;
                var prev_count = 0;

                option.SimpleInfoCallback?.Invoke(title);

                do
                {
                    prev_count = sub_urls.Count;
                    var task = NetTask.MakeDefault("https://attrangs.co.kr/shop/list_ajax.php");
                    task.Query = make_next_list_dict(id, page++);
                    sub_urls.UnionWith(get_first_urls(NetTools.DownloadString(task)));
                } while (prev_count != sub_urls.Count);

                option.ProgressMax?.Invoke(sub_urls.Count);

                var sub_htmls = new List<string>();
                var rand = new Random();
                foreach (var surl in sub_urls)
                {
                    sub_htmls.Add(NetTools.DownloadString(surl));
                    option.PostStatus?.Invoke(1);
                    // Kuipernet Handling
                    Thread.Sleep(rand.Next(5, 11) * 100);
                }

                foreach (var shtml in sub_htmls)
                {
                    var view = new AttrangsViewParser(shtml);

                    foreach (var img in view.Images)
                    {
                        var task = NetTask.MakeDefault(img);
                        task.SaveFile = true;
                        task.Filename = img.Split('/').Last();
                        task.Format = new ExtractorFileNameFormat
                        {
                            Gallery = title,
                            Title = view.Title,
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

        static HashSet<string> get_first_urls(string html)
        {
            var regex = new Regex(@"/shop/view\.php\?index_no=(\d+)");
            return new HashSet<string>(regex.Matches(html).OfType<Match>().Select(x => "https://attrangs.co.kr" + x.Groups[0].Value).ToList());
        }

        static Dictionary<string, string> make_next_list_dict(string id, int page)
        {
            return new Dictionary<string, string>
                {
                    { "page", page.ToString() } ,
                    { "cate", id } ,
                    { "orby", "" } ,
                    { "soldout", "" } ,
                };
        }

        public class AttrangsViewParser
        {
            public string ThumbnailURL;
            public string Title;
            public List<string> Images = new List<string>();

            public AttrangsViewParser (string html)
            {
                var node = html.ToHtmlNode();
                ThumbnailURL = node.SelectSingleNode("/html[1]/body[1]/div[2]/div[3]/div[1]/div[1]/div[1]/div[1]/div[1]/img[1]").GetAttributeValue("src", "");
                Title = node.SelectSingleNode("/html[1]/head[1]/title[1]").InnerText.Replace("아뜨랑스 - ", "").Trim();

                try
                {
                    // Thumbnail Images
                    Images.AddRange(node.SelectNodes("/html[1]/body[1]/div[2]/div[3]/div[1]/div[1]/div[1]/div[1]/div[2]/div[1]/div[1]//img").Select(img => img.GetAttributeValue("src", "")));
                    // Model Image Shot
                    var mm = node.SelectNodes("/html[1]/body[1]/div[2]/div[3]/div[1]/div[1]/div[10]/div[1]/div[1]/div[1]/div[1]//img");
                    if (mm != null)
                        Images.AddRange(mm.Select(img => "https:" + img.GetAttributeValue("src", "")));
                    // Body Images
                    var xx = node.SelectNodes("/html[1]/body[1]/div[2]/div[3]/div[1]/div[1]/div[11]/div[3]/div[1]/div[5]//img");
                    if (xx == null)
                        xx = node.SelectNodes("/html[1]/body[1]/div[2]/div[3]/div[1]/div[1]/div[11]/div[4]/div[1]/div[5]//img");
                    if (xx == null)
                        xx = node.SelectNodes("/html[1]/body[1]/div[2]/div[3]/div[1]/div[1]/div[11]/div[4]/div[1]/div[6]//img");
                    if (xx == null)
                        xx = node.SelectNodes("/html[1]/body[1]/div[2]/div[3]/div[1]/div[1]/div[11]/div[3]/div[1]/div[6]//img");
                    Images.AddRange(xx.Select(img => img.GetAttributeValue("src", "")));
                }
                catch (Exception e)
                {
                    ;
                }
            }
        }

    }
}
