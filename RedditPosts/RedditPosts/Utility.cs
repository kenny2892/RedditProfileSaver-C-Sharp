using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public static string GetSubredditIcon(string subreddit)
        {
            return Old_GetSubredditIcon(subreddit);
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
