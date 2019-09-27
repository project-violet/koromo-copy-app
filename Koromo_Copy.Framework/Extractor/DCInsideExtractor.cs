// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Koromo_Copy.Framework.Network;

namespace Koromo_Copy.Framework.Extractor
{
    public class DCInsideExtractorOption : ExtractorOption
    {

    }

    public class DCInsideExtractor : ExtractorModel
    {
        public new static string ValidUrl()
            => @"^https?://(gall|m)\.dcinside\.com/(mgallery/)?board/(lists|view/)\?(.*?)$";

        /// <summary>
        /// Extract Images
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public override List<NetTask> Extract(string url)
        {
            var regex = new Regex(ValidUrl());
            var match = regex.Match(url).Groups;
            var result = new List<NetTask>();
            var html = NetTools.DownloadStringAsync(NetTask.MakeDefault(url)).Result;

            if (html == null)
                return result;

            if (match[1].Value == "gall")
            {
                try
                {
                    var document = new HtmlDocument();
                    document.LoadHtml(html);
                    HtmlNode node = document.DocumentNode.SelectNodes("//div[@class='view_content_wrap']")[0];

                    var ImagesLink = node.SelectNodes("//ul[@class='appending_file']/li").Select(x => x.SelectSingleNode("./a").GetAttributeValue("href", "")).ToList();
                    var FilesName = node.SelectNodes("//ul[@class='appending_file']/li").Select(x => x.SelectSingleNode("./a").InnerText).ToList();

                    for (int i = 0; i < ImagesLink.Count; i++)
                    {
                        var task = NetTask.MakeDefault(ImagesLink[i]);
                        task.Filename = FilesName[i];
                        task.SaveFile = true;
                        task.Referer = url;
                        result.Add(task);
                    }
                }
                catch (Exception e)
                {
                    Log.Logs.Instance.PushError("[DCInsideExtractor] Image extract error - " + e.Message + "\r\n" + e.StackTrace);
                }
            }

            return result;
        }
    }
}
