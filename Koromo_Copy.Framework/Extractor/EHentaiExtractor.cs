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
using System.Web;

namespace Koromo_Copy.Framework.Extractor
{
    #region EHentai, ExHentai Models

    public class EHentaiArticle
    {
        public string Thumbnail { get; set; }

        public string Title { get; set; }
        public string SubTitle { get; set; }

        public string Type { get; set; }
        public string Uploader { get; set; }

        public string Posted { get; set; }
        public string Parent { get; set; }
        public string Visible { get; set; }
        public string Language { get; set; }
        public string FileSize { get; set; }
        public int Length;
        public int Favorited;

        public string reclass;
        public string[] language;
        public string[] group;
        public string[] parody;
        public string[] character;
        public string[] artist;
        public string[] male;
        public string[] female;
        public string[] misc;

        public Tuple<DateTime, string, string>[] comment;
        public List<string> ImagesLink { get; set; }
        public string Archive { get; set; }
    }

    #endregion

    public class EHentaiExtractorOption : IExtractorOption
    {
    }

    public class EHentaiExtractor : ExtractorModel
    {
        public EHentaiExtractor()
        {
            HostName = new Regex(@"e-hentai\.org");
            ValidUrl = new Regex(@"^https?://e-hentai\.org/g/(\d+)/(.*?)/?$");
            ExtractorInfo = "e-hentai extactor info\r\n" +
                "   title:            English title.\r\n" +
                "   original_title:   Japanes title\r\n" +
                "   artist:           Artist name (if not exists N/A)\r\n" +
                "   group:            Group name (if not exists N/A)\r\n" +
                "   series:           Parody name (if not exists N/A)";
        }

        public override IExtractorOption RecommendOption(string url)
        {
            return new EHentaiExtractorOption { Type = EHentaiExtractorOption.ExtractorType.Images };
        }

        public override string RecommendFormat(IExtractorOption option)
        {
            return "%(title)s/%(file)s.%(ext)s";
        }

        public override Tuple<List<NetTask>, object> Extract(string url, IExtractorOption option = null)
        {
            var html = NetTools.DownloadString(url);
            var data = ParseArticleData(html);
            var pages = GetPagesUri(html);
            var image_urls = new List<string>();

            if (option == null)
                option = RecommendOption(url);

            if (option.ExtractInformation)
                return new Tuple<List<NetTask>, object>(null, data);

            //
            //  Extract Image Url-Url
            //

            image_urls.AddRange(GetImagesUri(html));

            for (int i = 1; i < pages.Length; i++)
            {
                (option as EHentaiExtractorOption).PageReadCallback?.Invoke(pages[i]);

                var page = NetTools.DownloadString(pages[i]);
                image_urls.AddRange(GetImagesUri(page));
            }

            //
            //  Extract Image Url
            //

            var result = new NetTask[image_urls.Count];
            var count = image_urls.Count;
            var wait = new ManualResetEvent(false);

            var artist = "N/A";
            var group = "N/A";
            var series = "N/A";

            if (data.artist != null && data.artist.Length > 0)
                artist = data.artist[0];
            if (data.group != null && data.group.Length > 0)
                group = data.group[0];
            if (data.parody != null && data.parody.Length > 0)
                series = data.parody[0];

            for (int i = 0; i < image_urls.Count; i++)
            {
                var task = NetTask.MakeDefault(image_urls[i]);
                var j = i;

                task.Priority = new NetPriority { Type = NetPriorityType.Trivial, TaskPriority = i };
                task.DownloadString = true;
                task.CompleteCallbackString = (string str) =>
                {
                    var durl = GetImagesAddress(str);
                    var tt = NetTask.MakeDefault(durl);
                    tt.SaveFile = true;
                    tt.Filename = durl.Split('/').Last();
                    tt.Format = new ExtractorFileNameFormat
                    {
                        Title = data.Title,
                        FilenameWithoutExtension = Path.GetFileNameWithoutExtension(tt.Filename),
                        Extension = Path.GetExtension(tt.Filename).Replace(".", ""),
                        OriginalTitle = data.SubTitle,
                        Artist = artist,
                        Group = group,
                        Series = series
                    };
                    result[j] = tt;
                    if (Interlocked.Decrement(ref count) == 0)
                        wait.Set();
                };

                AppProvider.Scheduler.Add(task);
            }

            wait.WaitOne();

            return new Tuple<List<NetTask>, object>(result.ToList(), data);
        }

        #region Parse for EHentai Web Site

