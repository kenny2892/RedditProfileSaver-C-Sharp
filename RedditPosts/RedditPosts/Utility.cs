using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reddit;
using Reddit.Controllers;
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
        private static RedditClient RedditClient { get; set; }

        private static Dictionary<string, string> SubredditIconUrls = new Dictionary<string, string>();

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

            if(!CreateConfiguration())
            {
                return toReturn;
            }

            if(!SubredditIconUrls.ContainsKey(subredditName))
            {
                System.Diagnostics.Debug.WriteLine("GETTING SUBREDDIT: " + subredditName);

                Subreddit subredditAbout = RedditClient.Subreddit(subredditName).About();
                string communityIcon = subredditAbout.SubredditData.CommunityIcon;
                string iconImg = "";

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
                    toReturn = !String.IsNullOrEmpty(iconImg) ? iconImg : communityIcon;
                    SubredditIconUrls.Add(subredditName, toReturn);
                }
            }

            else
            {
                toReturn = SubredditIconUrls.GetValueOrDefault(subredditName);
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

        private static string Old_GetSubredditIcon(string subreddit)
        {
            string toReturn = "https://b.thumbs.redditmedia.com/iTldIIlQVSoH6SPlH9iiPZZVzFWubJU7cOM__uqSOqU.png"; // Default to Reddit Announcements Icon

            if(!SubredditIconUrls.ContainsKey(subreddit))
            {
                string url = "https://www.reddit.com/r/" + subreddit + "/about.json";
                string jsonContent = Utility.GetContentsOfUrl(url);
                JObject jsonObject = JObject.Parse(jsonContent);

                foreach(var topElement in jsonObject)
                {
                    if(topElement.Key == "data")
                    {
                        JObject dataJson = JObject.Parse(topElement.Value.ToString());
                        foreach(var data in dataJson)
                        {
                            if(data.Key == "icon_img" && !String.IsNullOrEmpty(data.Value.ToString()))
                            {
                                toReturn = data.Value.ToString();
                                SubredditIconUrls.Add(subreddit, toReturn);
                                break;
                            }

                            else if(data.Key == "community_icon" && !String.IsNullOrEmpty(data.Value.ToString()))
                            {
                                toReturn = data.Value.ToString();
                                SubredditIconUrls.Add(subreddit, toReturn);
                                break;
                            }
                        }
                        break;
                    }
                }
            }

            else
            {
                toReturn = SubredditIconUrls.GetValueOrDefault(subreddit);
            }

            return toReturn;
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
