using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditPosts.Models
{
    public class RedditPost
    {
        public int ID { get; set; }
        public int Number { get; set; }
        private string title = "";
        public string Title // If word is to long, split it in half as to avoid the table being to wide for mobile display
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                string[] words = title.Split(" ");

                for (int i = 0; i < words.Count(); i++)
                {
                    string word = words[i];

                    if (word.Length > 15)
                    {
                        word = word.Substring(0, word.Length / 2) + "-\n" + word.Substring(word.Length / 2);
                    }

                    if (i != 0)
                    {
                        word = " " + word;
                    }

                    sb.Append(word);
                }

                return sb.ToString();
            }

            set { title = value; }
        }
        public string Author { get; set; }
        public string Subreddit { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

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

            else if (UrlContent.Contains(".jpg") || UrlContent.Contains(".png") || UrlContent.Contains(".jpeg") || (UrlContent.Contains(".gif") && !UrlContent.Contains(".gifv")))
            {
                type = ContentType.Image;
            }

            else if (UrlContent.Contains(".mp4"))
            {
                type = ContentType.Mp4;
            }

            else if (UrlContent.Contains("redgifs") && UrlContent.Contains("watch"))
            {
                type = ContentType.RedGifWatch;
            }

            else if (UrlContent.Contains("youtu.be"))
            {
                type = ContentType.Youtube;
            }

            else if (UrlContent.Contains("gifv"))
            {
                type = ContentType.Gifv;
            }

            else if (UrlContent.Contains("v.redd.it"))
            {
                type = ContentType.Vreddit;
            }

            else if (UrlContent.Contains("gfycat.com"))
            {
                type = ContentType.GfyCat;
            }

            else if (UrlContent.Contains("https://imgur.com/a/"))
            {
                type = ContentType.ImgurGallery;
            }

            return type;
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

            return UrlContent.Replace("https://imgur.com/", "");
        }

        public string GetImgurIdNum()
        {
            if (GetContentType() != ContentType.ImgurGallery)
            {
                return "";
            }

            return UrlContent.Replace("https://imgur.com/a/", "");
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
    }
}
