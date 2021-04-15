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

        public static SubredditInfo MakeDefaultSubredditInfo()
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
