using RedditPosts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedditPosts.ViewModels
{
    public class SubredditsViewModel
    {
        public Dictionary<SubredditInfo, int> Subreddits { get; set; }
        public List<SubredditInfo> MostUpvoted { get; set; }
        public List<SubredditInfo> LeastUpvoted { get; set; }
        public int MostUpvotedCount { get; set; }
        public int LeastUpvotedCount { get; set; }
    }
}
