// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using HtmlAgilityPack;
using Koromo_Copy.Framework.CL;
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

namespace Koromo_Copy.Framework.Extractor
{
    public class HitomiArticle : EHentaiArticle
    {
        public string Magic { get; set; }
        public string[] Tags { get; set; }
    }

    public class HitomiExtractorOption : IExtractorOption
    {
        [CommandLine("--real-filename", CommandType.OPTION, Info = "Use the real file name that is provided from hitomi.la.")]
        public bool RealFilename;

        public override void CLParse(ref IExtractorOption model, string[] args)
        {
            model = CommandLineParser.Parse(model as HitomiExtractorOption, args);
        }
    }

    public class HitomiExtractor : ExtractorModel
    {
        public HitomiExtractor()
        {
            HostName = new Regex(@"hitomi\.la");
            ValidUrl = new Regex(@"^https?://hitomi\.la/(?:galleries|reader|cg|gamecg|doujinshi|manga)/(?<title>.*?)?\-?(?<id>\d+)(?:\.html|js)?$");
        }

        public override IExtractorOption RecommendOption(string url)
        {
            return new HitomiExtractorOption { Type = HitomiExtractorOption.ExtractorType.Images };
        }

        public override string RecommendFormat(IExtractorOption option)
        {
            return "%(extractor)s/%(artist)s/[%(id)s] %(title)s/%(file)s.%(ext)s";
        }

