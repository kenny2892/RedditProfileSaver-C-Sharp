using RedditPosts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedditPosts.ViewModels
{
    public class RedditViewModel
    {
        public List<RedditPost> RedditPosts { get; set; }

        public string TitleFilter { get; set; } = "";
        public string AuthorFilter { get; set; } = "";
        public string SubredditFilter { get; set; } = "";
        public bool SavedOnly { get; set; } = false;
        public bool Randomize { get; set; } = false;
        public int RandomizeSeed { get; set; } = 0;
        public NsfwSettings NsfwSetting { get; set; } = NsfwSettings.No_Filter;
        public bool IsMobile { get; set; } = false;

        public bool Mp4 { get; set; } = true;
        public bool Twitter { get; set; } = true;
        public bool Youtube { get; set; } = true;
        public bool Image { get; set; } = true;
        public bool Gif { get; set; } = true;
        public bool Gifv { get; set; } = true;
        public bool ImgurGallery { get; set; } = true;
        public bool GfyCat { get; set; } = true;
        public bool RedGifWatch { get; set; } = true;
        public bool Gallery { get; set; } = true;
        public bool Vreddit { get; set; } = true;
        public bool UrlPreview { get; set; } = true;
        public bool Blank { get; set; } = true;

        public RedditViewModel()
        {
            Random seedGen = new Random();
            RandomizeSeed = seedGen.Next();
        }
    }
}
