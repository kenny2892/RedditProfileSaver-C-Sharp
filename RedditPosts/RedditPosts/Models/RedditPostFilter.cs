using RedditPosts.Models;
using RedditPosts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedditPosts.Models
{
    public class RedditPostFilter
    {
        private RedditViewModel Vm { get; set; }
        private IEnumerable<RedditPost> PostsToFilter { get; set; }
        private bool DenyNsfw { get; set; }

        public RedditPostFilter(RedditViewModel vm, bool denyNsfw)
        {
            Vm = vm;
            DenyNsfw = denyNsfw;
        }

        public IEnumerable<RedditPost> FilterPosts(IEnumerable<RedditPost> postsToFilter)
        {
            PostsToFilter = postsToFilter;

            MaxPostNumber();
            Sort();
            TitleFilter();
            AuthorFilter();
            SubredditFilter();
            NsfwFilter();
            SavedFilter();
            FavoriteFilter();
            DateRangeFilter();
            ContentTypeFilter();

            return PostsToFilter;
        }

        private void MaxPostNumber()
        {
            if(Vm.MaxPostNumber >= 0)
            {
                PostsToFilter = PostsToFilter.Where(post => post.Number <= Vm.MaxPostNumber);
            }
        }

        private void TitleFilter()
        {
            FilterKeywords("Title", RetreiveKeywords(Vm.TitleFilter));
        }

        private void AuthorFilter()
        {
            FilterKeywords("Author", RetreiveKeywords(Vm.AuthorFilter));
        }

        private void SubredditFilter()
        {
            FilterKeywords("Subreddit", RetreiveKeywords(Vm.SubredditFilter));
        }

        private void NsfwFilter()
        {
            if(DenyNsfw)
            {
                PostsToFilter = PostsToFilter.Where(s => !s.IsNsfw);
            }

            switch(Vm.NsfwSetting)
            {
                case NsfwSettings.No_Filter:
                    break;

                case NsfwSettings.Nsfw_Only:
                    PostsToFilter = PostsToFilter.Where(s => s.IsNsfw);
                    break;

                case NsfwSettings.No_Nsfw:
                    PostsToFilter = PostsToFilter.Where(s => !s.IsNsfw);
                    break;
            }
        }

        private void SavedFilter()
        {
            if(Vm.SavedOnly)
            {
                PostsToFilter = PostsToFilter.Where(s => s.IsSaved);
            }
        }

        private void FavoriteFilter()
        {
            if(Vm.FavoritedOnly)
            {
                PostsToFilter = PostsToFilter.Where(s => s.IsFavorited);
            }
        }

        private void ContentTypeFilter()
        {
            List<RedditPost> posts = PostsToFilter.ToList();
            List<ContentType> types = Enum.GetValues(typeof(ContentType)).Cast<ContentType>().ToList();

            for(int i = posts.Count - 1; i >= 0; i--)
            {
                RedditPost post = posts[i];
                ContentType type = post.GetContentType();

                for(int j = 0; j < types.Count; j++)
                {
                    bool isWhitelisted = Vm.ContentTypes[j];

                    if(!isWhitelisted && types[j] == type)
                    {
                        posts.RemoveAt(i);
                    }
                }
            }

            PostsToFilter = posts.AsEnumerable();
        }

        private void Sort()
        {
            switch(Vm.SortingSetting)
            {
                case RedditPostSortingSettings.Newest_Added:
                    PostsToFilter = PostsToFilter.OrderByDescending(post => post.Number);
                    break;

                case RedditPostSortingSettings.Oldest_Added:
                    PostsToFilter = PostsToFilter.OrderBy(post => post.Number);
                    break;
                case RedditPostSortingSettings.Newest_By_Date:
                    PostsToFilter = PostsToFilter.OrderByDescending(post => post.Date);
                    break;

                case RedditPostSortingSettings.Oldest_By_Date:
                    PostsToFilter = PostsToFilter.OrderBy(post => post.Date);
                    break;

                case RedditPostSortingSettings.Subreddit:
                    PostsToFilter = PostsToFilter.OrderBy(post => post.Subreddit.ToLower());
                    break;

                case RedditPostSortingSettings.Random:
                    Randomize();
                    break;
            }
        }

        private void Randomize()
        {
            List<RedditPost> posts = PostsToFilter.ToList();
            Random r = new Random(Vm.RandomizeSeed);

            int curr = posts.Count;
            while(curr > 1)
            {
                curr--;
                int toSwap = r.Next(curr + 1);
                RedditPost value = posts[toSwap];
                posts[toSwap] = posts[curr];
                posts[curr] = value;
            }

            PostsToFilter = posts.AsEnumerable();
        }

        private void DateRangeFilter()
        {
            if(Vm.UseDateRange && Vm.StartDate.Date <= Vm.EndDate.Date)
            {
                PostsToFilter = PostsToFilter.Where(post => Vm.StartDate.Date <= post.Date.Date && post.Date.Date <= Vm.EndDate.Date);
            }
        }

        private (List<string>, List<string>, List<string>) RetreiveKeywords(string words)
        {
            List<string> keywords = new List<string>();
            List<string> requiredKeywords = new List<string>();
            List<string> bannedKeywords = new List<string>();

            if(!String.IsNullOrEmpty(words))
            {
                keywords.Add(words);

                if(words.Contains("\""))
                {
                    // Source: https://stackoverflow.com/a/13024181
                    var reg = new Regex("\".*?\"");
                    var matches = reg.Matches(words);

                    foreach(var item in matches)
                    {
                        requiredKeywords.Add(item.ToString().Replace("\"", ""));
                    }
                }

                if(words.Contains(" "))
                {
                    if(requiredKeywords.Count > 0)
                    {
                        foreach(string requiredKeyword in requiredKeywords)
                        {
                            words = words.Replace("\"" + requiredKeyword + "\"", requiredKeyword);
                        }
                    }

                    keywords = words.Split(" ").ToList();
                }
            }

            for(int i = keywords.Count - 1; i >= 0; i--)
            {
                string keyword = keywords[i];

                if(keyword.ElementAt(0) == '-')
                {
                    string noDashKeyword = keyword.Substring(1);

                    keywords.RemoveAt(i);
                    bannedKeywords.Add(noDashKeyword);
                }
            }

            return (keywords, requiredKeywords, bannedKeywords);
        }

        // Source for abstract properties: https://stackoverflow.com/a/11431611
        private void FilterKeywords(string propertyName, (List<string>, List<string>, List<string>) keywordSplits)
        {
            if(typeof(RedditPost).GetProperty(propertyName) == null)
            {
                return;
            }

            PropertyInfo property = typeof(RedditPost).GetProperty(propertyName);
            List<string> keywords = keywordSplits.Item1;
            List<string> requiredKeywords = keywordSplits.Item2;
            List<string> bannedKeywords = keywordSplits.Item3;

            if(keywords.Count() > 0)
            {
                PostsToFilter = PostsToFilter.Where(post => keywords.Any(word => property.GetValue(post).ToString().ToLower().Contains(word.ToLower())));
            }

            if(requiredKeywords.Count() > 0)
            {
                foreach(string requiredWord in requiredKeywords)
                {
                    PostsToFilter = PostsToFilter.Where(post => property.GetValue(post).ToString().ToLower().Contains(requiredWord.ToLower()));
                }
            }

            if(bannedKeywords.Count() > 0)
            {
                foreach(string bannedWord in bannedKeywords)
                {
                    PostsToFilter = PostsToFilter.Where(post => !property.GetValue(post).ToString().ToLower().Contains(bannedWord.ToLower()));
                }
            }
        }
    }
}
