using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace RedditPosts.Controllers
{
    public class PasswordController : Controller
    {
        private readonly IConfiguration _configuration;
        public PasswordController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index(string password = "")
        {
            if(HasPasswordAlready() || password == _configuration.GetConnectionString("Password"))
            {
                if(password == _configuration.GetConnectionString("Password"))
                {
                    var serialisedPass = JsonConvert.SerializeObject(_configuration.GetConnectionString("Password"));
                    HttpContext.Session.SetString(_configuration.GetConnectionString("PasswordKey"), serialisedPass);
                }

                return RedirectToAction("Index", "RedditPosts");
            }

            return View();
        }

        private bool HasPasswordAlready()
        {
            string passValue = HttpContext.Session.GetString(_configuration.GetConnectionString("PasswordKey"));

            if(!string.IsNullOrEmpty(passValue))
            {
                return passValue == "\"" + _configuration.GetConnectionString("Password") + "\"";
            }

            return false;
        }
    }
}
