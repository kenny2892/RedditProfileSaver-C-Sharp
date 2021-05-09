using RedditPosts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedditPosts.ViewModels
{
    public class SubredditPageViewModel
    {
        public SubredditSortingSettings SortingSetting { get; set; }
        public NsfwSettings NsfwSetting { get; set; } = NsfwSettings.No_Filter;
        public bool IsMobile { get; set; } = false;
    }
}
