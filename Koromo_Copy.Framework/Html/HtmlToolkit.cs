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
            var tokens = split_token(pattern);

            Result = new List<string>();
        }

        /// <summary>
        /// Tokenize e-xpath.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private List<string> split_token(string str)
        {
            var result = new List<string>();
            for (int i = 0; i < str.Length; i++)
            {
                var builder = new StringBuilder();
                bool skip = false;
                for (; i < str.Length; i++)
                {
                    if (str[i] == ',' && skip == false)
                    {
                        result.Add(builder.ToString());
                        break;
                    }
                    if (str[i] == '[')
                        skip = true;
                    else if (str[i] == ']' && skip)
                        skip = false;
                    builder.Append(str[i]);
                }
                if (i == str.Length)
                    result.Add(builder.ToString());
            }
            return result;
        }
    }
}
