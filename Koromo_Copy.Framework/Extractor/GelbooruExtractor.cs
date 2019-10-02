// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using HtmlAgilityPack;
using Koromo_Copy.Framework.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using static Koromo_Copy.Framework.Extractor.IExtractorOption;

namespace Koromo_Copy.Framework.Extractor
{
    public class GelbooruExtractorOption : IExtractorOption
    {
        public int StartPage = 0;
        public int EndPage = int.MaxValue;
    }

    public class GelbooruExtractor : ExtractorModel
    {
        public GelbooruExtractor()
        {
            HostName = new Regex(@"gelbooru\.com");
            ValidUrl = new Regex(@"^https?://gelbooru\.com/index\.php\?.*?tags\=(.*?)(\&.*?)?$");
        }

        public override IExtractorOption RecommendOption(string url)
        {
            throw new NotImplementedException();
        }

        public override string RecommendFormat(IExtractorOption option)
        {
            throw new NotImplementedException();
        }

        public override Tuple<List<NetTask>, object> Extract(string url, IExtractorOption option = null)
        {
            var match = ValidUrl.Match(url).Groups;

            if (option == null)
                option = new GelbooruExtractorOption { Type = ExtractorType.Images };

            var tags = match[1].Value;
            var result = new List<NetTask>();
            var page = (option as GelbooruExtractorOption).StartPage;

            while (true)
            {
                var durl = "https://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=100&tags=" + tags + "&pid=" + page.ToString();

                option.PageReadCallback?.Invoke(durl);

                var data = NetTools.DownloadString(durl);

                var document = new HtmlDocument();
                document.LoadHtml(data);
                var nodes = document.DocumentNode.SelectNodes("/posts[1]/post");

                if (nodes == null || nodes.Count == 0)
                    break;

                foreach (var node in nodes)
                {
                    var imgurl = node.GetAttributeValue("file_url", "");
                    var task = NetTask.MakeDefault(imgurl);
                    task.SaveFile = true;
                    task.Filename = imgurl.Split('/').Last();
                    result.Add(task);
                }

                page += 1;

                if (page > (option as GelbooruExtractorOption).EndPage)
                    break;
            }

            return new Tuple<List<NetTask>, object>(result, HttpUtility.UrlDecode(tags));
        }
    }
}
