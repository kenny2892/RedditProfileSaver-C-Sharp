using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reddit;
using Reddit.Controllers;
using RedditPosts.Data;
using RedditPosts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RedditPosts
{
    public class Utility // Source: https://stackoverflow.com/a/54167231
    {
        public static string DefaultSubredditIcon { get; } = "Default";

        private static IConfiguration Configuration { get; set; }
        private static SubredditInfoContext _subredditInfoContext;
        private static RedditClient RedditClient { get; set; }

        public static void Initialize(SubredditInfoContext subredditInfoContext)
        {
            _subredditInfoContext = subredditInfoContext;
        }

        public static HtmlString EnumToString<T>()
        {
            List<T> enums = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            Dictionary<string, string> dict = new Dictionary<string, string>();

            int count = 0;
            foreach(T value in enums)
            {
                dict.Add(value.ToString(), "ContentTypes_" + count + "_");
                count++;
            }

            return new HtmlString(JsonConvert.SerializeObject(dict));
        }

        public static SubredditInfo GetSubredditInfo(string subredditName)
        {
            SubredditInfo toReturn = MakeDefaultSubredditInfo();

            if(_subredditInfoContext is null || !CreateConfiguration())
            {
                return toReturn;
            }

            var subsQuery = from m in _subredditInfoContext.SubredditInfo select m;
            var results = subsQuery.Where(sub => sub.SubredditName == subredditName);

            if(results.Count() != 1)
            {
                SubredditInfo subredditInfo = RetrieveSubredditInfo(subredditName);

                if(!(subredditInfo is null))
                {
                    _subredditInfoContext.Add(subredditInfo);
                    _subredditInfoContext.SaveChanges();

                    toReturn = subredditInfo;
                }
            }

            else
            {
                toReturn = results.ToList().Cast<SubredditInfo>().ElementAt(0);
            }

            return toReturn;
        }

        public static bool UpdateSubredditIcons()
        {
            if(_subredditInfoContext is null || !CreateConfiguration())
            {
                return false;
            }

            var subsQuery = from m in _subredditInfoContext.SubredditInfo select m;
            var subredditList = subsQuery.OrderBy(subreddit => subreddit.SubredditName.ToLower()).Select(subreddit => subreddit.SubredditName);

            foreach(string subredditName in subredditList)
            {
                SubredditInfo subInfo = RetrieveSubredditInfo(subredditName);
                _subredditInfoContext.Update(subInfo);
                _subredditInfoContext.SaveChanges();

                System.Threading.Thread.Sleep(700); // Sleep to avoid overloading the Reddit Api
            }

            System.Diagnostics.Debug.WriteLine("Finished updating Subreddits");
            return true;
        }

        private static SubredditInfo RetrieveSubredditInfo(string subredditName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("GETTING SUBREDDIT: " + subredditName);

                Subreddit subredditAbout = RedditClient.Subreddit(subredditName).About();

                string primaryColor = !String.IsNullOrEmpty(subredditAbout.PrimaryColor) ? subredditAbout.PrimaryColor : GenerateSubredditColor(subredditName);
                string communityIcon = "";
                string iconImg = "";
                string url = DefaultSubredditIcon;

                if(!(subredditAbout.SubredditData.CommunityIcon is null)) // Reddit's current icon system
                {
                    communityIcon = subredditAbout.SubredditData.CommunityIcon;

                    if(communityIcon.Contains("&amp;"))
                    {
                        communityIcon = communityIcon.Replace("&amp;", "&");
                    }
                }

                if(!(subredditAbout.SubredditData.IconImg is null)) // Reddit's old icon system (used as a backup and in outlier cases such as User posts)
                {
                    iconImg = subredditAbout.SubredditData.IconImg.ToString();

                    if(iconImg.Contains("&amp;"))
                    {
                        iconImg = iconImg.Replace("&amp;", "&");
                    }
                }

                if(!String.IsNullOrEmpty(iconImg) || !String.IsNullOrEmpty(communityIcon)) // Try and use new system, but if not available, use old one.
                {
                    url = !String.IsNullOrEmpty(communityIcon) ? communityIcon : iconImg;
                }

                return new SubredditInfo { SubredditName = subredditName, IconUrl = url, PrimaryColor = primaryColor};
            }

            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Could not get SubredditInfo: " + subredditName);
                return MakeDefaultSubredditInfo();
            }
        }

        private static string GenerateSubredditColor(string subredditName) // Source: https://stackoverflow.com/a/57726983
        {
            string toReturn = "#000001";

            if(subredditName != null)
            {
                int hash = subredditName.GetHashCode();
                toReturn = $"#{hash:X8}";
            }

            return toReturn;
        }

        private static bool CreateConfiguration()
        {
            if(!(RedditClient is null))
            {
                return true;
            }

            try
            {
                Configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", true, true)
               .Build();

                string configPath = Configuration.GetConnectionString("PythonScriptDirectory") + @"\Config.json";
                string id = "";
                string secretId = "";
                string userAgent = "";
                string refreshToken = "";

                using(StreamReader file = File.OpenText(configPath))
                {
                    using(JsonTextReader reader = new JsonTextReader(file))
                    {
                        JObject o2 = (JObject)JToken.ReadFrom(reader);

                        foreach(var element in o2)
                        {
                            switch(element.Key)
                            {
                                case "Id":
                                    id = element.Value.ToString();
                                    break;

                                case "Secret":
                                    secretId = element.Value.ToString();
                                    break;

                                case "User Agent":
                                    userAgent = element.Value.ToString();
                                    break;

                                case "Refresh Token":
                                    refreshToken = element.Value.ToString();
                                    break;
                            }
                        }
                    }
                }

                RedditClient = new RedditClient(appId: id, appSecret: secretId, refreshToken: refreshToken, userAgent: userAgent);
            }

            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        public static string GetContentsOfUrl(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using(Stream stream = response.GetResponseStream())
            using(StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static SubredditInfo MakeDefaultSubredditInfo()
        {
            return new SubredditInfo
            {
                SubredditName = "DEFAULT NAME",
                IconUrl = DefaultSubredditIcon,
                PrimaryColor = "#000001"
            };
        }
    }
}
