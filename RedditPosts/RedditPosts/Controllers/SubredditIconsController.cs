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
    public class SubredditIconsController : BaseController
    {
        public SubredditIconsController(RedditPostContext redditPostContext, SubredditInfoContext subredditInfoContext, IConfiguration configuration) : base(redditPostContext, subredditInfoContext, configuration)
        {

        }

        public IActionResult Index()
        {
            if(!HasPasswordAlready())
            {
                return RedirectToAction("Index", "Password", new { redirectTo = "Icons" });
            }

            var iconQuery = from m in _subredditInfoContext.SubredditInfo select m;
            iconQuery = iconQuery.OrderBy(subreddit => subreddit.SubredditName.ToLower());

            return View(iconQuery.ToList());
        }
    }
}
