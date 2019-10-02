// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Koromo_Copy.Framework.Setting
{
    public class SettingModel
    {
        public class NetworkSetting
        {
            public bool TimeoutInfinite;
            public int TimeoutMillisecond;
            public int DownloadBufferSize;
            public int RetryCount;
        }

        public NetworkSetting NetworkSettings;

        public class PixivSetting
        {
            public string Id;
            public string Password;
        }

        public PixivSetting PixivSettings;

        /// <summary>
        /// Scheduler Thread Count
        /// </summary>
        public int ThreadCount;

        /// <summary>
        /// Postprocessor Scheduler Thread Count
        /// </summary>
        public int PostprocessorThreadCount;

        /// <summary>
        /// Provider Language
        /// </summary>
        public string Language;
    }

    public class Settings : ILazy<Settings>
    {
        public const string Name = "settings.json";

        public SettingModel Model { get; set; }
        public SettingModel.NetworkSetting Network { get { return Model.NetworkSettings; } }

        public Settings()
        {
            var full_path = Path.Combine(Directory.GetCurrentDirectory(), Name);
            if (File.Exists(full_path))
                Model = JsonConvert.DeserializeObject<SettingModel>(File.ReadAllText(full_path));

            if (Model == null)
            {
                var lang = Thread.CurrentThread.CurrentCulture.ToString();
                var language = "all";

                switch (lang)
                {
                    case "ko-KR":
                        language = "korean";
                        break;

                    case "ja-JP":
                        language = "japanese";
                        break;

                    case "en-US":
                        language = "english";
                        break;
                }

                Model = new SettingModel
                {
                    Language = language,
                    ThreadCount = Environment.ProcessorCount,
                    PostprocessorThreadCount = 3,

                    NetworkSettings = new SettingModel.NetworkSetting
                    {
                        TimeoutInfinite = false,
                        TimeoutMillisecond = 10000,
                        DownloadBufferSize = 131072,
                        RetryCount = 10,
                    },

                    PixivSettings = new SettingModel.PixivSetting()
                    {
                        
                    },
                };
            }

            Save();
        }

        public void Save()
        {
            var full_path = Path.Combine(Directory.GetCurrentDirectory(), Name);
            var json = JsonConvert.SerializeObject(Model, Formatting.Indented);
            using (var fs = new StreamWriter(new FileStream(full_path, FileMode.Create, FileAccess.Write)))
            {
                fs.Write(json);
            }
        }
    }
}
