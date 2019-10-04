// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.CL;
using Koromo_Copy.Framework.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using static Koromo_Copy.Framework.Extractor.IExtractorOption;

namespace Koromo_Copy.Framework.Extractor
{
    public class InstagramExtractorOption : IExtractorOption
    {
        [CommandLine("--only-images", CommandType.OPTION, Info = "Extract only images.")]
        public bool OnlyImages;

        public override void CLParse(ref IExtractorOption model, string[] args)
        {
            model = CommandLineParser.Parse(model as InstagramExtractorOption, args);
        }
    }

    class InstagramExtractor : ExtractorModel
    {
        public InstagramExtractor()
        {
            HostName = new Regex(@"www\.instagram\.com");
            ValidUrl = new Regex(@"^https?://www\.instagram\.com/(?:p\/)?(?<id>.*?)/?.*?$");
        }

        public override IExtractorOption RecommendOption(string url)
        {
            return new InstagramExtractorOption { Type = ExtractorType.Images };
        }

        public override string RecommendFormat(IExtractorOption option)
        {
            return "%(user)s (%(account)s)/%(file)s.%(ext)s";
        }

        public override Tuple<List<NetTask>, object> Extract(string url, IExtractorOption option = null)
        {
            if (option == null)
                option = RecommendOption(url);

            var html = NetTools.DownloadString(url);
            var user = InstaApi.get_user(html);
            var urls = new List<string>();
            urls.AddRange(user.FirstPost.DisplayUrls);

            var pp = user.FirstPost;
            while (pp.HasNext)
            {
                var posts = InstaApi.query_next(option as InstagramExtractorOption, InstaApi.query_hash(), user.UserId, "50", pp.EndCursor);
                urls.AddRange(posts.DisplayUrls);
                pp = posts;
            }

            var result = new List<NetTask>();
            foreach (var surl in urls)
            {
                var task = NetTask.MakeDefault(surl);
                task.SaveFile = true;

                var fn = surl.Split('?')[0].Split('/').Last();
                task.Filename = fn;
                task.Format = new ExtractorFileNameFormat
                {
                    FilenameWithoutExtension = Path.GetFileNameWithoutExtension(fn),
                    Extension = Path.GetExtension(fn).Replace(".", ""),
                    User = user.FullName,
                    Account = user.UserName
                };

                result.Add(task);
            }

            return new Tuple<List<NetTask>, object>(result, null);
        }

        public class InstaApi
        {
            public static string query_hash()
                => "58b6785bea111c67129decbe6a448951";

            public static string get_sharedData(string html)
                => Regex.Match(html, @"window\._sharedData = (.*);</script>").Value;

            public class Posts
            {
                public bool HasNext { get; set; }
                public string EndCursor { get; set; }
                public List<string> DisplayUrls { get; set; }
            }

            public class User
            {
                public string UserName { get; set; }
                public string FullName { get; set; }
                public string UserId { get; set; }
                public Posts FirstPost { get; set; }
            }

            /// <summary>
            /// Get userid from user nickname
            /// </summary>
            /// <param name="html">https://www.instagram.com/(.*?)/</param>
            /// <returns></returns>
            public static User get_user(string html)
            {
                var json = Regex.Match(html, @"window\._sharedData = (.*);</script>").Groups[1].Value;

                var user = new User
                {
                    UserId = (JToken.Parse(json)["entry_data"]["ProfilePage"] as JArray)[0]["graphql"]["user"]["id"].ToString(),
                    UserName = (JToken.Parse(json)["entry_data"]["ProfilePage"] as JArray)[0]["graphql"]["user"]["username"].ToString(),
                    FullName = (JToken.Parse(json)["entry_data"]["ProfilePage"] as JArray)[0]["graphql"]["user"]["full_name"].ToString(),
                    FirstPost = new Posts
                    {
                        HasNext = (bool)(JToken.Parse(json)["entry_data"]["ProfilePage"] as JArray)[0]["graphql"]["user"]["edge_owner_to_timeline_media"]["page_info"]["has_next_page"],
                        EndCursor = (JToken.Parse(json)["entry_data"]["ProfilePage"] as JArray)[0]["graphql"]["user"]["edge_owner_to_timeline_media"]["page_info"]["end_cursor"].ToString(),
                        DisplayUrls = new List<string>()
                    }
                };

                foreach (var post in (JToken.Parse(json)["entry_data"]["ProfilePage"] as JArray)[0]["graphql"]["user"]["edge_owner_to_timeline_media"]["edges"])
                    user.FirstPost.DisplayUrls.Add(post["node"]["thumbnail_resources"].Last["src"].ToString());

                return user;
            }

            public static string graphql_qurey(InstagramExtractorOption option, string query_hash, Dictionary<string, string> param)
            {
                var url = $"https://www.instagram.com/graphql/query/?query_hash=" + query_hash + "&variables=" +
                    HttpUtility.UrlEncode(JsonConvert.SerializeObject(param));
                option.PageReadCallback?.Invoke(url);
                return NetTools.DownloadString(url);
            }

            public static Posts query_next(InstagramExtractorOption option, string query_hash, string id, string first, string after)
            {
                var json = graphql_qurey(option, query_hash, new Dictionary<string, string> { { "id", id }, { "first", first }, { "after", after } });
                var posts = new Posts
                {
                    HasNext = (bool)JToken.Parse(json)["data"]["user"]["edge_owner_to_timeline_media"]["page_info"]["has_next_page"],
                    EndCursor = JToken.Parse(json)["data"]["user"]["edge_owner_to_timeline_media"]["page_info"]["end_cursor"].ToString(),
                    DisplayUrls = new List<string>()
                };

                foreach (var post in JToken.Parse(json)["data"]["user"]["edge_owner_to_timeline_media"]["edges"])
                {
                    if ((bool)post["node"]["is_video"] && !option.OnlyImages)
                        posts.DisplayUrls.Add(post["node"]["video_url"].ToString());
                    else
                        posts.DisplayUrls.Add(post["node"]["display_url"].ToString());
                }

                return posts;
            }
        }
    }
}
