using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RedditPosts.Data;

namespace RedditPosts.Controllers
{
    public class PasswordController : BaseController
    {
        public PasswordController(RedditPostContext redditPostContext, SubredditInfoContext subredditInfoContext, IConfiguration configuration) : base(redditPostContext, subredditInfoContext, configuration)
        {

        }

        public IActionResult Index(string password = "", string redirectTo = "Posts")
        {
            if(HasPasswordAlready() || password == GetConnectionString("Password"))
            {
                if(password == GetConnectionString("Password"))
                {
                    var serialisedPass = JsonConvert.SerializeObject(GetConnectionString("Password"));
                    SetSessionString(GetConnectionString("PasswordKey"), serialisedPass);
                }

                if(redirectTo == "Icons")
                {
                    return RedirectToAction("Index", "SubredditIcons");
                }

                return RedirectToAction("Index", "RedditPosts");
            }

            return View("Index" , redirectTo);
        }
    }
}