        public override (List<NetTask>, ExtractedInfo) Extract(string url, IExtractorOption option = null)
        {
            var match = ValidUrl.Match(url).Groups;

            if (option == null)
                option = RecommendOption(url);

            if (option.Type == ExtractorType.Images)
            {
                var id = match["id"].Value;

                // Handle Redirect
                var string3 = NetTools.DownloadString(url);
            RETRY_DOWNLOAD1:
                if (string.IsNullOrEmpty(string3))
                    return (null, null);
                if (string3.ToHtmlNode().SelectSingleNode("//title").InnerText == "Redirect")
                {
                    id = string3.ToHtmlNode().SelectSingleNode("//a").GetAttributeValue("href", "").Split('/', '-').Last().Split('.')[0];
                    string3 = NetTools.DownloadString(string3.ToHtmlNode().SelectSingleNode("//a").GetAttributeValue("href", ""));
                    goto RETRY_DOWNLOAD1;
                }

                var sinfo = new ExtractedInfo.WorksComic();
                var imgs_url = $"https://ltn.hitomi.la/galleries/{id}.js";
                option.PageReadCallback?.Invoke($"https://ltn.hitomi.la/galleryblock/{id}.html");
                option.PageReadCallback?.Invoke(url);
                option.PageReadCallback?.Invoke(imgs_url);
                var urls = new List<string> {
                    $"https://ltn.hitomi.la/galleryblock/{id}.html", 
                    imgs_url };

                var strings = NetTools.DownloadStrings(urls);

                if (string.IsNullOrEmpty(strings[0]) || string.IsNullOrEmpty(strings[1]))
                    return (null, null);

                var data1 = ParseGalleryBlock(strings[0]);
                var imgs = strings[1];

                var string2 = NetTools.DownloadString($"https://hitomi.la{data1.Magic}");
                if (string.IsNullOrEmpty(string2))
                    return (null, null);
                var data2 = ParseGallery(string2);

                option.SimpleInfoCallback?.Invoke($"[{id}] {data1.Title}");

                // download.js
                var number_of_frontends = 3;
                var subdomain = Convert.ToChar(97 + (Convert.ToInt32(id.Last()) % number_of_frontends));
                if (id.Last() == '0')
                    subdomain = 'a';

                var arr = JToken.Parse(imgs.Substring(imgs.IndexOf('=') + 1))["files"];
                var img_urls = new List<string>();
                foreach (var obj in (JArray)arr)
                {
                    var hash = obj.Value<string>("hash");
                    if (obj.Value<int>("haswebp") == 0 || hash == null)
                    {
                        //img_urls.Add($"https://{subdomain}a.hitomi.la/galleries/{id}/{obj.Value<string>("name")}");
                        var postfix = hash.Substring(hash.Length - 3);
                        img_urls.Add($"https://{subdomain}a.hitomi.la/images/{postfix[2]}/{postfix[0]}{postfix[1]}/{hash}.{obj.Value<string>("name").Split('.').Last()}");
                    }
                    else if (hash == "")
                        img_urls.Add($"https://{subdomain}a.hitomi.la/webp/{obj.Value<string>("name")}.webp");
                    else if (hash.Length < 3)
                        img_urls.Add($"https://{subdomain}a.hitomi.la/webp/{hash}.webp");
                    else
                    {
                        var postfix = hash.Substring(hash.Length - 3);
                        img_urls.Add($"https://{subdomain}a.hitomi.la/webp/{postfix[2]}/{postfix[0]}{postfix[1]}/{hash}.webp");
                    }
                }

                var result = new List<NetTask>();
                var ordering = 1;
                foreach (var img in img_urls)
                {
                    var filename = Path.GetFileNameWithoutExtension(img.Split('/').Last());
                    if (!(option as HitomiExtractorOption).RealFilename)
                        filename = ordering++.ToString("000");

                    var task = NetTask.MakeDefault(img);
                    task.SaveFile = true;
                    task.Filename = img.Split('/').Last();
                    task.Format = new ExtractorFileNameFormat
                    {
                        Title = data1.Title,
                        Id = id,
                        Language = data1.Language,
                        UploadDate = data1.Posted,
                        FilenameWithoutExtension = filename,
                        Extension = Path.GetExtension(img.Split('/').Last()).Replace(".", "")
                    };

                    if (data1.artist != null)
                        task.Format.Artist = data1.artist[0];
                    else
                        task.Format.Artist = "NA";

                    if (data1.parody != null)
                        task.Format.Series = data1.parody[0];
                    else
                        task.Format.Series = "NA";

                    if (data2.group != null)
                        task.Format.Group = data2.group[0];
                    else
                        task.Format.Group = "NA";

                    if (data2.character != null)
                        task.Format.Character = data2.character[0];
                    else
                        task.Format.Character = "NA";

                    if (task.Format.Artist == "NA" && task.Format.Group != "NA")
                        task.Format.Artist = task.Format.Group;

                    result.Add(task);
                }

                option.ThumbnailCallback?.Invoke(result[0]);

                sinfo.Thumbnail = result[0];
                sinfo.URL = url;
                sinfo.Title = data1.Title;
                sinfo.Author = data1.artist?.ToArray();
                sinfo.AuthorGroup = data2.group?.ToArray();
                sinfo.ShortInfo = $"[{id}] {data1.Title}";
                sinfo.Tags = data1.Tags?.ToArray();
                sinfo.Characters = data2.character?.ToArray();
                sinfo.Language = data1.Language;
                sinfo.Parodies = data1.parody?.ToArray();

                result.ForEach(task => task.Format.Extractor = GetType().Name.Replace("Extractor", ""));
                return (result, new ExtractedInfo { Info = sinfo, Type = ExtractedInfo.ExtractedType.WorksComic });
            }

            return (null, null);
        }

