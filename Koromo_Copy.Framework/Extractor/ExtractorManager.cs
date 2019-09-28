// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Network;
using Koromo_Copy.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Koromo_Copy.Framework.Extractor
{
    public class ExtractorException : Exception
    {
        public ExtractorException(string msg)
            : base(msg) { }
    }

    public interface IExtractorOption
    {
    }

    public abstract class ExtractorModel<T>
        where T : IExtractorOption
    {
        public static Regex ValidUrl { get; protected set; }

        public abstract Tuple<List<NetTask>, object> Extract(string url, T option);
    }

    public class ExtractorManager : ILazy<ExtractorManager>
    {
        public static Type[] Extractors =
        {
            typeof(DCInsideExtractor)
        };


    }
}
