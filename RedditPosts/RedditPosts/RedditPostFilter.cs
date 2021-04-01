using RedditPosts.Models;
using RedditPosts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace RedditPosts
{
    public class RedditPostFilter
    {
        private RedditViewModel Vm { get; set; }
        private IEnumerable<RedditPost> PostsToFilter { get; set; }

        public RedditPostFilter(RedditViewModel vm, IEnumerable<RedditPost> postsToFilter)
        {
            Vm = vm;
            PostsToFilter = postsToFilter;
        }

        public IEnumerable<RedditPost> FilterPosts()
        {
            TitleFilter();
            AuthorFilter();
            SubredditFilter();
            NsfwFilter();
            SavedFilter();
            HiddenFilter();
            ContentTypeFilter();
            Randomize();

            return PostsToFilter;
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
                PostsToFilter = PostsToFilter.Where(s => s.IsSaved == true);
            }
        }

        private void HiddenFilter()
        {
            if(!Vm.ShowHidden)
            {
                PostsToFilter = PostsToFilter.Where(s => s.Hidden == false);
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

        private void Randomize()
        {
            if(Vm.Randomize)
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

            else
            {
                PostsToFilter = PostsToFilter.OrderByDescending(post => post.Number);
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

                if(words.Contains(" "))
                {
                    keywords = words.Split(" ").ToList();
                }
            }

            for(int i = keywords.Count - 1; i >= 0; i--)
            {
                string keyword = keywords[i];

                if(keyword.ElementAt(0) == '\"' && keyword.ElementAt(keyword.Length - 1) == '\"')
                {
                    string noQuotesKeyword = keyword.Substring(1, keyword.Length - 2);

                    keywords.RemoveAt(i);
                    keywords.Add(noQuotesKeyword);
                    requiredKeywords.Add(noQuotesKeyword);
                }

                else if(keyword.ElementAt(0) == '-')
                {
                    string noDashKeyword = keyword.Substring(1);

                    keywords.RemoveAt(i);
                    keywords.Add(noDashKeyword);
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

            PropertyInfo property = typeof(RedditPost).GetProperties().Single(u => u.Name == propertyName);
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
