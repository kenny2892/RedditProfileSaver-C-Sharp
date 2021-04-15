using RedditPosts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedditPosts.ViewModels
{
    public class RedditPostsViewModel
    {
        public List<RedditPost> Posts;
        public Dictionary<string, SubredditInfo> Subreddits;
    }
}