        public static EHentaiArticle ParseArticleData(string source, string thumbnail = @"https://ehgt.org/.*?(?=\))")
        {
            EHentaiArticle article = new EHentaiArticle();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(source);
            HtmlNode nodes = document.DocumentNode.SelectNodes("//div[@class='gm']")[0];

            article.Thumbnail = Regex.Match(nodes.SelectSingleNode(".//div[@id='gleft']//div//div").GetAttributeValue("style", ""), thumbnail).Groups[0].Value;

            article.Title = nodes.SelectSingleNode(".//div[@id='gd2']//h1[@id='gn']").InnerText;
            article.SubTitle = nodes.SelectSingleNode(".//div[@id='gd2']//h1[@id='gj']").InnerText;

            try { article.Type = nodes.SelectSingleNode(".//div[@id='gmid']//div//div[@id='gdc']//a//img").GetAttributeValue("alt", ""); } catch { }
            article.Uploader = nodes.SelectSingleNode(".//div[@id='gmid']//div//div[@id='gdn']//a").InnerText;

            HtmlNodeCollection nodes_static = nodes.SelectNodes(".//div[@id='gmid']//div//div[@id='gdd']//table//tr");

            article.Posted = nodes_static[0].SelectSingleNode(".//td[@class='gdt2']").InnerText;
            article.Parent = nodes_static[1].SelectSingleNode(".//td[@class='gdt2']").InnerText;
            article.Visible = nodes_static[2].SelectSingleNode(".//td[@class='gdt2']").InnerText;
            article.Language = nodes_static[3].SelectSingleNode(".//td[@class='gdt2']").InnerText.Split(' ')[0].ToLower();
            article.FileSize = nodes_static[4].SelectSingleNode(".//td[@class='gdt2']").InnerText;
            int.TryParse(nodes_static[5].SelectSingleNode(".//td[@class='gdt2']").InnerText.Split(' ')[0], out article.Length);
            int.TryParse(nodes_static[6].SelectSingleNode(".//td[@class='gdt2']").InnerText.Split(' ')[0], out article.Favorited);

            HtmlNodeCollection nodes_data = nodes.SelectNodes(".//div[@id='gmid']//div[@id='gd4']//table//tr");

            Dictionary<string, string[]> information = new Dictionary<string, string[]>();

            foreach (var i in nodes_data)
            {
                try
                {
                    information.Add(i.SelectNodes(".//td")[0].InnerText.Trim(),
                        i.SelectNodes(".//td")[1].SelectNodes(".//div").Select(e => e.SelectSingleNode(".//a").InnerText).ToArray());
                }
                catch { }
            }

            if (information.ContainsKey("language:")) article.language = information["language:"];
            if (information.ContainsKey("group:")) article.group = information["group:"];
            if (information.ContainsKey("parody:")) article.parody = information["parody:"];
            if (information.ContainsKey("character:")) article.character = information["character:"];
            if (information.ContainsKey("artist:")) article.artist = information["artist:"];
            if (information.ContainsKey("male:")) article.male = information["male:"];
            if (information.ContainsKey("female:")) article.female = information["female:"];
            if (information.ContainsKey("misc:")) article.misc = information["misc:"];

            HtmlNode nodesc = document.DocumentNode.SelectNodes("//div[@id='cdiv']")[0];
            HtmlNodeCollection nodes_datac = nodesc.SelectNodes(".//div[@class='c1']");
            List<Tuple<DateTime, string, string>> comments = new List<Tuple<DateTime, string, string>>();

            foreach (var i in nodes_datac ?? Enumerable.Empty<HtmlNode>())
            {
                try
                {
                    string date = HttpUtility.HtmlDecode(i.SelectNodes(".//div[@class='c2']//div[@class='c3']")[0].InnerText.Trim());
                    string author = HttpUtility.HtmlDecode(i.SelectNodes(".//div[@class='c2']//div[@class='c3']//a")[0].InnerText.Trim());
                    string contents = Regex.Replace(HttpUtility.HtmlDecode(i.SelectNodes(".//div[@class='c6']")[0].InnerHtml.Trim()), @"<br>", "\r\n");
                    comments.Add(new Tuple<DateTime, string, string>(
                        DateTime.Parse(date.Remove(date.IndexOf(" UTC")).Substring("Posted on ".Length) + "Z"),
                        author,
                        contents));
                }
                catch { }
            }

            comments.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            article.comment = comments.ToArray();

            return article;
        }

        public static string[] GetPagesUri(string source)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(source);
            HtmlNode nodes = document.DocumentNode.SelectNodes("//div[@class='gtb']")[0];

            List<string> uri = new List<string>();
            try
            {
                foreach (var div in nodes.SelectNodes(".//table//tr//td[contains(@onclick, 'document')]"))
                    try
                    {
                        uri.Add(div.SelectSingleNode(".//a").GetAttributeValue("href", ""));
                    }
                    catch { }
            }
            catch
            {
                uri.Add(nodes.SelectSingleNode(".//table//tr//td[@class='ptds']//a").GetAttributeValue("href", "") + "?p=0");
            }

            int max = 0;
            foreach (var page_c in uri)
            {
                int value;
                if (int.TryParse(Regex.Split(page_c, @"\?p\=")[1], out value))
                    if (max < value)
                        max = value;
            }

            if (uri.Count == 0) return null;

            var result = new List<string>();
            var prefix = Regex.Split(uri[0], @"\?p\=")[0];
            for (int i = 0; i <= max; i++)
                result.Add(prefix + "?p=" + i.ToString());

            return result.ToArray();
        }

        public static string[] GetImagesUri(string source)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(source);
            HtmlNode nodes = document.DocumentNode.SelectNodes("//div[@id='gdt']")[0];

            List<string> uri = new List<string>();
            foreach (var div in nodes.SelectNodes("./div"))
                try
                {
                    uri.Add(div.SelectSingleNode(".//a").GetAttributeValue("href", ""));
                }
                catch { }

            return uri.ToArray();
        }

        public static string GetImagesAddress(string source)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(source);
            HtmlNode nodes = document.DocumentNode.SelectNodes("//div[@id='i1']")[0];

            return nodes.SelectSingleNode(".//div[@id='i3']//a//img").GetAttributeValue("src", "");
        }

        #endregion
    }
}
