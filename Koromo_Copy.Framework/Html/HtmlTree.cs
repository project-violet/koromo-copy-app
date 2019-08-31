// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Koromo_Copy.Framework.Html
{
    /// <summary>
    /// Truncates HTML by depth.
    /// </summary>
    public class HtmlTree
    {
        HtmlNode root_node;
        List<List<HtmlNode>> depth_map;

        public HtmlTree(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            root_node = document.DocumentNode;
        }

        /// <summary>
        /// Gets the maximum depth of the HTML tree.
        /// </summary>
        public int Height { get { return depth_map.Count - 1; } }

        /// <summary>
        /// Gets all nodes at a certain depth.
        /// </summary>
        /// <param name="i">Depth</param>
        /// <returns></returns>
        public List<HtmlNode> this[int i]
        {
            get { return depth_map[i]; }
        }

        /// <summary>
        /// Visit all nodes in the HTML tree and generate a depth map.
        /// </summary>
        /// <param name="lower_bound"></param>
        /// <param name="upper_bound"></param>
        public void BuildTree(int lower_bound = 0, int upper_bound = int.MaxValue)
        {
            depth_map = new List<List<HtmlNode>>();

            var queue = new Queue<Tuple<int, HtmlNode>>();
            var nodes = new List<HtmlNode>();
            int latest_depth = 0;

            queue.Enqueue(Tuple.Create(0, root_node));

            while (queue.Count > 0)
            {
                var e = queue.Dequeue();

                if (e.Item1 != latest_depth)
                {
                    depth_map.Add(nodes);
                    nodes = new List<HtmlNode>();
                    latest_depth = e.Item1;
                }

                if (lower_bound <= latest_depth)
                    nodes.Add(e.Item2);

                if (latest_depth < upper_bound && e.Item2.HasChildNodes)
                {
                    foreach (var node in e.Item2.ChildNodes)
                    {
                        queue.Enqueue(Tuple.Create(e.Item1 + 1, node));
                    }
                }
            }

            if (nodes.Count > 0)
                depth_map.Add(nodes);
        }
    }
}
