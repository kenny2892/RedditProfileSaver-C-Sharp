﻿using RedditPosts.Models;
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
        public List<bool> ContentTypes { get; set; }

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
