using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DeviceDetectorNET;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RedditPosts.Data;
using RedditPosts.Models;
using RedditPosts.ViewModels;

namespace RedditPosts.Controllers
{
    public class SubredditsController : BaseController
    {
        public SubredditsController(RedditPostContext redditPostContext, SubredditInfoContext subredditInfoContext, IConfiguration configuration) : base(redditPostContext, subredditInfoContext, configuration)
        {

        }

        public IActionResult Index()
        {
            if(!HasPasswordAlready())
            {
                return RedirectToAction("Index", "Password", new { redirectTo = "Subreddits" });
            }  

            return View();
        }

        public IActionResult _Subreddits()
        {
            if(!HasPasswordAlready())
            {
                return RedirectToAction("Index", "Password", new { redirectTo = "Subreddits" });
            }

            IQueryable<RedditPost> postsQuery = from m in _redditPostContext.RedditPost select m;
            var subredditNames = postsQuery.Select(post => post.Subreddit).Distinct().OrderBy(subredditName => subredditName.ToLower()).ToList();

            var iconQuery = from m in _subredditInfoContext.SubredditInfo select m;
            iconQuery = iconQuery.OrderBy(subreddit => subreddit.SubredditName.ToLower());

            var subredditsAlreadyStored = iconQuery.Select(subreddit => subreddit.SubredditName).ToList();
            var subredditsToRetrieve = subredditNames.Where(subName => !subredditsAlreadyStored.Contains(subName)).ToList();
            foreach(string subredditName in subredditsToRetrieve)
            {
                GetSubredditInfo(subredditName);
            }

            SubredditsViewModel vm = CreateViewModel(postsQuery);

            return PartialView(vm);
        }

        private SubredditsViewModel CreateViewModel(IQueryable<RedditPost> postsQuery)
        {
            var iconQuery = from m in _subredditInfoContext.SubredditInfo select m;
            iconQuery = iconQuery.OrderBy(subreddit => subreddit.SubredditName.ToLower());

            Dictionary<SubredditInfo, int> model = new Dictionary<SubredditInfo, int>();

            List<SubredditInfo> mostUpvotedInfo = new List<SubredditInfo>() { Utility.MakeDefaultSubredditInfo() };
            List<SubredditInfo> leastUpvotedInfo = new List<SubredditInfo>() { Utility.MakeDefaultSubredditInfo() };

            int mostUpvotedCount = -1;
            int leastUpvotedCount = 999999;

            foreach(var subreddit in iconQuery)
            {
                int count = postsQuery.Count(post => post.Subreddit == subreddit.SubredditName);

                if(count >= mostUpvotedCount)
                {
                    if(count > mostUpvotedCount)
                    {
                        mostUpvotedInfo.Clear();
                    }

                    mostUpvotedCount = count;
                    mostUpvotedInfo.Add(subreddit);
                }

                else if(count <= leastUpvotedCount)
                {
                    if(count < leastUpvotedCount)
                    {
                        leastUpvotedInfo.Clear();
                    }

                    leastUpvotedCount = count;
                    leastUpvotedInfo.Add(subreddit);
                }

                model.Add(subreddit, count);
            }

            SubredditsViewModel vm = new SubredditsViewModel
            {
                Subreddits = model,
                MostUpvoted = mostUpvotedInfo,
                MostUpvotedCount = mostUpvotedCount,
                LeastUpvoted = leastUpvotedInfo,
                LeastUpvotedCount = leastUpvotedCount
            };

            return vm;
        }
    }
}
