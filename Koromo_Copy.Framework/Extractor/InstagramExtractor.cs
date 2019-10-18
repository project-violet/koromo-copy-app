// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.CL;
using Koromo_Copy.Framework.Network;
using Koromo_Copy.Framework.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using static Koromo_Copy.Framework.Extractor.IExtractorOption;

namespace Koromo_Copy.Framework.Extractor
{
    public class InstagramExtractorOption : IExtractorOption
    {
        [CommandLine("--only-images", CommandType.OPTION, Info = "Extract only images.")]
        public bool OnlyImages;
        [CommandLine("--only-thumbnail", CommandType.OPTION, Info = "Extract only thumbnails.")]
        public bool OnlyThumbnail;
        [CommandLine("--include-thumbnail", CommandType.OPTION, Info = "Include thumbnail extracting video.")]
        public bool IncludeThumbnail;
        [CommandLine("--limit-posts", CommandType.ARGUMENTS, Info = "Limit read posts count.", Help = "use --limit-posts <Number of post>")]
        public string[] LimitPosts;

        public override void CLParse(ref IExtractorOption model, string[] args)
        {
            model = CommandLineParser.Parse(model as InstagramExtractorOption, args);
        }
    }

    public class InstagramExtractor : ExtractorModel
    {
        public InstagramExtractor()
        {
            HostName = new Regex(@"www\.instagram\.com");
            ValidUrl = new Regex(@"^https?://(www\.)?instagram\.com/(?:p\/)?(?<id>.*?)/?.*?$");
            ExtractorInfo = "Instagram extactor info\r\n" +
                "   user:             Full-name.\r\n" +
                "   account:          User-name";
        }

        public override IExtractorOption RecommendOption(string url)
        {
            return new InstagramExtractorOption { Type = ExtractorType.Images };
        }

        public override string RecommendFormat(IExtractorOption option)
        {
            return "%(user)s (%(account)s)/%(file)s.%(ext)s";
        }

