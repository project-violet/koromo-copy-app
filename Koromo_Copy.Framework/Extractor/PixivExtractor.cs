// This source code is a part of Koromo Copy Project.
// Copyright (C) 2019. dc-koromo. Licensed under the MIT Licence.

using Koromo_Copy.Framework.Crypto;
using Koromo_Copy.Framework.Log;
using Koromo_Copy.Framework.Network;
using Koromo_Copy.Framework.Postprocessor;
using Koromo_Copy.Framework.Setting;
using Koromo_Copy.Framework.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Koromo_Copy.Framework.Extractor.IExtractorOption;

namespace Koromo_Copy.Framework.Extractor
{
    public class PixivExtractorOption : IExtractorOption
    {
    }

    public class PixivExtractor : ExtractorModel
    {
        public PixivExtractor()
        {
            HostName = new Regex(@"www\.pixiv\.net");
            ValidUrl = new Regex(@"^https?://www\.pixiv\.net/(member(?:_illust)?\.php\?id\=|artworks/)(?<id>.*?)$");
        }

        public override IExtractorOption RecommendOption(string url)
        {
            return new PixivExtractorOption { Type = ExtractorType.Works };
        }

        public override string RecommendFormat(IExtractorOption option)
        {
            return "%(artist)s (%(account)s)/%(file)s.%(ext)s";
        }

        public override Tuple<List<NetTask>, object> Extract(string url, IExtractorOption option = null)
        {
            if (!PixivAPI.Auth(Settings.Instance.Model.PixivSettings.Id, Settings.Instance.Model.PixivSettings.Password))
            {
                throw new ExtractorException("Authentication error! Check setting.json/PixivSetting.");
            }

            var match = ValidUrl.Match(url).Groups;

            if (option == null)
                option = new PixivExtractorOption { Type = ExtractorType.Works };

            if (match[1].Value.StartsWith("member") && option.ExtractInformation == false)
            {
                var user = PixivAPI.GetUsersAsync(match["id"].Value.ToInt()).Result;
                var works = PixivAPI.GetUsersWorksAsync(match["id"].Value.ToInt(), 1, 10000000).Result;

                var result = new List<NetTask>();

                foreach (var work in works)
                {
                    if (work.Type == null || work.Type == "illustration")
                    {
                        var task = NetTask.MakeDefault(work.ImageUrls.Large);
                        task.Filename = work.ImageUrls.Large.Split('/').Last();
                        task.SaveFile = true;
                        task.Referer = url;
                        task.Format = new ExtractorFileNameFormat
                        {
                            Artist = user[0].Name,
                            Account = user[0].Account,
                            Id = user[0].Id.Value.ToString(),
                            FilenameWithoutExtension = Path.GetFileNameWithoutExtension(work.ImageUrls.Large.Split('/').Last()),
                            Extension = Path.GetExtension(work.ImageUrls.Large.Split('/').Last()).Replace(".", "")
                        };
                        result.Add(task);
                    }
                    else if (work.Type == "ugoira")
                    {
                        var ugoira_uri = $"https://www.pixiv.net/ajax/illust/{work.Id}/ugoira_meta";

                        option.PageReadCallback?.Invoke(ugoira_uri);

                        var ugoira_data = JToken.Parse(NetTools.DownloadString(ugoira_uri)).SelectToken("body").ToObject<PixivAPI.Ugoira>();
                        var task = NetTask.MakeDefault(ugoira_data.OriginalSource);
                        task.Filename = ugoira_data.OriginalSource.Split('/').Last();
                        task.SaveFile = true;
                        task.Referer = url;
                        var pptask = new PostprocessorTask();
                        pptask.Postprocessor = new UgoiraPostprocessor { Frames = ugoira_data.Frames };
                        task.PostProcess = pptask;
                        task.Format = new ExtractorFileNameFormat
                        {
                            Artist = user[0].Name,
                            Account = user[0].Account,
                            Id = user[0].Id.Value.ToString(),
                            FilenameWithoutExtension = Path.GetFileNameWithoutExtension(ugoira_data.OriginalSource.Split('/').Last()),
                            Extension = Path.GetExtension(ugoira_data.OriginalSource.Split('/').Last()).Replace(".", "")
                        };
                        result.Add(task);
                    }
                }

                return new Tuple<List<NetTask>, object>(result, user);
            }
            else if (option.ExtractInformation == true)
            {
                var user = PixivAPI.GetUsersAsync(match["id"].Value.ToInt()).Result;
                return new Tuple<List<NetTask>, object>(null, user);
            }

            return null;
        }

