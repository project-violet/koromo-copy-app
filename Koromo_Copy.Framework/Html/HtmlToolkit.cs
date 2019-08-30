// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Koromo_Copy.Framework.Html
{
    /// <summary>
    /// Parse extended xpaths that parse HTML.
    /// </summary>
    public class HtmlToolkit
    {
        string pattern;
        HtmlNode root;

        /// <summary>
        /// Create new instance of html-toolkit.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="root_node">The top node of the location to search for the xpath.</param>
        /// <returns></returns>
        public static HtmlToolkit Create(string pattern, HtmlNode root_node)
            => new HtmlToolkit(pattern, root_node);
        public HtmlToolkit(string pattern, HtmlNode root_node)
        {
            this.pattern = pattern;
            root = root_node;
            parse_pattern();
        }

        public List<string> Result { get; private set; }

        private void parse_pattern()
        {
            // xpath, options ...
            Result = new List<string>();


        }
    }
}
