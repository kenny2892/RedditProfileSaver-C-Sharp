using DeviceDetectorNET;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RedditPosts.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedditPosts.Controllers
{
    public class BaseController : Controller
    {
        protected readonly RedditPostContext _redditPostContext;
        protected readonly SubredditInfoContext _subredditInfoContext;
        protected readonly IConfiguration _configuration;

        public BaseController(RedditPostContext redditPostContext, SubredditInfoContext subredditInfoContext, IConfiguration configuration)
        {
            _redditPostContext = redditPostContext;
            _subredditInfoContext = subredditInfoContext;
            _configuration = configuration;

            Utility.Initialize(_subredditInfoContext);
        }

        protected bool HasPasswordAlready()
        {
            string passValue = GetSessionString(GetConnectionString("PasswordKey"));

            if(!string.IsNullOrEmpty(passValue))
            {
                return passValue == "\"" + GetConnectionString("Password") + "\"";
            }

            return false;
        }

        protected bool IsMobile()
        {
            try
            {
                var dd = new DeviceDetector(Request.Headers["User-Agent"].ToString());
                dd.Parse();

                var device = dd.GetDeviceName();

                return device != "desktop";
            }

            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return false;
        }

        public string GetConnectionString(string key)
        {
            if(key is null)
            {
                return "";
            }

            return _configuration.GetConnectionString(key);
        }

        public string GetSessionString(string key)
        {
            if(key is null)
            {
                return "";
            }

            string value = HttpContext.Session.GetString(key);
            return !String.IsNullOrEmpty(value) ? value : "";
        }

        public bool SetSessionString(string key, string value)
        {
            if(key is null || value is null)
            {
                return false;
            }

            HttpContext.Session.SetString(key, value);
            return true;
        }
    }
}