        public override (List<NetTask>, ExtractedInfo) Extract(string url, IExtractorOption option = null)
        {
            if (option == null)
                option = RecommendOption(url);

            var limit = int.MaxValue;
            if ((option as InstagramExtractorOption).LimitPosts != null)
                limit = (option as InstagramExtractorOption).LimitPosts[0].ToInt();

            var html = NetTools.DownloadString(url);
            var user = InstaApi.get_user(option as InstagramExtractorOption, html);
            var urls = new List<string>();
            urls.AddRange(user.FirstPost.DisplayUrls);
            option.PostStatus?.Invoke(user.FirstPost.PostCount);

            option.SimpleInfoCallback?.Invoke($"{user.FullName} ({user.UserName})");

            var count = 0;
            var pp = user.FirstPost;
            while (pp.HasNext)
            {
                if (count >= limit)
                    break;

                var posts = InstaApi.query_next(option as InstagramExtractorOption, InstaApi.posts_query_hash(), user.UserId, "50", pp.EndCursor);
                urls.AddRange(posts.DisplayUrls);
                option.PostStatus?.Invoke(posts.PostCount);
                count += 50;
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

            return (result, , new ExtractedInfo { Type = ExtractedInfo.ExtractedType.UserArtist });
        }

        public class InstaApi
        {
            public static string posts_query_hash()
                => "58b6785bea111c67129decbe6a448951";

            public static string sidecar_query_hash()
                => "865589822932d1b43dfe312121dd353a";

            public static string get_sharedData(string html)
                => Regex.Match(html, @"window\._sharedData = (.*);</script>").Value;

            public class Posts
            {
                public bool HasNext { get; set; }
                public string EndCursor { get; set; }
                public List<string> DisplayUrls { get; set; }
                public int PostCount { get; set; }
            }

            public class User
            {
                public string UserName { get; set; }
                public string FullName { get; set; }
                public string UserId { get; set; }
                public Posts FirstPost { get; set; }
                public int TotalPostsCount { get; set; }
            }

            /// <summary>
            /// Get userid from user nickname
            /// </summary>
            /// <param name="html">https://www.instagram.com/(.*?)/</param>
            /// <returns></returns>
            public static User get_user(InstagramExtractorOption option, string html)
            {
                var json = Regex.Match(html, @"window\._sharedData = (.*);</script>").Groups[1].Value;
                var juser = (JToken.Parse(json)["entry_data"]["ProfilePage"] as JArray)[0]["graphql"]["user"];

                var user = new User
                {
                    UserId = juser["id"].ToString(),
                    UserName = juser["username"].ToString(),
                    FullName = juser["full_name"].ToString(),
                    TotalPostsCount = juser["edge_owner_to_timeline_media"]["count"].ToString().ToInt(),
                    FirstPost = new Posts
                    {
                        HasNext = (bool)juser["edge_owner_to_timeline_media"]["page_info"]["has_next_page"],
                        EndCursor = juser["edge_owner_to_timeline_media"]["page_info"]["end_cursor"].ToString(),
                        DisplayUrls = new List<string>(),
                    }
                };

                option.ProgressMax?.Invoke(user.TotalPostsCount);

                option.ThumbnailCallback?.Invoke(NetTask.MakeDefault(juser["profile_pic_url_hd"].ToString()));

                foreach (var post in juser["edge_owner_to_timeline_media"]["edges"])
                {
                    user.FirstPost.PostCount++;
                    if (post["node"]["__typename"].ToString() == "GraphImage" || option.OnlyThumbnail)
                        user.FirstPost.DisplayUrls.Add(post["node"]["display_url"].ToString());
                    else
                    {
                        var short_code = post["node"]["shortcode"].ToString();
                        var json2 = graphql_qurey(option, sidecar_query_hash(), new Dictionary<string, string> { { "shortcode", short_code } });

                        if (post["node"]["__typename"].ToString() == "GraphVideo")
                        {
                            var media = JToken.Parse(json2)["data"]["shortcode_media"];
                            extract_url(media, option, user.FirstPost.DisplayUrls);
                        }
                        else
                        {
                            foreach (var media in JToken.Parse(json2)["data"]["shortcode_media"]["edge_sidecar_to_children"]["edges"])
                                extract_url(media["node"], option, user.FirstPost.DisplayUrls);
                        }
                    }
                }

                return user;
            }

            private static void extract_url(JToken media, InstagramExtractorOption option, List<string> urls)
            {
                if ((bool)media["is_video"] && !option.OnlyImages)
                {
                    urls.Add(media["video_url"].ToString());
                    if (option.IncludeThumbnail)
                        urls.Add(media["display_url"].ToString());
                }
                else
                    urls.Add(media["display_url"].ToString());
            }

            private static string graphql_qurey(InstagramExtractorOption option, string query_hash, Dictionary<string, string> param)
            {
                var url = $"https://www.instagram.com/graphql/query/?query_hash=" + query_hash + "&variables=" +
                    HttpUtility.UrlEncode(JsonConvert.SerializeObject(param));
                option.PageReadCallback?.Invoke(url);
                return NetTools.DownloadString(url);
            }

            public static Posts query_next(InstagramExtractorOption option, string query_hash, string id, string first, string after)
            {
            RETRY:
                try
                {
                    var json = graphql_qurey(option, query_hash, new Dictionary<string, string> { { "id", id }, { "first", first }, { "after", after } });
                    var jmedia = JToken.Parse(json)["data"]["user"]["edge_owner_to_timeline_media"];

                    var posts = new Posts
                    {
                        HasNext = (bool)jmedia["page_info"]["has_next_page"],
                        EndCursor = jmedia["page_info"]["end_cursor"].ToString(),
                        DisplayUrls = new List<string>()
                    };

                    foreach (var post in jmedia["edges"])
                    {
                        posts.PostCount++;
                        if (post["node"]["__typename"].ToString() != "GraphSidecar" || option.OnlyThumbnail)
                        {
                            extract_url(post["node"], option, posts.DisplayUrls);
                        }
                        else
                        {
                            foreach (var media in post["node"]["edge_sidecar_to_children"]["edges"])
                                extract_url(media["node"], option, posts.DisplayUrls);
                        }
                    }

                    return posts;
                }
                catch
                {
                    Thread.Sleep(2000);
                    goto RETRY;
                }
            }
        }
    }
}
