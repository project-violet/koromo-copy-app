// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Koromo_Copy.Framework.Component.Hitomi
{
    public class HitomiDataAnalysis
    {
        public static List<string> GetLanguageList()
        {
            return HitomiData.Instance.tagdata_collection.language.Select(x => x.Tag).ToList();
        }

        public static List<HitomiTagdata> GetLanguageList(string startswith, bool constains = false)
        {
            List<HitomiTagdata> result = new List<HitomiTagdata>();
            foreach (var tagdata in HitomiData.Instance.tagdata_collection.language)
                if (tagdata.Tag.ToLower().Replace(' ', '_').StartsWith(startswith.ToLower()))
                { HitomiTagdata data = new HitomiTagdata(); data.Tag = tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            return result;
        }

        public static List<HitomiTagdata> GetArtistList(string startswith, bool constains = false)
        {
            List<HitomiTagdata> result = new List<HitomiTagdata>();
            if (!constains)
            {
                foreach (var tagdata in HitomiData.Instance.tagdata_collection.artist)
                    if (tagdata.Tag.ToLower().Replace(' ', '_').StartsWith(startswith.ToLower()))
                    { HitomiTagdata data = new HitomiTagdata(); data.Tag = tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            }
            else
            {
                foreach (var tagdata in HitomiData.Instance.tagdata_collection.artist)
                    if (tagdata.Tag.ToLower().Replace(' ', '_').Contains(startswith))
                    { HitomiTagdata data = new HitomiTagdata(); data.Tag = tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            }
            return result;
        }

        public static List<HitomiTagdata> GetTagList(string startswith, bool constains = false)
        {
            List<HitomiTagdata> target = new List<HitomiTagdata>();
            target.AddRange(HitomiData.Instance.tagdata_collection.female);
            target.AddRange(HitomiData.Instance.tagdata_collection.male);
            target.AddRange(HitomiData.Instance.tagdata_collection.tag);
            target.Sort((a, b) => b.Count.CompareTo(a.Count));
            List<HitomiTagdata> result = new List<HitomiTagdata>();
            if (!constains)
            {
                foreach (var tagdata in target)
                    if (tagdata.Tag.ToLower().Replace(' ', '_').StartsWith(startswith.ToLower()))
                    { HitomiTagdata data = new HitomiTagdata(); data.Tag = tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            }
            else
            {
                foreach (var tagdata in target)
                    if (tagdata.Tag.ToLower().Replace(' ', '_').Contains(startswith))
                    { HitomiTagdata data = new HitomiTagdata(); data.Tag = tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            }
            return result;
        }

        public static List<HitomiTagdata> GetGroupList(string startswith, bool constains = false)
        {
            List<HitomiTagdata> result = new List<HitomiTagdata>();
            if (!constains)
            {
                foreach (var tagdata in HitomiData.Instance.tagdata_collection.group)
                    if (tagdata.Tag.ToLower().Replace(' ', '_').StartsWith(startswith.ToLower()))
                    { HitomiTagdata data = new HitomiTagdata(); data.Tag = tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            }
            else
            {
                foreach (var tagdata in HitomiData.Instance.tagdata_collection.group)
                    if (tagdata.Tag.ToLower().Replace(' ', '_').Contains(startswith.ToLower()))
                    { HitomiTagdata data = new HitomiTagdata(); data.Tag = tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            }
            return result;
        }

        public static List<HitomiTagdata> GetSeriesList(string startswith, bool constains = false)
        {
            List<HitomiTagdata> result = new List<HitomiTagdata>();
            if (!constains)
            {
                foreach (var tagdata in HitomiData.Instance.tagdata_collection.series)
                    if (tagdata.Tag.ToLower().Replace(' ', '_').StartsWith(startswith.ToLower()))
                    { HitomiTagdata data = new HitomiTagdata(); data.Tag = tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            }
            else
            {
                foreach (var tagdata in HitomiData.Instance.tagdata_collection.series)
                    if (tagdata.Tag.ToLower().Replace(' ', '_').Contains(startswith.ToLower()))
                    { HitomiTagdata data = new HitomiTagdata(); data.Tag = tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            }
            return result;
        }

        public static List<HitomiTagdata> GetCharacterList(string startswith, bool constains = false)
        {
            List<HitomiTagdata> result = new List<HitomiTagdata>();
            if (!constains)
            {
                foreach (var tagdata in HitomiData.Instance.tagdata_collection.character)
                    if (tagdata.Tag.ToLower().Replace(' ', '_').StartsWith(startswith.ToLower()))
                    { HitomiTagdata data = new HitomiTagdata(); data.Tag = tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            }
            else
            {
                foreach (var tagdata in HitomiData.Instance.tagdata_collection.character)
                    if (tagdata.Tag.ToLower().Replace(' ', '_').Contains(startswith.ToLower()))
                    { HitomiTagdata data = new HitomiTagdata(); data.Tag = tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            }
            return result;
        }

        public static List<HitomiTagdata> GetTypeList(string startswith)
        {
            List<HitomiTagdata> result = new List<HitomiTagdata>();
            foreach (var tagdata in HitomiData.Instance.tagdata_collection.type)
                if (tagdata.Tag.ToLower().Replace(' ', '_').StartsWith(startswith.ToLower()))
                { HitomiTagdata data = new HitomiTagdata(); data.Tag = tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            return result;
        }

        public static List<HitomiTagdata> GetTotalList(string contains)
        {
            //if (Settings.Instance.Hitomi.UsingFuzzy) return GetTotalListFuzzy(contains);
            List<HitomiTagdata> result = new List<HitomiTagdata>();
            List<HitomiTagdata> target = new List<HitomiTagdata>();
            target.AddRange(HitomiData.Instance.tagdata_collection.female);
            target.AddRange(HitomiData.Instance.tagdata_collection.male);
            contains = contains.ToLower();
            foreach (var tagdata in HitomiData.Instance.tagdata_collection.artist)
                if (tagdata.Tag.ToLower().Replace(' ', '_').Contains(contains))
                { HitomiTagdata data = new HitomiTagdata(); data.Tag = "artist:" + tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            foreach (var tagdata in HitomiData.Instance.tagdata_collection.group)
                if (tagdata.Tag.ToLower().Replace(' ', '_').Contains(contains))
                { HitomiTagdata data = new HitomiTagdata(); data.Tag = "group:" + tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            foreach (var tagdata in HitomiData.Instance.tagdata_collection.series)
                if (tagdata.Tag.ToLower().Replace(' ', '_').Contains(contains))
                { HitomiTagdata data = new HitomiTagdata(); data.Tag = "series:" + tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            foreach (var tagdata in HitomiData.Instance.tagdata_collection.character)
                if (tagdata.Tag.ToLower().Replace(' ', '_').Contains(contains))
                { HitomiTagdata data = new HitomiTagdata(); data.Tag = "character:" + tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            foreach (var tagdata in HitomiData.Instance.tagdata_collection.type)
                if (tagdata.Tag.ToLower().Replace(' ', '_').Contains(contains))
                { HitomiTagdata data = new HitomiTagdata(); data.Tag = "type:" + tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            foreach (var tagdata in HitomiData.Instance.tagdata_collection.language)
                if (tagdata.Tag.ToLower().Replace(' ', '_').Contains(contains))
                { HitomiTagdata data = new HitomiTagdata(); data.Tag = "lang:" + tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            foreach (var tagdata in target)
                if (tagdata.Tag.ToLower().Replace(' ', '_').Contains(contains))
                { HitomiTagdata data = new HitomiTagdata(); data.Tag = tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            foreach (var tagdata in HitomiData.Instance.tagdata_collection.tag)
                if (tagdata.Tag.ToLower().Replace(' ', '_').Contains(contains))
                { HitomiTagdata data = new HitomiTagdata(); data.Tag = "tag:" + tagdata.Tag.ToLower().Replace(' ', '_'); data.Count = tagdata.Count; result.Add(data); }
            result.Sort((a, b) => b.Count.CompareTo(a.Count));
            return result;
        }

        //public static List<HitomiTagdata> GetTotalListFuzzy(string search)
        //{
        //    List<HitomiTagdata> result = new List<HitomiTagdata>();
        //    List<HitomiTagdata> target = new List<HitomiTagdata>();
        //    target.AddRange(HitomiData.Instance.tagdata_collection.female);
        //    target.AddRange(HitomiData.Instance.tagdata_collection.male);
        //    search = search.ToLower();
        //    foreach (var tagdata in HitomiData.Instance.tagdata_collection.artist)
        //    {
        //        HitomiTagdata data = new HitomiTagdata(); data.Tag = "artist:" + tagdata.Tag.ToLower().Replace(' ', '_');
        //        data.Count = -Strings.ComputeLevenshteinDistance(search, tagdata.Tag.ToLower().Replace(' ', '_')); result.Add(data);
        //    }
        //    foreach (var tagdata in HitomiData.Instance.tagdata_collection.group)
        //    {
        //        HitomiTagdata data = new HitomiTagdata(); data.Tag = "group:" + tagdata.Tag.ToLower().Replace(' ', '_');
        //        data.Count = -Strings.ComputeLevenshteinDistance(search, tagdata.Tag.ToLower().Replace(' ', '_')); result.Add(data);
        //    }
        //    foreach (var tagdata in HitomiData.Instance.tagdata_collection.series)
        //    {
        //        HitomiTagdata data = new HitomiTagdata(); data.Tag = "series:" + tagdata.Tag.ToLower().Replace(' ', '_');
        //        data.Count = -Strings.ComputeLevenshteinDistance(search, tagdata.Tag.ToLower().Replace(' ', '_')); result.Add(data);
        //    }
        //    foreach (var tagdata in HitomiData.Instance.tagdata_collection.character)
        //    {
        //        HitomiTagdata data = new HitomiTagdata(); data.Tag = "character:" + tagdata.Tag.ToLower().Replace(' ', '_');
        //        data.Count = -Strings.ComputeLevenshteinDistance(search, tagdata.Tag.ToLower().Replace(' ', '_')); result.Add(data);
        //    }
        //    foreach (var tagdata in HitomiData.Instance.tagdata_collection.type)
        //    {
        //        HitomiTagdata data = new HitomiTagdata(); data.Tag = "type:" + tagdata.Tag.ToLower().Replace(' ', '_');
        //        data.Count = -Strings.ComputeLevenshteinDistance(search, tagdata.Tag.ToLower().Replace(' ', '_')); result.Add(data);
        //    }
        //    foreach (var tagdata in target)
        //    {
        //        HitomiTagdata data = new HitomiTagdata(); data.Tag = tagdata.Tag.ToLower().Replace(' ', '_');
        //        data.Count = -Strings.ComputeLevenshteinDistance(search, tagdata.Tag.ToLower().Replace(' ', '_')); result.Add(data);
        //    }
        //    foreach (var tagdata in HitomiData.Instance.tagdata_collection.tag)
        //    {
        //        HitomiTagdata data = new HitomiTagdata(); data.Tag = "tag:" + tagdata.Tag.ToLower().Replace(' ', '_');
        //        data.Count = -Strings.ComputeLevenshteinDistance(search, tagdata.Tag.ToLower().Replace(' ', '_')); result.Add(data);
        //    }
        //    result.Sort((a, b) => b.Count.CompareTo(a.Count));
        //    return result;
        //}
        //
        //public class CompareMetadata : IComparer<HitomiDataMetadata>
        //{
        //    public int Compare(HitomiDataMetadata x, HitomiDataMetadata y)
        //    {
        //        return y.ID.CompareTo(x.ID);
        //    }
        //}
        //
        //public static HitomiDataMetadata? GetMetadataFromMagic(string magic)
        //{
        //    HitomiDataMetadata tmp = new HitomiDataMetadata() { ID = Convert.ToInt32(magic) };
        //    var pos = HitomiData.Instance.metadata_collection.BinarySearch(tmp, new CompareMetadata());
        //    if (pos < 0) return null;
        //    return HitomiData.Instance.metadata_collection[pos];
        //}
    }
}
