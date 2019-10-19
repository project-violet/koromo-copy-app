// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.CL;
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
    public class DanbooruExtractorOption : IExtractorOption
    {
        [CommandLine("--start-page", CommandType.ARGUMENTS, Info = "Set start page.", Help = "use --start-page <Start Page Number>")]
        public new string[] StartPage;
        [CommandLine("--end-page", CommandType.ARGUMENTS, Info = "Set end page.", Help = "use --start-page <End Page Number>")]
        public new string[] EndPage;
        [CommandLine("--exclude-video", CommandType.OPTION, Info = "Exclude video.")]
        public bool ExcludeVideo;

        public override void CLParse(ref IExtractorOption model, string[] args)
        {
            model = CommandLineParser.Parse(model as DanbooruExtractorOption, args);
        }
    }

    public class DanbooruExtractor : ExtractorModel
    {
        public DanbooruExtractor()
        {
            HostName = new Regex(@"danbooru\.donmai\.us");
            ValidUrl = new Regex(@"^https?://danbooru\.donmai\.us/posts\?.*?(tags=(?<search>.*))?&?.*?$");
        }

        public override IExtractorOption RecommendOption(string url)
        {
            return new DanbooruExtractorOption { Type = ExtractorType.Images };
        }

        public override string RecommendFormat(IExtractorOption option)
        {
            return "%(extractor)s/%(search)s/%(file)s.%(ext)s";
        }

        public override (List<NetTask>, ExtractedInfo) Extract(string url, IExtractorOption option = null)
        {
            var match = ValidUrl.Match(url).Groups;

            if (option == null)
                option = new DanbooruExtractorOption { Type = ExtractorType.Images };

            var tags = match["search"].Value;
            var result = new List<NetTask>();
            var page = 1;

            option.SimpleInfoCallback?.Invoke($"{tags}");

            if ((option as DanbooruExtractorOption).StartPage != null)
                page = (option as DanbooruExtractorOption).StartPage[0].ToInt();

            var end_page = int.MaxValue;
            if ((option as DanbooruExtractorOption).EndPage != null)
                end_page = (option as DanbooruExtractorOption).EndPage[0].ToInt();

            var already_thumbnail = false;
            while (true)
            {
                var durl = $"https://danbooru.donmai.us/posts?tags={tags}&page=" + page.ToString();

                option.PageReadCallback?.Invoke(durl);

                var html = NetTools.DownloadString(durl);
                var node = html.ToHtmlNode().SelectNodes("/html[1]/body[1]/div[1]/div[3]/div[1]/section[1]/div[3]/div[1]/article");

                if (node == null)
                    break;

                var ds = new List<string>();
                foreach (var sub in node)
                    ds.Add("https://danbooru.donmai.us" + sub.SelectSingleNode("./a").GetAttributeValue("href", ""));

                var htmls = NetTools.DownloadStrings(ds);

                //foreach (var shtml in htmls)
                for (int i = 0; i < htmls.Count; i++)
                {
                    var snode = htmls[i].ToHtmlNode();

                    var img_url = "";
                    // Just one banner
                    if (snode.SelectSingleNode("/html[1]/body[1]/div[1]/div[2]/div[1]/section[1]/div[1]/span[1]/a[1]")?.GetAttributeValue("id", "") == "image-resize-link")
                    {
                        img_url = snode.SelectSingleNode("/html[1]/body[1]/div[1]/div[2]/div[1]/section[1]/div[1]/span[1]/a[1]").GetAttributeValue("href", "");
                    }
                    // Two banner
                    else if (snode.SelectSingleNode("/html[1]/body[1]/div[1]/div[2]/div[1]/section[1]/div[2]/span[1]/a[1]")?.GetAttributeValue("id", "") == "image-resize-link")
                    {
                        img_url = snode.SelectSingleNode("/html[1]/body[1]/div[1]/div[2]/div[1]/section[1]/div[2]/span[1]/a[1]").GetAttributeValue("href", "");
                    }
                    // Three or none banner
                    else if (snode.SelectSingleNode("/html[1]/body[1]/div[1]/div[2]/div[1]/section[1]/section[1]/img[1]") != null)
                    {
                        img_url = snode.SelectSingleNode("/html[1]/body[1]/div[1]/div[2]/div[1]/section[1]/section[1]/img[1]").GetAttributeValue("src", "");
                    }
                    // Video URL
                    else if (snode.SelectSingleNode("/html[1]/body[1]/div[1]/div[2]/div[1]/section[1]/section[1]/p[1]/a[1]") != null)
                    {
                        if ((option as DanbooruExtractorOption).ExcludeVideo)
                            continue;
                        img_url = snode.SelectSingleNode("/html[1]/body[1]/div[1]/div[2]/div[1]/section[1]/section[1]/p[1]/a[1]").GetAttributeValue("href", "");
                    }
                    else
                    {
                        // ?
                        Log.Logs.Instance.PushError("[DanbooruExtractor] Cannot find html format! " + ds[i]);
                    }

                    var task = NetTask.MakeDefault(img_url);
                    task.SaveFile = true;
                    task.Filename = img_url.Split('/').Last();
                    task.Format = new ExtractorFileNameFormat
                    {
                        Search = tags,
                        FilenameWithoutExtension = Path.GetFileNameWithoutExtension(task.Filename),
                        Extension = Path.GetExtension(task.Filename).Replace(".", "")
                    };
                    result.Add(task);
                }

                if (!already_thumbnail)
                {
                    option.ThumbnailCallback?.Invoke(result[0]);
                    already_thumbnail = true;
                }

                page += 1;

                if (page > end_page)
                    break;
            }

            result.ForEach(task => task.Format.Extractor = GetType().Name.Replace("Extractor", ""));

            return (result, new ExtractedInfo { Type = ExtractedInfo.ExtractedType.UserArtist });
        }
    }
}
