using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RedditPosts.Models
{
    public class SubredditInfo
    {
        [Key]
        public string SubredditName { get; set; }
        public string IconUrl { get; set; }
        public string PrimaryColor { get; set; }
    }
}
