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

        public static string GetSubredditIcon(string subredditName)
        {
            string toReturn = "https://b.thumbs.redditmedia.com/iTldIIlQVSoH6SPlH9iiPZZVzFWubJU7cOM__uqSOqU.png"; // Default to Reddit Announcements Icon

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
                }    
            }

            else
            {
                toReturn = results.ToList().Cast<SubredditInfo>().ElementAt(0).IconUrl;
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
            var subredditList = subsQuery.Select((name, url) => name);

            return true;
        }

        private static SubredditInfo RetrieveSubredditInfo(string subredditName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("GETTING SUBREDDIT: " + subredditName);

                Subreddit subredditAbout = RedditClient.Subreddit(subredditName).About();
                string communityIcon = subredditAbout.SubredditData.CommunityIcon;
                string iconImg = "";
                string url = "https://b.thumbs.redditmedia.com/iTldIIlQVSoH6SPlH9iiPZZVzFWubJU7cOM__uqSOqU.png";

                if(!(subredditAbout.SubredditData.IconImg is null))
                {
                    iconImg = subredditAbout.SubredditData.IconImg.ToString();
                }

                if(communityIcon.Contains("&amp;s="))
                {
                    communityIcon = communityIcon.Replace("&amp;s=", "&s=");
                }

                if(!String.IsNullOrEmpty(iconImg) || !String.IsNullOrEmpty(communityIcon))
                {
                    url = !String.IsNullOrEmpty(iconImg) ? iconImg : communityIcon;
                }

                return new SubredditInfo { SubredditName = subredditName, IconUrl = url };
            }

            catch(Exception e)
            {
                return null;
            }
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
    }
}
