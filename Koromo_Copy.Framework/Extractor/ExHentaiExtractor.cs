// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using HtmlAgilityPack;
using Koromo_Copy.Framework.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Koromo_Copy.Framework.Extractor
{
    public class ExHentaiExtractor : ExtractorModel
    {
        public ExHentaiExtractor()
        {
            HostName = new Regex(@"exhentai\.org");
            ValidUrl = new Regex(@"^https?://exhentai\.org/g/(\d+)/(.*?)/?$");
            ExtractorInfo = "exhentai extactor info\r\n" +
                "   title:            English title.\r\n" +
                "   original_title:   Japanes title\r\n" +
                "   artist:           Artist name (if not exists N/A)\r\n" +
                "   group:            Group name (if not exists N/A)\r\n" +
                "   series:           Parody name (if not exists N/A)";
        }

        public override IExtractorOption RecommendOption(string url)
        {
            return new EHentaiExtractorOption { Type = IExtractorOption.ExtractorType.Images };
        }

        public override string RecommendFormat(IExtractorOption option)
        {
            return "%(extractor)s/%(title)s/%(file)s.%(ext)s";
        }

        readonly static List<string> cookies = new List<string>()
        {
            "igneous=30e0c0a66;ipb_member_id=2742770;ipb_pass_hash=6042be35e994fed920ee7dd11180b65f;sl=dm_2",
            "igneous=5676ef9eb21f775ab55895d02b30e2805d616aaed60eb5f9e7e5bddeb018be5596a971e6ad5947c4c1f2cb02ef069779db694b2649da1b0bfb5a7b2a23767fa4;ipb_member_id=2263496;ipb_pass_hash=6d94181101e10c5e8497c22bcfdf49e5;sl=dm_2",
            "ipb_member_id=1885095;ipb_pass_hash=c09d537c4eb19c406aca61fedc525eef",
            "ipb_member_id=1804967;ipb_pass_hash=1f3cf1b418ad112a234aea89d04ab7a8",
            "ipb_member_id=2195218;ipb_pass_hash=55e08b8e81a8c93f41c14bafb38e4d0a",
            "ipb_member_id=1715959;ipb_pass_hash=67e57ed90cfc3b391c8a32e920a31cf0",
        };

        public override (List<NetTask>, ExtractedInfo) Extract(string url, IExtractorOption option = null)
        {
            var html = NetTools.DownloadString(NetTask.MakeDefault(url, cookie: cookies[0]));
            var data = EHentaiExtractor.ParseArticleData(html, @"https://exhentai.org/.*?(?=\))");
            var pages = EHentaiExtractor.GetPagesUri(html);
            var image_urls = new List<string>();

            option.SimpleInfoCallback?.Invoke($"{data.Title}");

            if (option == null)
                option = RecommendOption(url);

            if (option.ExtractInformation)
                return (null, null/*data*/);

            //
            //  Extract Image Url-Url
            //

            image_urls.AddRange(EHentaiExtractor.GetImagesUri(html));

            for (int i = 1; i < pages.Length; i++)
            {
                (option as EHentaiExtractorOption).PageReadCallback?.Invoke(pages[i]);

                var page = NetTools.DownloadString(NetTask.MakeDefault(pages[i], cookie: cookies[0]));
                image_urls.AddRange(EHentaiExtractor.GetImagesUri(page));
            }

            //
            //  Extract Image Url
            //

            var result = new NetTask[image_urls.Count];

            var artist = "N/A";
            var group = "N/A";
            var series = "N/A";

            if (data.artist != null && data.artist.Length > 0)
                artist = data.artist[0];
            if (data.group != null && data.group.Length > 0)
                group = data.group[0];
            if (data.parody != null && data.parody.Length > 0)
                series = data.parody[0];

            if (artist == "N/A" && group != "N/A")
                artist = group;

            for (int i = 0; i < image_urls.Count; i++)
            {
                var html2 = NetTools.DownloadString(NetTask.MakeDefault(image_urls[i], cookies[0]));
                var durl = EHentaiExtractor.GetImagesAddress(html2);
                var task = NetTask.MakeDefault(durl, cookies[0]);
                task.SaveFile = true;
                task.Filename = durl.Split('/').Last();
                task.Format = new ExtractorFileNameFormat
                {
                    Title = data.Title,
                    FilenameWithoutExtension = Path.GetFileNameWithoutExtension(task.Filename),
                    Extension = Path.GetExtension(task.Filename).Replace(".", ""),
                    OriginalTitle = data.SubTitle,
                    Artist = artist,
                    Group = group,
                    Series = series
                };
                result[i] = task;
                if (i == 0)
                    option.ThumbnailCallback?.Invoke(task);
            }

            var result_list = result.ToList();
            result_list.ForEach(task => task.Format.Extractor = GetType().Name.Replace("Extractor", ""));
            return (result_list, new ExtractedInfo { Type = ExtractedInfo.ExtractedType.WorksComic });
        }
    }
}