        /// <summary>
        /// C# porting version of pixivpy
        /// 
        /// Reference: https://github.com/upbit/pixivpy
        /// Reference: https://github.com/cucmberium/Pixeez
        /// </summary>
        public class PixivAPI
        {
            public static string AccessToken { get; private set; }

            #region Pixiv Model

            public class Paginated<T> : List<T>
                where T : class, new()
            {
                public Pagination Pagination { get; set; }
            }

            public class ProfileImageUrls
            {
                [JsonProperty("px_16x16")]
                public string Px16x16 { get; set; }

                [JsonProperty("px_50x50")]
                public string Px50x50 { get; set; }

                [JsonProperty("px_170x170")]
                public string Px170x170 { get; set; }
            }

            public class UserStats
            {
                [JsonProperty("works")]
                public int? Works { get; set; }

                [JsonProperty("favorites")]
                public int? Favorites { get; set; }

                [JsonProperty("following")]
                public int? Following { get; set; }

                [JsonProperty("friends")]
                public int? Friends { get; set; }
            }

            public class User
            {
                [JsonProperty("id")]
                public long? Id { get; set; }

                [JsonProperty("account")]
                public string Account { get; set; }

                [JsonProperty("name")]
                public string Name { get; set; }

                [JsonProperty("is_following")]
                public bool? IsFollowing { get; set; }

                [JsonProperty("is_follower")]
                public bool? IsFollower { get; set; }

                [JsonProperty("is_friend")]
                public bool? IsFriend { get; set; }

                [JsonProperty("is_premium")]
                public bool? IsPremium { get; set; }

                [JsonProperty("profile_image_urls")]
                public ProfileImageUrls ProfileImageUrls { get; set; }

                [JsonProperty("stats")]
                public UserStats Stats { get; set; }

                [JsonProperty("profile")]
                public Profile Profile { get; set; }
            }

            public class Contacts
            {
                [JsonProperty("twitter")]
                public string Twitter { get; set; }
            }

            public class Workspace
            {
                [JsonProperty("computer")]
                public string Computer { get; set; }

                [JsonProperty("monitor")]
                public string Monitor { get; set; }

                [JsonProperty("software")]
                public string Software { get; set; }

                [JsonProperty("scanner")]
                public string Scanner { get; set; }

                [JsonProperty("tablet")]
                public string Tablet { get; set; }

                [JsonProperty("mouse")]
                public string Mouse { get; set; }

                [JsonProperty("printer")]
                public string Printer { get; set; }

                [JsonProperty("on_table")]
                public string OnTable { get; set; }

                [JsonProperty("music")]
                public string Music { get; set; }

                [JsonProperty("table")]
                public string Table { get; set; }

                [JsonProperty("chair")]
                public string Chair { get; set; }

                [JsonProperty("other")]
                public string Other { get; set; }

                [JsonProperty("image_url")]
                public string ImageUrl { get; set; }

                [JsonProperty("image_urls")]
                public ImageUrls ImageUrls { get; set; }
            }

            public class Profile
            {
                [JsonProperty("contacts")]
                public Contacts Contacts { get; set; }

                [JsonProperty("workspace")]
                public Workspace Workspace { get; set; }

                [JsonProperty("job")]
                public string Job { get; set; }

                [JsonProperty("introduction")]
                public string Introduction { get; set; }

                [JsonProperty("location")]
                public string Location { get; set; }

                [JsonProperty("gender")]
                public string Gender { get; set; }

                [JsonProperty("tags")]
                public object Tags { get; set; }

                [JsonProperty("homepage")]
                public string Homepage { get; set; }

                [JsonProperty("birth_date")]
                public string BirthDate { get; set; }

                [JsonProperty("blood_type")]
                public string BloodType { get; set; }
            }

            public class Pagination
            {
                [JsonProperty("previous")]
                public int? Previous { get; set; }

                [JsonProperty("next")]
                public int? Next { get; set; }

                [JsonProperty("current")]
                public int? Current { get; set; }

