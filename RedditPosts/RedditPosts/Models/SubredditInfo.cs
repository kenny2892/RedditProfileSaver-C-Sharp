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

        public string DisplaySubreddit // If word is to long, split it to avoid the table being to wide for mobile display
        {
            get
            {
                string toReturn = SubredditName;

                if(toReturn.Length > 15)
                {
                    toReturn = toReturn.Substring(0, 12) + "...";
                }

                return toReturn;
            }
        }

        public bool IsDefaultIcon()
        {
            return IconUrl == Utility.DefaultSubredditIcon;
        }
    }
}
