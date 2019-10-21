// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Setting;
using Koromo_Copy.Framework.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Koromo_Copy.Framework.Component.Hitomi
{
    [MessagePackObject]
    public class HitomiIndexModel
    {
        [Key(0)]
        public string[] Artists;
        [Key(1)]
        public string[] Groups;
        [Key(2)]
        public string[] Series;
        [Key(3)]
        public string[] Characters;
        [Key(4)]
        public string[] Languages;
        [Key(5)]
        public string[] Types;
        [Key(6)]
        public string[] Tags;
    }

    [MessagePackObject]
    public struct HitomiIndexMetadata
    {
        [Key(0)]
        public int[] Artists { get; set; }
        [Key(1)]
        public int[] Groups { get; set; }
        [Key(2)]
        public int[] Parodies { get; set; }
        [Key(3)]
        public int[] Tags { get; set; }
        [Key(4)]
        public int[] Characters { get; set; }
        [Key(5)]
        public int Language { get; set; }
        [Key(6)]
        public string Name { get; set; }
        [Key(7)]
        public int Type { get; set; }
        [Key(8)]
        public int ID { get; set; }
    }

    [MessagePackObject]
    public class HitomiIndexDataModel
    {
        [Key(0)]
        public HitomiIndexModel index;
        [Key(1)]
        public List<HitomiIndexMetadata> metadata;
    }

    public class HitomiData : ILazy<HitomiData>
    {
        public HitomiTagdataCollection tagdata_collection = new HitomiTagdataCollection();
        public List<HitomiIndexMetadata> metadata_collection = new List<HitomiIndexMetadata>();
        public HitomiIndexModel index;

        public void Load()
        {
            if (CheckMetadataExist())
            {
                var re = MessagePackSerializer.Deserialize<HitomiIndexDataModel>(File.ReadAllBytes(Path.Combine(AppProvider.ApplicationPath, "index-metadata.json")));
                metadata_collection = re.metadata;
                index = re.index;
                SortMetadata();
            }
        }

        public bool CheckMetadataExist()
        {
            return File.Exists(Path.Combine(AppProvider.ApplicationPath, "index-metadata.json"));
        }

        public void SortMetadata()
        {
            metadata_collection.Sort((a, b) => b.ID.CompareTo(a.ID));
        }

        public DateTime DateTimeMetadata()
        {
            return File.GetLastWriteTime(Path.Combine(AppProvider.ApplicationPath, "index-metadata.json"));
        }

        public void OptimizeMetadata()
        {
            List<HitomiIndexMetadata> tmeta = new List<HitomiIndexMetadata>();
            int m = metadata_collection.Count;
            for (int i = 0; i < m; i++)
            {
                string lang = "n/a"; ;
                if (metadata_collection[i].Language >= 0) lang = index.Languages[metadata_collection[i].Language];
                if (Settings.Instance.Model.Language != "all" &&
                    Settings.Instance.Model.Language != lang)
                    continue;
                tmeta.Add(metadata_collection[i]);
            }
            metadata_collection.Clear();
            metadata_collection = tmeta;
        }

#region TagData

        public void SortTagdata()
        {
            tagdata_collection.artist.Sort((a, b) => b.Count.CompareTo(a.Count));
            tagdata_collection.tag.Sort((a, b) => b.Count.CompareTo(a.Count));
            tagdata_collection.female.Sort((a, b) => b.Count.CompareTo(a.Count));
            tagdata_collection.male.Sort((a, b) => b.Count.CompareTo(a.Count));
            tagdata_collection.group.Sort((a, b) => b.Count.CompareTo(a.Count));
            tagdata_collection.character.Sort((a, b) => b.Count.CompareTo(a.Count));
            tagdata_collection.series.Sort((a, b) => b.Count.CompareTo(a.Count));
            tagdata_collection.type.Sort((a, b) => b.Count.CompareTo(a.Count));
            tagdata_collection.language.Sort((a, b) => b.Count.CompareTo(a.Count));
        }

        private void Add(Dictionary<string, int> dic, string key)
        {
            if (dic.ContainsKey(key))
                dic[key] += 1;
            else
                dic.Add(key, 1);
        }

        public void RebuildTagData()
        {
            tagdata_collection.artist?.Clear();
            tagdata_collection.tag?.Clear();
            tagdata_collection.female?.Clear();
            tagdata_collection.male?.Clear();
            tagdata_collection.group?.Clear();
            tagdata_collection.character?.Clear();
            tagdata_collection.series?.Clear();
            tagdata_collection.type?.Clear();
            tagdata_collection.language?.Clear();

            Dictionary<string, int> artist = new Dictionary<string, int>();
            Dictionary<string, int> tag = new Dictionary<string, int>();
            Dictionary<string, int> female = new Dictionary<string, int>();
            Dictionary<string, int> male = new Dictionary<string, int>();
            Dictionary<string, int> group = new Dictionary<string, int>();
            Dictionary<string, int> character = new Dictionary<string, int>();
            Dictionary<string, int> series = new Dictionary<string, int>();
            Dictionary<string, int> type = new Dictionary<string, int>();
            Dictionary<string, int> language = new Dictionary<string, int>();

            foreach (var metadata in metadata_collection)
            {
                string lang = "n/a";
                if (metadata.Language >= 0) lang = index.Languages[metadata.Language];
                Add(language, lang);
                if (Settings.Instance.Model.Language != "all" &&
                    Settings.Instance.Model.Language != lang) continue;
                if (metadata.Artists != null) metadata.Artists.ToList().ForEach(x => Add(artist, index.Artists[x]));
                if (metadata.Tags != null) metadata.Tags.ToList().ForEach(x => { if (index.Tags[x].StartsWith("female:")) Add(female, index.Tags[x]); else if (index.Tags[x].StartsWith("male:")) Add(male, index.Tags[x]); else Add(tag, index.Tags[x]); });
                if (metadata.Groups != null) metadata.Groups.ToList().ForEach(x => Add(group, index.Groups[x]));
                if (metadata.Characters != null) metadata.Characters.ToList().ForEach(x => Add(character, index.Characters[x]));
                if (metadata.Parodies != null) metadata.Parodies.ToList().ForEach(x => Add(series, index.Series[x]));
                if (metadata.Type >= 0) Add(type, index.Types[metadata.Type]);
            }

            tagdata_collection.artist = artist.Select(x => new HitomiTagdata() { Tag = x.Key, Count = x.Value }).ToList();
            tagdata_collection.tag = tag.Select(x => new HitomiTagdata() { Tag = x.Key, Count = x.Value }).ToList();
            tagdata_collection.female = female.Select(x => new HitomiTagdata() { Tag = x.Key, Count = x.Value }).ToList();
            tagdata_collection.male = male.Select(x => new HitomiTagdata() { Tag = x.Key, Count = x.Value }).ToList();
            tagdata_collection.group = group.Select(x => new HitomiTagdata() { Tag = x.Key, Count = x.Value }).ToList();
            tagdata_collection.character = character.Select(x => new HitomiTagdata() { Tag = x.Key, Count = x.Value }).ToList();
            tagdata_collection.series = series.Select(x => new HitomiTagdata() { Tag = x.Key, Count = x.Value }).ToList();
            tagdata_collection.type = type.Select(x => new HitomiTagdata() { Tag = x.Key, Count = x.Value }).ToList();
            tagdata_collection.language = language.Select(x => new HitomiTagdata() { Tag = x.Key, Count = x.Value }).ToList();

            SortTagdata();
        }

        #endregion

    }
}
