// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Network;
using Koromo_Copy.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Koromo_Copy.Framework.Extractor
{
    public class ExtractorException : Exception
    {
        public ExtractorException(string msg)
            : base(msg) { }
    }

    public class IExtractorOption
    {
        public bool ExtractInformation { get; set; }
    }

    public abstract class ExtractorModel
    {
        public Regex HostName { get; protected set; }
        public Regex ValidUrl { get; protected set; }
        public string ExtractorInfo { get; protected set; }

        public abstract IExtractorOption RecommendOption(string url);
        public abstract Tuple<List<NetTask>, object> Extract(string url, IExtractorOption option);
    }

    public class ExtractorFileNameFormat
    {
        public Dictionary<string, string> Format { get; set; }
            = new Dictionary<string, string>();

        public string Title { get { if (Format.ContainsKey("title")) return Format["title"]; return null; } set { Format.Add("title", value); } }
        public string OriginalTitle { get { if (Format.ContainsKey("original_title")) return Format["original_title"]; return null; } set { Format.Add("original_title", value); } }
        public string Id { get { if (Format.ContainsKey("id")) return Format["id"]; return null; } set { Format.Add("id", value); } }
        public string Author { get { if (Format.ContainsKey("author")) return Format["author"]; return null; } set { Format.Add("author", value); } }
        public string EnglishAuthor { get { if (Format.ContainsKey("eng_author")) return Format["eng_author"]; return null; } set { Format.Add("eng_author", value); } }
        public string Artist { get { if (Format.ContainsKey("artist")) return Format["artist"]; return null; } set { Format.Add("artist", value); } }
        public string Group { get { if (Format.ContainsKey("group")) return Format["group"]; return null; } set { Format.Add("group", value); } }
        public string Search { get { if (Format.ContainsKey("search")) return Format["search"]; return null; } set { Format.Add("search", value); } }
        public string UploadDate { get { if (Format.ContainsKey("upload_date")) return Format["upload_date"]; return null; } set { Format.Add("upload_date", value); } }
        public string Uploader { get { if (Format.ContainsKey("uploader")) return Format["uploader"]; return null; } set { Format.Add("uploader", value); } }
        public string UploaderId { get { if (Format.ContainsKey("uploader_id")) return Format["uploader_id"]; return null; } set { Format.Add("uploader_id", value); } }
        public string Character { get { if (Format.ContainsKey("character")) return Format["character"]; return null; } set { Format.Add("character", value); } }
        public string Series { get { if (Format.ContainsKey("series")) return Format["series"]; return null; } set { Format.Add("series", value); } }
        public string SeasonNumber { get { if (Format.ContainsKey("season_number")) return Format["season_number"]; return null; } set { Format.Add("season_number", value); } }
        public string Season { get { if (Format.ContainsKey("season")) return Format["season"]; return null; } set { Format.Add("season", value); } }
        public string Episode { get { if (Format.ContainsKey("episode")) return Format["episode"]; return null; } set { Format.Add("episode", value); } }
        public string EpisodeNumber { get { if (Format.ContainsKey("episode_number")) return Format["episode_number"]; return null; } set { Format.Add("episode_number", value); } }
        public string Extension { get { if (Format.ContainsKey("ext")) return Format["ext"]; return null; } set { Format.Add("ext", value); } }
        public string Url { get { if (Format.ContainsKey("url")) return Format["url"]; return null; } set { Format.Add("url", value); } }
        public string License { get { if (Format.ContainsKey("license")) return Format["license"]; return null; } set { Format.Add("license", value); } }
        public string Genre { get { if (Format.ContainsKey("genre")) return Format["genre"]; return null; } set { Format.Add("genre", value); } }
    }

    public class ExtractorManager : ILazy<ExtractorManager>
    {
        public static ExtractorModel[] Extractors =
        {
            new DCInsideExtractor(),
            new PixivExtractor(),
            new GelbooruExtractor(),
            new NaverExtractor(),
            new EHentaiExtractor()
        };

        public ExtractorModel GetExtractor(string url)
        {
            foreach (var em in Extractors)
            {
                if (em.ValidUrl.IsMatch(url))
                    return em;
            }
            return null;
        }

        public ExtractorModel GetExtractorFromHostName(string url)
        {
            foreach (var em in Extractors)
            {
                if (em.HostName.IsMatch(url))
                    return em;
            }
            return null;
        }
    }
}
