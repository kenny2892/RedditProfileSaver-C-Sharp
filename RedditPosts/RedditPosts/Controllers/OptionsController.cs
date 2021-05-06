using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RedditPosts.Data;

namespace RedditPosts.Controllers
{
    public class OptionsController : BaseController
    {
        public OptionsController(RedditPostContext redditPostContext, SubredditInfoContext subredditInfoContext, IConfiguration configuration) : base(redditPostContext, subredditInfoContext, configuration)
        {

        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
