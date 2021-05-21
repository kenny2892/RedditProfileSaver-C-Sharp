using RedditPosts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedditPosts.ViewModels
{
    public class RedditViewModel
    {
        public int PostCount { get; set; } = 0;
        public string DisplayPostCount => String.Format("{0:n0}", PostCount);
        public string TitleFilter { get; set; } = "";
        public string AuthorFilter { get; set; } = "";
        public string SubredditFilter { get; set; } = "";
        public bool SavedOnly { get; set; } = false;
        public bool FavoritedOnly { get; set; } = false;
        public bool ShowHidden { get; set; } = false;
        public int RandomizeSeed { get; set; } = 0;
        public NsfwSettings NsfwSetting { get; set; } = NsfwSettings.No_Filter;
        public RedditPostSortingSettings SortingSetting { get; set; } = RedditPostSortingSettings.Newest_Added;
        public bool IsMobile { get; set; } = false;
        public List<bool> ContentTypes { get; set; }
        public bool UseDateRange { get; set; } = false;
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;
        public int MaxPostNumber { get; set; } = -1;

        public RedditViewModel()
        {
            Random seedGen = new Random();
            RandomizeSeed = seedGen.Next();

            ContentTypes = new List<bool>();
            foreach(ContentType type in Enum.GetValues(typeof(ContentType)))
            {
                ContentTypes.Add(true);
            }
        }
    }
}
