// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Log;
using Koromo_Copy.Framework.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Koromo_Copy.Framework.Network
{
    public class ProxyList : ILazy<ProxyList>
    {
        public const string Name = "proxy.txt";

        public List<string> List { get; set; } = new List<string>();

        public ProxyList()
        {
            var full_path = Path.Combine(Directory.GetCurrentDirectory(), Name);
            if (!File.Exists(full_path))
            {
                Logs.Instance.PushError("[Proxy] 'proxy.txt' not found!");
                return;
            }
            var txt = File.ReadAllLines(full_path);
            foreach (var line in txt)
            {
                if (string.IsNullOrEmpty(line))
                    break;
                List.Add(line.Split(' ')[0].Trim());
            }
        }

        public string RandomPick()
            => List[new Random().Next(List.Count)];
    }
}
