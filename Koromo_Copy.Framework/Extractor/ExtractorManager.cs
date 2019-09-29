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

        public static IExtractorOption RecommendOption(string url)
            => throw new NotImplementedException();
        public static Tuple<List<NetTask>, object> Extract(string url, IExtractorOption option)
            => throw new NotImplementedException();
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
