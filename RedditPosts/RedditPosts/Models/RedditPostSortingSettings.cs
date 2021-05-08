using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedditPosts.Models
{
    public enum RedditPostSortingSettings
    {
        Newest_Added,
        Oldest_Added,
        Newest_By_Date,
        Oldest_By_Date,
        Subreddit,
        Random
    }
}