                [JsonProperty("per_page")]
                public int? PerPage { get; set; }

                [JsonProperty("total")]
                public int? Total { get; set; }

                [JsonProperty("pages")]
                public int? Pages { get; set; }
            }

            public class ImageUrls
            {
                [JsonProperty("px_128x128")]
                public string Px128x128 { get; set; }

                [JsonProperty("small")]
                public string Small { get; set; }

                [JsonProperty("medium")]
                public string Medium { get; set; }

                [JsonProperty("large")]
                public string Large { get; set; }

                [JsonProperty("px_480mw")]
                public string Px480mw { get; set; }
            }

            public class FavoritedCount
            {
                [JsonProperty("public")]
                public int? Public { get; set; }

                [JsonProperty("private")]
                public int? Private { get; set; }
            }

            public class WorkStats
            {
                [JsonProperty("scored_count")]
                public int? ScoredCount { get; set; }

                [JsonProperty("score")]
                public int? Score { get; set; }

                [JsonProperty("views_count")]
                public int? ViewsCount { get; set; }

                [JsonProperty("favorited_count")]
                public FavoritedCount FavoritedCount { get; set; }

                [JsonProperty("commented_count")]
                public int? CommentedCount { get; set; }
            }

            public class Page
            {
                [JsonProperty("image_urls")]
                public ImageUrls ImageUrls { get; set; }
            }

            public class Metadata
            {
                [JsonProperty("pages")]
                public IList<Page> Pages { get; set; }
            }

            public class UsersWork
            {
                [JsonProperty("id")]
                public long? Id { get; set; }

                [JsonProperty("title")]
                public string Title { get; set; }

                [JsonProperty("caption")]
                public string Caption { get; set; }

                [JsonProperty("tags")]
                public IList<string> Tags { get; set; }

                [JsonProperty("tools")]
                public IList<string> Tools { get; set; }

                [JsonProperty("image_urls")]
                public ImageUrls ImageUrls { get; set; }

                [JsonProperty("width")]
                public int? Width { get; set; }

                [JsonProperty("height")]
                public int? Height { get; set; }

                [JsonProperty("stats")]
                public WorkStats Stats { get; set; }

                [JsonProperty("publicity")]
                public int? Publicity { get; set; }

                [JsonProperty("age_limit")]
                public string AgeLimit { get; set; }

                [JsonProperty("created_time")]
                public DateTimeOffset CreatedTime { get; set; }

                [JsonProperty("reuploaded_time")]
                public string ReuploadedTime { get; set; }

                [JsonProperty("user")]
                public User User { get; set; }

                [JsonProperty("is_manga")]
                public bool? IsManga { get; set; }

                [JsonProperty("is_liked")]
                public bool? IsLiked { get; set; }

                [JsonProperty("favorite_id")]
                public long? FavoriteId { get; set; }

                [JsonProperty("page_count")]
                public int PageCount { get; set; }

                [JsonProperty("book_style")]
                public string BookStyle { get; set; }

                [JsonProperty("type")]
                public string Type { get; set; }

                [JsonProperty("metadata")]
                public Metadata Metadata { get; set; }

                [JsonProperty("content_type")]
                public string ContentType { get; set; }

                [JsonProperty("sanity_level")]
                public string SanityLevel { get; set; }
            }

            public class UgoiraFrames
            {
                [JsonProperty("file")]
                public string File { get; set; }

                [JsonProperty("delay")]
                public int? Delay { get; set; }
            }

            public class Ugoira
            {
                [JsonProperty("src")]
                public string Source { get; set; }

                [JsonProperty("originalSrc")]
                public string OriginalSource { get; set; }

                [JsonProperty("mime_type")]
                public string MimeType { get; set; }

                [JsonProperty("frames")]
                public List<UgoiraFrames> Frames { get; set; }
            }

            #endregion

