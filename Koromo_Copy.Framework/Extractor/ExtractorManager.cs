// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Network;
using Koromo_Copy.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        private static string crop(string pp)
        {
            var builder = new StringBuilder();

            foreach (var ch in pp)
            {
                if (char.IsUpper(ch))
                {
                    if (builder.Length != 0)
                        builder.Append("_");
                    builder.Append(char.ToLower(ch));
                }
                else
                    builder.Append(ch);
            }

            return builder.ToString();
        }

        private string check_getter(string pp)
        {
            var cc = crop(pp);
            if (Format.ContainsKey(cc))
                return Format[cc];
            return null;
        }

        private void check_setter(string pp, string value)
        {
            var cc = crop(pp);
            if (Format.ContainsKey(cc))
                Format[cc] = value;
            else
                Format.Add(cc, value);
        }

        public string Title { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string OriginalTitle { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string Id { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string Author { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string EnglishAuthor { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string Artist { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string Group { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string Search { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string UploadDate { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string Uploader { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string UploaderId { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string Character { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string Series { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string SeasonNumber { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string Season { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string Episode { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string EpisodeNumber { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string Extension { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string Url { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string License { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
        public string Genre { get { return check_getter(MethodBase.GetCurrentMethod().Name); } set { check_setter(MethodBase.GetCurrentMethod().Name, value); } }
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
