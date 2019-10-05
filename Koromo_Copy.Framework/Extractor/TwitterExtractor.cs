// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using HtmlAgilityPack;
using Koromo_Copy.Framework.Network;
using Koromo_Copy.Framework.Utils;
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
    public class TwitterExtractorOption : IExtractorOption
    {
    }

    public class TwitterExtractor : ExtractorModel
    {
        public TwitterExtractor()
        {
            HostName = new Regex(@"twitter\.com");
            ValidUrl = new Regex(@"^https?://twitter\.com/(?<id>hashtag|.*?)(/(?<search>.*))?$");
            ExtractorInfo = "Twitter extactor info\r\n" +
                "   user:             Full-name.\r\n" +
                "   account:          User-name";
        }

        public override IExtractorOption RecommendOption(string url)
        {
            return new TwitterExtractorOption { Type = ExtractorType.Images };
        }

        public override string RecommendFormat(IExtractorOption option)
        {
            return "%(user)s (%(account)s)/%(file)s.%(ext)s";
        }

        public override Tuple<List<NetTask>, object> Extract(string url, IExtractorOption option = null)
        {
            if (option == null)
                option = RecommendOption(url);

            var match = ValidUrl.Match(url).Groups;

            if (match["id"].Value == "hashtag")
            {
#if DEBUG
                var html = NetTools.DownloadString(url);
                var search = HttpUtility.UrlDecode(match["search"].Value);
                var position = Regex.Match(html, @"data-max-position""(.*?)""").Groups[1].Value;

                var document = new HtmlDocument();
                document.LoadHtml(html);
                var node = document.DocumentNode.SelectSingleNode("/html[1]/body[1]/div[1]/div[2]/div[1]/div[2]/div[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[2]/ol[1]");
                var tweets = node.SelectNodes("./li[@data-item-type='tweet']");
                var urls = new List<string>();

                foreach (var tweet in tweets)
                    urls.AddRange(parse_tweet_hashtag(option as TwitterExtractorOption, tweet));

                while (true)
                {
                    try
                    {
                        var next = seach_query(option as TwitterExtractorOption, search, position);
                        position = JToken.Parse(next)["min_position"].ToString();
                        var html2 = JToken.Parse(next)["items_html"].ToString();
                        var document2 = new HtmlDocument();
                        document2.LoadHtml(html2);
                        var tweets2 = node.SelectNodes("./li[@data-item-type='tweet']");
                        foreach (var tweet in tweets2)
                            urls.AddRange(parse_tweet_hashtag(option as TwitterExtractorOption, tweet));
                    }
                    catch
                    {
                        break;
                    }
                }

                var result = new List<NetTask>();
                foreach (var surl in urls)
                {
                    var task = NetTask.MakeDefault(surl);
                    task.SaveFile = true;

                    var fn = surl.Split('/').Last();
                    task.Filename = fn;
                    task.Format = new ExtractorFileNameFormat
                    {
                        FilenameWithoutExtension = Path.GetFileNameWithoutExtension(fn),
                        Extension = Path.GetExtension(fn).Replace(".", ""),
                        User = search
                    };

                    result.Add(task);
                }
                return new Tuple<List<NetTask>, object>(result, null);
#endif
                throw new ExtractorException("'hashtag' is not support yet!");
            }
            else
            {
                var name = match["id"].Value;
                var html = NetTools.DownloadString($"https://twitter.com/{name}/media");
                var min_position = Regex.Match(html, @"data-min-position=""(.*?)""").Groups[1].Value;
                var node = html.ToHtmlNode();
                var tweets = node.SelectNodes("./html[1]/body[1]/div[1]/div[2]/div[1]/div[2]/div[1]/div[1]/div[2]/div[1]/div[2]/div[2]/div[1]/div[2]/ol[1]/li[@data-item-type='tweet']");
                var urls = new List<string>();
                var user = node.SelectSingleNode("/html[1]/body[1]/div[1]/div[2]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/h1[1]/a[1]").InnerText;

                foreach (var tweet in tweets)
                    urls.AddRange(parse_tweet_hashtag(option as TwitterExtractorOption, tweet));

                while (true)
                {
                    var next = profile_query(option as TwitterExtractorOption, name, min_position);
                    var html2 = JToken.Parse(next)["items_html"].ToString();
                    var tweets2 = html2.ToHtmlNode().SelectNodes("./li[@data-item-type='tweet']");
                    if (tweets2 == null)
                        break;
                    foreach (var tweet in tweets2)
                        urls.AddRange(parse_tweet_hashtag(option as TwitterExtractorOption, tweet));
                    min_position = JToken.Parse(next)["min_position"].ToString();
                    if (!(bool)JToken.Parse(next)["has_more_items"])
                        break;
                }

                var result = new List<NetTask>();
                foreach (var surl in urls)
                {
                    var task = NetTask.MakeDefault(surl);
                    task.SaveFile = true;

                    var fn = surl.Split('/').Last();
                    task.Filename = fn;
                    task.Format = new ExtractorFileNameFormat
                    {
                        FilenameWithoutExtension = Path.GetFileNameWithoutExtension(fn),
                        Extension = Path.GetExtension(fn).Replace(".", ""),
                        Account = name,
                        User = user,
                    };

                    result.Add(task);
                }

                return new Tuple<List<NetTask>, object>(result, null);
            }

            return null;
        }

        private List<string> parse_tweet_hashtag(TwitterExtractorOption option, HtmlNode tweet)
        {
            var result = new List<string>();
            var media_img = tweet.SelectNodes("./div[1]/div[2]/div[@class='AdaptiveMediaOuterContainer']//img");
            var media_video = tweet.SelectNodes("./div[1]/div[2]/div[@class='AdaptiveMediaOuterContainer']//div[@class='AdaptiveMedia-video']");

            if (media_img != null)
            {
                foreach (var img in media_img)
                    result.Add(img.GetAttributeValue("src", ""));
            }

            if (media_video != null)
            {
                var id = tweet.SelectSingleNode("./div[1]").GetAttributeValue("data-tweet-id", "");
                var url = $"https://api.twitter.com/1.1/videos/tweet/config/{id}.json";

                option.PageReadCallback?.Invoke(url);

                var task = NetTask.MakeDefault(url);
                task.Headers = new Dictionary<string, string>();
                task.Headers.Add("authorization", "Bearer AAAAAAAAAAAAAAAAAAAAAPYXBAAAAAAACLXUNDekMxqa8h%2F40K4moUkGsoc%3DTYfbDKbT3jJPCEVnMYqilB28NHfOPqkca3qaAxGfsyKCs0wRbw");
                var data = NetTools.DownloadString(task);

                if (JToken.Parse(data)["track"]["contentType"].ToString() == "gif")
                    result.Add(JToken.Parse(data)["track"]["playbackUrl"].ToString());
            }

            return result;
        }

        private static string seach_query(TwitterExtractorOption option, string search, string position)
        {
            var url = $"https://twitter.com/i/search/timeline?";
            url += "verical=default&";
            url += "q=" + HttpUtility.UrlEncode("#" + search) + "&";
            url += "include_available_features=1&";
            url += "include_entities=1&";
            url += "max_position=" + HttpUtility.UrlEncode(position) + "&";
            url += "reset_error_state=false";
            option.PageReadCallback?.Invoke(url);
            return NetTools.DownloadString(url);
        }

        private static string profile_query(TwitterExtractorOption option, string name, string position)
        {
            var url = $"https://twitter.com/i/profiles/show/{name}/media_timeline?";
            url += "include_available_features=1&";
            url += "include_entities=1&";
            url += $"max_position={position}&";
            url += "reset_error_state=false";
            option.PageReadCallback?.Invoke(url);
            return NetTools.DownloadString(url);
        }
    }
}
