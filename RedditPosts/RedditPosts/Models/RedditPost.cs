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

        private string urlContent;
        [Display(Name = "Content")]
        public string UrlContent
        {
            get
            {
                if(urlContent.Contains("/gallery/"))
                {
                    string[] links = urlContent.Split(" ");

                    if(links.Count() < 2)
                    {
                        return links[0];
                    }

                    return links[1];
                }

                else if(urlContent.Contains(".gifv"))
                {
                    return urlContent.Replace(".gifv", ".mp4");
                }

                else if(urlContent.Contains("https://gfycat.com/"))
                {
                    return urlContent.Replace("https://gfycat.com/", "https://gfycat.com/ifr/");
                }

                else if(urlContent.Contains("https://imgur.com/") && !urlContent.Contains("/a/")) // An Imgur pic that wasn't posted with the direct link
                {
                    return urlContent.Replace("https://imgur.com/", "https://i.imgur.com/") + ".png";
                }

                else if(urlContent.Contains("http://imgur.com/") && !urlContent.Contains("/a/")) // An Imgur pic that wasn't posted with the direct link
                {
                    return urlContent.Replace("http://imgur.com/", "http://i.imgur.com/") + ".png";
                }

                return urlContent;
            }

            set { urlContent = value; }
        }

        [Display(Name = "Post")]
        public string UrlPost { get; set; }

        [Display(Name = "Thumbnail")]
        public string UrlThumbnail { get; set; }

        [Display(Name = "Saved")]
        public bool IsSaved { get; set; }
        [Display(Name = "NSFW")]
        public bool IsNsfw { get; set; }
        public ContentType GetContentType()
        {
            ContentType type = ContentType.Blank;

            if(urlContent.Contains(" ")) // Contains spaces, meaning it has multiple Urls and is a gallery
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

            else if(UrlContent.Contains("v.redd.it"))
            {
                type = ContentType.Vreddit;
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

        public string GetYoutubeUrl()
        {
            if(GetContentType() != ContentType.Youtube)
            {
                return "";
            }

            else if(UrlContent.Contains("youtu.be"))
            {
                return UrlContent.Replace("https://youtu.be/", "https://www.youtube.com/embed/");
            }

            return UrlContent.Replace("https://www.youtube.com/watch?v=", "https://www.youtube.com/embed/");
        }

        public string[] GetGalleryUrls()
        {
            if(GetContentType() != ContentType.Gallery)
            {
                return new string[] { UrlContent };
            }

            return urlContent.Split(" ").Skip(1).ToArray();
        }

        public string GetImgurId()
        {
            if(GetContentType() != ContentType.ImgurGallery)
            {
                return "";
            }

            else if(UrlContent.Contains("https"))
            {
                return UrlContent.Replace("https://imgur.com/", "");
            }

            return UrlContent.Replace("http://imgur.com/", "");
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
    }
}