            /// <summary>
            /// Try login to pixiv
            /// </summary>
            /// <param name="username"></param>
            /// <param name="password"></param>
            /// <returns></returns>
            public static bool Auth(string username, string password)
            {
                if (!string.IsNullOrEmpty(AccessToken))
                    return true;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    return false;

                Logs.Instance.Push("[PixivAPI] Try auth...");

                const string client_id = "MOBrBDS8blbauoSck0ZfDbtuzpyT";
                const string client_secret = "lsACyCD94FhDUtGTXi3QzcFE2uU1hqtDaKeqrdwj";
                const string hash_secret = "28c1fdd170a5204386cb1313c7077b34f83e4aaf4aa829ce78c231e05b0bae2c";
                var local_time = DateTime.Now.ToString("s");

                var task = NetTask.MakeDefault("https://oauth.secure.pixiv.net/auth/token");

                task.UserAgent = "PixivAndroidApp/5.0.64 (Android 6.0)";
                task.Referer = "http://www.pixiv.net/";

                task.Headers = new Dictionary<string, string>();
                task.Headers.Add("X-Client-Time", local_time);
                task.Headers.Add("X-Client-Hash", (local_time + hash_secret).GetHashMD5().ToLower());

                task.Query = new Dictionary<string, string>();
                task.Query.Add("grant_type", "password");
                task.Query.Add("username", username);
                task.Query.Add("password", password);
                task.Query.Add("get_secure_url", "1");
                task.Query.Add("client_id", client_id);
                task.Query.Add("client_secret", client_secret);

                var token = NetTools.DownloadString(task);

                if (token == null)
                {
                    Logs.Instance.PushError("[PixivAPI] Auth fail. Try again.");
                    return false;
                }

                AccessToken = JObject.Parse(token)["response"]["access_token"].ToString();

                Logs.Instance.Push("[PixivAPI] Success auth - " + AccessToken);

                return true;
            }

            /// <summary>
            /// Get user informations
            /// </summary>
            /// <param name="authorId"></param>
            /// <returns></returns>
            public static async Task<List<User>> GetUsersAsync(long authorId)
            {
                var url = "https://public-api.secure.pixiv.net/v1/users/" + authorId.ToString() + ".json";

                var param = new Dictionary<string, string>
                {
                    { "profile_image_sizes", "px_170x170,px_50x50" } ,
                    { "image_sizes", "px_128x128,small,medium,large,px_480mw" } ,
                    { "include_stats", "1" } ,
                    { "include_profile", "1" } ,
                    { "include_workspace", "1" } ,
                    { "include_contacts", "1" } ,
                };

                var task = NetTask.MakeDefault(url + "?" + string.Join("&", param.ToList().Select(x => $"{x.Key}={x.Value}")));
                task.Referer = "http://spapi.pixiv.net/";
                task.UserAgent = "PixivIOSApp/5.8.0";
                task.Headers = new Dictionary<string, string>();
                task.Headers.Add("Authorization", "Bearer " + AccessToken);

                var result = await NetTools.DownloadStringAsync(task);
                return JToken.Parse(result).SelectToken("response").ToObject<List<User>>();
            }

            /// <summary>
            /// Get users works async by author-id.
            /// </summary>
            /// <param name="authorId"></param>
            /// <param name="page"></param>
            /// <param name="perPage"></param>
            /// <param name="publicity"></param>
            /// <param name="includeSanityLevel"></param>
            /// <returns></returns>
            public static async Task<Paginated<UsersWork>> GetUsersWorksAsync(long authorId, int page = 1, int perPage = 30, string publicity = "public", bool includeSanityLevel = true)
            {
                var url = "https://public-api.secure.pixiv.net/v1/users/" + authorId.ToString() + "/works.json";

                var param = new Dictionary<string, string>
                {
                    { "page", page.ToString() } ,
                    { "per_page", perPage.ToString() } ,
                    { "publicity", publicity } ,
                    { "include_stats", "1" } ,
                    { "include_sanity_level", Convert.ToInt32(includeSanityLevel).ToString() } ,
                    { "image_sizes", "px_128x128,small,medium,large,px_480mw" } ,
                    { "profile_image_sizes", "px_170x170,px_50x50" } ,
                };

                var task = NetTask.MakeDefault(url + "?" + string.Join("&", param.ToList().Select(x => $"{x.Key}={x.Value}")));
                task.Referer = "http://spapi.pixiv.net/";
                task.UserAgent = "PixivIOSApp/5.8.0";
                task.Headers = new Dictionary<string, string>();
                task.Headers.Add("Authorization", "Bearer " + AccessToken);

                var result = await NetTools.DownloadStringAsync(task);
                return JToken.Parse(result).SelectToken("response").ToObject<Paginated<UsersWork>>();
            }
        }
    }
}
