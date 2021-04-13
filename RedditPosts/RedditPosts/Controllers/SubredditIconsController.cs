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
    public class SubredditIconsController : Controller
    {
        private readonly SubredditInfoContext _subredditContext;
        private readonly IConfiguration _configuration;

        public SubredditIconsController(SubredditInfoContext subredditContext, IConfiguration configuration)
        {
            _subredditContext = subredditContext;
            _configuration = configuration;

            Utility.Initialize(_subredditContext);
        }

        private bool HasPasswordAlready()
        {
            string passValue = HttpContext.Session.GetString(_configuration.GetConnectionString("PasswordKey"));

            if (!string.IsNullOrEmpty(passValue))
            {
                return passValue == "\"" + _configuration.GetConnectionString("Password") + "\"";
            }

            return false;
        }

        public IActionResult Index()
        {
            if(!HasPasswordAlready())
            {
                return RedirectToAction("Index", "Password", new { redirectTo = "Icons" });
            }

            var iconQuery = from m in _subredditContext.SubredditInfo select m;
            iconQuery = iconQuery.OrderBy(subreddit => subreddit.SubredditName.ToLower());

            return View(iconQuery.ToList());
        }
    }
}
