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

        public IActionResult Index(SubredditPageViewModel vm)
        {
            if(!HasPasswordAlready())
            {
                return RedirectToAction("Index", "Password", new { redirectTo = "Subreddits" });
            }  

            return View(vm);
        }

        public IActionResult _Subreddits(SubredditPageViewModel vm)
        {
            if(!HasPasswordAlready())
            {
                return RedirectToAction("Index", "Password", new { redirectTo = "Subreddits" });
            }

            IQueryable<RedditPost> postsQuery = from m in _redditPostContext.RedditPost select m;
            var subredditNames = postsQuery.Select(post => post.Subreddit).Distinct().OrderBy(subredditName => subredditName.ToLower()).ToList();

            var iconQuery = from m in _subredditInfoContext.SubredditInfo select m;
            var subredditsAlreadyStored = iconQuery.Select(subreddit => subreddit.SubredditName).ToList();
            var subredditsToRetrieve = subredditNames.Where(subName => !subredditsAlreadyStored.Contains(subName)).ToList();
            foreach(string subredditName in subredditsToRetrieve)
            {
                GetSubredditInfo(subredditName);
            }

            SubredditsViewModel itemsVm = CreateViewModel(postsQuery, vm.SortingSetting, vm.NsfwSetting);

            return PartialView(itemsVm);
        }

        private SubredditsViewModel CreateViewModel(IQueryable<RedditPost> postsQuery, SubredditSortingSettings sortingSetting, NsfwSettings nsfwSetting)
        {
            var iconQuery = from m in _subredditInfoContext.SubredditInfo select m;

            switch(nsfwSetting)
            {
                case NsfwSettings.No_Nsfw:
                    iconQuery = iconQuery.Where(sub => !sub.IsNsfw);
                    break;

                case NsfwSettings.Nsfw_Only:
                    iconQuery = iconQuery.Where(sub => sub.IsNsfw);
                    break;
            }

            Dictionary<SubredditInfo, int> subredditCountDict = new Dictionary<SubredditInfo, int>();

            List<SubredditInfo> mostUpvotedInfo = new List<SubredditInfo>() { Utility.MakeDefaultSubredditInfo() };
            List<SubredditInfo> leastUpvotedInfo = new List<SubredditInfo>() { Utility.MakeDefaultSubredditInfo() };

            int mostUpvotedCount = -1;
            int leastUpvotedCount = 999999;
            int totalSubCount = 0;

            foreach(var subreddit in iconQuery)
            {
                totalSubCount++;
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

                subredditCountDict.Add(subreddit, count);
            }

            var iconEnumerable = iconQuery.AsEnumerable();

            switch(sortingSetting)
            {
                case SubredditSortingSettings.Alphabetical:
                    iconEnumerable = iconEnumerable.OrderBy(subreddit => subreddit.SubredditName.ToLower());
                    break;

                case SubredditSortingSettings.Reverse_Alphabetical:
                    iconEnumerable = iconEnumerable.OrderByDescending(subreddit => subreddit.SubredditName.ToLower());
                    break;

                case SubredditSortingSettings.Count:
                    iconEnumerable = iconEnumerable.OrderByDescending(subreddit => subredditCountDict.GetValueOrDefault(subreddit));
                    break;

                case SubredditSortingSettings.Reverse_Count:
                    iconEnumerable = iconEnumerable.OrderBy(subreddit => subredditCountDict.GetValueOrDefault(subreddit));
                    break;
            }

            SubredditsViewModel vm = new SubredditsViewModel
            {
                Subreddits = iconEnumerable.ToList(),
                SubredditCountDict = subredditCountDict,
                MostUpvoted = mostUpvotedInfo,
                MostUpvotedCount = mostUpvotedCount,
                LeastUpvoted = leastUpvotedInfo,
                LeastUpvotedCount = leastUpvotedCount,
                TotalSubredditCount = totalSubCount
            };

            return vm;
        }
    }
}
