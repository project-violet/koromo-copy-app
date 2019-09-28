// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Koromo_Copy.Framework.Utils
{
    public static class Extends
    {
        public static int ToInt(this string str) => Convert.ToInt32(str);

        public static string MyText(this HtmlNode node) =>
            string.Join("", node.ChildNodes.Where(x => x.Name == "#text").Select(x => x.InnerText.Trim()));
    }
}
