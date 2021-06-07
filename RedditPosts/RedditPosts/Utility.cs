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
using System.Text.RegularExpressions;
using System.Threading;
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

        // Source for Getting Image Metadata: https://www.codeproject.com/Articles/1120681/Generating-Facebook-Like-Preview-using-Regular-Exp
        public static MatchCollection GetWebPageMetaData(string urlContent)
        {
            Regex regex = new Regex("<meta[\\s]+[^>]*?content[\\s]?=[\\s\"\']+(.*?)[\"\']+.*?>");
            MatchCollection mc = regex.Matches(urlContent);

            return mc;
        }

        public static string ValidateImageUrl(string url, string sourceUrl)
        {
            if(!url.StartsWith("/")) // Not relative url
            {
                return IsValidUrl(url) ? url : "";
            }

            string toCheck = sourceUrl + url;
            if(IsValidUrl(toCheck))
            {
                return toCheck;
            }

            string plainSourceUrl = sourceUrl.Replace("http://www.", "").Replace("https://www.", "").Replace("http://", "").Replace("https://", "");

            // Have to check for each combo of "http", "https", and "www."
            toCheck = "http://" + plainSourceUrl + url;
            if(IsValidUrl(toCheck))
            {
                return toCheck;
            }

            Thread.Sleep(5);
            toCheck = "https://" + plainSourceUrl + url;
            if(IsValidUrl(toCheck))
            {
                return toCheck;
            }

            Thread.Sleep(5);
            toCheck = "http://www." + plainSourceUrl + url;
            if(IsValidUrl(toCheck))
            {
                return toCheck;
            }

            Thread.Sleep(5);
            toCheck = "https://www." + plainSourceUrl + url;
            if(IsValidUrl(toCheck))
            {
                return toCheck;
            }

            return "";
        }

        public static bool IsValidUrl(string url)
        {
            try
            {
                HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
                var response = request.GetResponse() as HttpWebResponse;

                return response.StatusCode.ToString().ToLower() == "ok";
            }

            catch(Exception)
            {
                return false;
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

        public static void Print(string str)
        {
            System.Diagnostics.Debug.WriteLine(str);
            Console.WriteLine(str);
        }
    }
}
