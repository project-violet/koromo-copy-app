// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Crypto;
using Koromo_Copy.Framework.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Koromo_Copy.Framework.Cache
{
    public class CacheManager : ILazy<CacheManager>
    {
        public string CacheDirectory { get; set; }

        public CacheManager()
        {
            CacheDirectory = Path.Combine(AppProvider.ApplicationPath, "Cache");
            if (!Directory.Exists(CacheDirectory))
                Directory.CreateDirectory(CacheDirectory);
        }

        public void Append<T>(string cache_name, T cache_object) where T : new()
            => File.WriteAllText(Path.Combine(CacheDirectory, cache_name.GetHashMD5()), Log.Logs.SerializeObject(cache_object));

        public bool Exists(string cache_name)
            => File.Exists(Path.Combine(CacheDirectory, cache_name.GetHashMD5()));

        public string Find(string cache_name)
            => File.ReadAllText(Path.Combine(CacheDirectory, cache_name.GetHashMD5()));
    }
}