        static public HitomiArticle ParseGalleryBlock(string source)
        {
            HitomiArticle article = new HitomiArticle();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(source);
            HtmlNode nodes = document.DocumentNode.SelectNodes("/div")[0];

            article.Magic = nodes.SelectSingleNode("./a").GetAttributeValue("href", "");
            try { article.Thumbnail = nodes.SelectSingleNode("./a//img").GetAttributeValue("data-src", "").Substring("//tn.hitomi.la/".Length).Replace("smallbig", "big"); }
            catch
            { article.Thumbnail = nodes.SelectSingleNode("./a//img").GetAttributeValue("src", "").Substring("//tn.hitomi.la/".Length); }
            article.Title = nodes.SelectSingleNode("./h1").InnerText;

            try { article.artist = nodes.SelectNodes(".//div[@class='artist-list']//li").Select(node => node.SelectSingleNode("./a").InnerText).ToArray(); }
            catch { article.artist = new[] { "NA" }; }

            var contents = nodes.SelectSingleNode("./div[2]/table");
            try { article.parody = contents.SelectNodes("./tr[1]/td[2]/ul/li").Select(node => node.SelectSingleNode(".//a").InnerText).ToArray(); } catch { }
            article.Type = contents.SelectSingleNode("./tr[2]/td[2]/a").InnerText;
            try { article.Language = LegalizeLanguage(contents.SelectSingleNode("./tr[3]/td[2]/a").InnerText); } catch { }
            try { article.Tags = contents.SelectNodes("./tr[4]/td[2]/ul/li").Select(node => LegalizeTag(node.SelectSingleNode(".//a").InnerText)).ToArray(); } catch { }

            article.Posted = nodes.SelectSingleNode("./div[2]/p").InnerText;

            return article;
        }

        static public HitomiArticle ParseGallery(string source)
        {
            HitomiArticle article = new HitomiArticle();

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(source);
            HtmlNode nodes = document.DocumentNode.SelectSingleNode("//div[@class='content']");

            article.Magic = nodes.SelectSingleNode(".//h1/a").GetAttributeValue("href", "").Split('/')[2].Split('.')[0];

            foreach (var tr in document.DocumentNode.SelectNodes("//div[@class='gallery-info']/table/tr").ToList())
            {
                var tt = tr.SelectSingleNode("./td").InnerText.ToLower().Trim();
                if (tt == "group")
                {
                    article.group = tr.SelectNodes(".//a")?.Select(x => x.InnerText.Trim()).ToArray();
                }
                else if (tt == "characters")
                {
                    article.character = tr.SelectNodes(".//a")?.Select(x => x.InnerText.Trim()).ToArray();
                }
            }

            return article;
        }

        public static string LegalizeTag(string tag)
        {
            if (tag.Trim().EndsWith("♀")) return "female:" + tag.Trim('♀').Trim();
            if (tag.Trim().EndsWith("♂")) return "male:" + tag.Trim('♂').Trim();
            return tag.Trim();
        }

        public static string LegalizeLanguage(string lang)
        {
            switch (lang)
            {
                case "모든 언어": return "all";
                case "한국어": return "korean";
                case "N/A": return "na";
                case "日本語": return "japanese";
                case "English": return "english";
                case "Español": return "spanish";
                case "ไทย": return "thai";
                case "Deutsch": return "german";
                case "中文": return "chinese";
                case "Português": return "portuguese";
                case "Français": return "french";
                case "Tagalog": return "tagalog";
                case "Русский": return "russian";
                case "Italiano": return "italian";
                case "polski": return "polish";
                case "tiếng việt": return "vietnamese";
                case "magyar": return "hungarian";
                case "Čeština": return "czech";
                case "Bahasa Indonesia": return "indonesian";
                case "العربية": return "arabic";
            }

            return lang;
        }

        public static string DeLegalizeLanguage(string lang)
        {
            switch (lang)
            {
                case "all": return "모든 언어";
                case "korean": return "한국어";
                case "n/a": return "N/A";
                case "japanese": return "日本語";
                case "english": return "English";
                case "spanish": return "Español";
                case "thai": return "ไทย";
                case "german": return "Deutsch";
                case "chinese": return "中文";
                case "portuguese": return "Português";
                case "french": return "Français";
                case "tagalog": return "Tagalog";
                case "russian": return "Русский";
                case "italian": return "Italiano";
                case "polish": return "polski";
                case "vietnamese": return "tiếng việt";
                case "hungarian": return "magyar";
                case "czech": return "Čeština";
                case "indonesian": return "Bahasa Indonesia";
                case "arabic": return "العربية";
            }

            return lang;
        }
    }
}
