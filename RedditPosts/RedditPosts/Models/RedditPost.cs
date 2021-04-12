using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace RedditPosts.Models
{
    public class RedditPost
    {
        public int ID { get; set; }
        public int Number { get; set; }
        public string Title { get; set; }
        public string DisplayTitle // If word is to long, split it in half as to avoid the table being to wide for mobile display
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                string[] words = Title.Split(" ");

                for(int i = 0; i < words.Count(); i++)
                {
                    string word = words[i];

                    if(word.Length > 15)
                    {
                        word = word.Substring(0, word.Length / 2) + "-\n" + word.Substring(word.Length / 2);
                    }

                    if(i != 0)
                    {
                        word = " " + word;
                    }

                    sb.Append(word);
                }

                return sb.ToString();
            }
        }
        public string Author { get; set; }
        public string Subreddit { get; set; }
        public string DisplaySubreddit // If word is to long, split it to avoid the table being to wide for mobile display
        {
            get
            {
                string toReturn = Subreddit;

                if(toReturn.Length > 15)
                {
                    toReturn = toReturn.Substring(0, 12) + "...";
                }

                return toReturn;
            }
        }
        public bool Hidden { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public string FullDate // Formats: https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
        {
            get
            {
                return Date.ToLocalTime().ToString("MMMM dd, yyyy - h:m:s tt");
            }
        }

        [Display(Name = "Content")]
        public string UrlContent { get; set; }

        [Display(Name = "Post")]
        public string UrlPost { get; set; }

        [Display(Name = "Thumbnail")]
        public string UrlThumbnail { get; set; }

        [Display(Name = "Saved")]
        public bool IsSaved { get; set; }
        [Display(Name = "NSFW")]
        public bool IsNsfw { get; set; }

        public string[] GetContentUrls()
        {
            string[] toReturn = new string[] { UrlContent };

            if(UrlContent.Contains("/gallery/"))
            {
                toReturn = UrlContent.Split(" ");
            }

            else if(UrlContent.Contains(".gifv"))
            {
                toReturn[0] = toReturn[0].Replace(".gifv", ".mp4");
            }

            else if(UrlContent.Contains("https://gfycat.com/"))
            {
                toReturn[0] = toReturn[0].Replace("https://gfycat.com/", "https://gfycat.com/ifr/");
            }

            else if(UrlContent.Contains("https://imgur.com/") && !UrlContent.Contains("/a/")) // An Imgur pic that wasn't posted with the direct link
            {
                toReturn[0] = toReturn[0].Replace("https://imgur.com/", "https://i.imgur.com/") + ".png";
            }

            else if(UrlContent.Contains("http://imgur.com/") && !UrlContent.Contains("/a/")) // An Imgur pic that wasn't posted with the direct link
            {
                toReturn[0] = toReturn[0].Replace("http://imgur.com/", "http://i.imgur.com/") + ".png";
            }

            else if(UrlContent.Contains("v.redd.it") && UrlContent.Contains(" "))
            {
                toReturn = UrlContent.Split(" ");
            }

            else if(UrlContent.Contains("youtu.be"))
            {
                toReturn[0] = toReturn[0].Replace("https://youtu.be/", "https://www.youtube.com/embed/");
            }

            else if(UrlContent.Contains("youtube.com"))
            {
                toReturn[0] = toReturn[0].Replace("https://www.youtube.com/watch?v=", "https://www.youtube.com/embed/");
            }

            return toReturn;
        }

        public ContentType GetContentType()
        {
            ContentType type = ContentType.Blank;

            if(UrlContent.Contains("v.redd.it"))
            {
                if(UrlContent.Contains(" "))
                {
                    type = ContentType.Vreddit;
                }

                else
                {
                    type = ContentType.VredditPostOnly;
                }
            }

            else if(UrlContent.Contains(" "))
            {
                type = ContentType.Gallery;
            }

            else if(UrlContent.Contains(".jpg") || UrlContent.Contains(".png") || UrlContent.Contains(".jpeg"))
            {
                type = ContentType.Image;
            }

            else if(UrlContent.Contains(".mp4"))
            {
                type = ContentType.Mp4;
            }

            else if(UrlContent.Contains("redgifs") && UrlContent.Contains("watch"))
            {
                type = ContentType.RedGifWatch;
            }

            else if(UrlContent.Contains("twitter.com/"))
            {
                type = ContentType.Twitter;
            }

            else if(UrlContent.Contains("youtu.be") || UrlContent.Contains("youtube.com"))
            {
                type = ContentType.Youtube;
            }

            else if(UrlContent.Contains("gifv"))
            {
                type = ContentType.Gifv;
            }

            else if(UrlContent.Contains("gif"))
            {
                type = ContentType.Gif;
            }

            else if(UrlContent.Contains("gfycat.com"))
            {
                type = ContentType.GfyCat;
            }

            else if(UrlContent.Contains("imgur.com/a/"))
            {
                type = ContentType.ImgurGallery;
            }

            else if(!UrlThumbnail.Contains("self") && !UrlThumbnail.Contains("default"))
            {
                type = ContentType.UrlPreview;
            }

            return type;
        }

        public string GetImgurIdNum()
        {
            if(GetContentType() != ContentType.ImgurGallery)
            {
                return "";
            }

            else if(UrlContent.Contains("https"))
            {
                return UrlContent.Replace("https://imgur.com/a/", "");
            }

            return UrlContent.Replace("http://imgur.com/a/", "");
        }

        public int GetVredditWidth()
        {
            int width = 50;

            try
            {
                if(GetContentType() == ContentType.Vreddit)
                {
                    width = Int32.Parse(GetContentUrls()[2]);
                }
            }

            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return width;
        }

        public int GetVredditHeight()
        {
            int height = 50;

            try
            {
                if(GetContentType() == ContentType.Vreddit)
                {
                    height = Int32.Parse(GetContentUrls()[3]);
                }
            }

            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return height;
        }

        public string GetVredditId()
        {
            if(GetContentType() != ContentType.Vreddit)
            {
                return "";
            }

            string id = UrlPost.Substring(UrlPost.IndexOf("comments/") + 9);
            id = id.Substring(0, id.IndexOf("/"));

            return id;
        }

        public string GetVredditAspectRatioString()
        {
            if(GetContentType() != ContentType.Vreddit)
            {
                return "100%";
            }

            double width = GetVredditWidth();
            double height = GetVredditHeight();

            double aspRatio = height / width;
            aspRatio *= 100;

            if(aspRatio > 100)
            {
                aspRatio = 100;
            }

            return aspRatio + "%";
        }

        public string GetMobileTitle()
        {
            StringBuilder sb = new StringBuilder();
            string[] words = Title.Split(" ");

            for(int i = 0; i < words.Count(); i++)
            {
                string word = words[i];

                if(word.Length > 15) // If word is to long, split it in half as to avoid the table being to wide for mobile display
                {
                    word = word.Substring(0, word.Length / 2) + "\n" + word.Substring(word.Length / 2);
                }

                if(i != 0)
                {
                    word = " " + word;
                }

                sb.Append(word);
            }

            return sb.ToString();
        }

        public string GetTwitterHtml()
        {
            string toReturn = "<b>TWITTER DEFAULT HTML</b>";

            try
            {
                if(GetContentType() == ContentType.Twitter)
                {
                    string url = @"https://publish.twitter.com/oembed?theme=dark&align=center&dnt=true&url=" + UrlContent;

                    string jsonContent = Utility.GetContentsOfUrl(url);
                    JObject jsonObject = JObject.Parse(jsonContent);

                    foreach(var element in jsonObject)
                    {
                        if(element.Key == "html")
                        {
                            toReturn = element.Value.ToString();
                            break;
                        }
                    }
                }
            }

            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return toReturn;
        }

        public SubredditInfo GetSubredditInfo()
        {
            return Utility.GetSubredditInfo(Subreddit);
        }
    }
}
