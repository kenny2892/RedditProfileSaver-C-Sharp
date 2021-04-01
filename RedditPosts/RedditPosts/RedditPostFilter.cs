using RedditPosts.Models;
using RedditPosts.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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
            (List<string>, List<string>, List<string>) inputs = RetreiveKeywords(Vm.TitleFilter);

            List<string> keywords = inputs.Item1;
            List<string> requiredKeywords = inputs.Item2;
            List<string> bannedKeywords = inputs.Item3;

            if(keywords.Count() > 0)
            {
                PostsToFilter = PostsToFilter.Where(post => keywords.Any(word => post.Title.ToLower().Contains(word.ToLower())));
            }

            if(requiredKeywords.Count() > 0)
            {
                foreach(string requiredWord in requiredKeywords)
                {
                    PostsToFilter = PostsToFilter.Where(post => post.Title.ToLower().Contains(requiredWord.ToLower()));
                }
            }

            if(bannedKeywords.Count() > 0)
            {
                foreach(string bannedWord in bannedKeywords)
                {
                    PostsToFilter = PostsToFilter.Where(post => !post.Title.ToLower().Contains(bannedWord.ToLower()));
                }
            }
        }

        private void AuthorFilter()
        {
            (List<string>, List<string>, List<string>) inputs = RetreiveKeywords(Vm.AuthorFilter);

            List<string> keywords = inputs.Item1;
            List<string> requiredKeywords = inputs.Item2;
            List<string> bannedKeywords = inputs.Item3;

            if(keywords.Count() > 0)
            {
                PostsToFilter = PostsToFilter.Where(post => keywords.Any(word => post.Author.ToLower().Contains(word.ToLower())));
            }

            if(requiredKeywords.Count() > 0)
            {
                foreach(string requiredWord in requiredKeywords)
                {
                    PostsToFilter = PostsToFilter.Where(post => post.Author.ToLower().Contains(requiredWord.ToLower()));
                }
            }

            if(bannedKeywords.Count() > 0)
            {
                foreach(string bannedWord in bannedKeywords)
                {
                    PostsToFilter = PostsToFilter.Where(post => !post.Author.ToLower().Contains(bannedWord.ToLower()));
                }
            }
        }

        private void SubredditFilter()
        {
            (List<string>, List<string>, List<string>) inputs = RetreiveKeywords(Vm.SubredditFilter);

            List<string> keywords = inputs.Item1;
            List<string> requiredKeywords = inputs.Item2;
            List<string> bannedKeywords = inputs.Item3;

            if(keywords.Count() > 0)
            {
                PostsToFilter = PostsToFilter.Where(post => keywords.Any(word => post.Subreddit.ToLower().Contains(word.ToLower())));
            }

            if(requiredKeywords.Count() > 0)
            {
                foreach(string requiredWord in requiredKeywords)
                {
                    PostsToFilter = PostsToFilter.Where(post => post.Subreddit.ToLower().Contains(requiredWord.ToLower()));
                }
            }

            if(bannedKeywords.Count() > 0)
            {
                foreach(string bannedWord in bannedKeywords)
                {
                    PostsToFilter = PostsToFilter.Where(post => !post.Subreddit.ToLower().Contains(bannedWord.ToLower()));
                }
            }
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
    }
}
