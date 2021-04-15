﻿using DeviceDetectorNET;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RedditPosts.Data;
using RedditPosts.Models;
using Reddit.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reddit;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedditPosts.Controllers
{
    public class BaseController : Controller
    {
        protected readonly RedditPostContext _redditPostContext;
        protected readonly SubredditInfoContext _subredditInfoContext;
        protected readonly IConfiguration _configuration;
        private RedditClient RedditClient { get; set; }

        public BaseController(RedditPostContext redditPostContext, SubredditInfoContext subredditInfoContext, IConfiguration configuration)
        {
            _redditPostContext = redditPostContext;
            _subredditInfoContext = subredditInfoContext;
            _configuration = configuration;
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

        protected Dictionary<string, SubredditInfo> GetSubredditDictionary()
        {
            Dictionary<string, SubredditInfo> dict = new Dictionary<string, SubredditInfo>();

            var subsQuery = from m in _subredditInfoContext.SubredditInfo select m;
            var subsNames = subsQuery.Select(subreddit => subreddit.SubredditName);

            foreach(string subredditName in subsNames)
            {
                dict.Add(subredditName, subsQuery.Where(sub => sub.SubredditName == subredditName).First());
            }

            return dict;
        }

        private bool CreateRedditClient()
        {
            if(!(RedditClient is null))
            {
                return true;
            }

            try
            {
                string configPath = _configuration.GetConnectionString("PythonScriptDirectory") + @"\Config.json";
                string id = "";
                string secretId = "";
                string userAgent = "";
                string refreshToken = "";

                using(StreamReader file = System.IO.File.OpenText(configPath))
                {
                    using(JsonTextReader reader = new JsonTextReader(file))
                    {
                        JObject o2 = (JObject)JToken.ReadFrom(reader);

                        foreach(var element in o2)
                        {
                            switch(element.Key)
                            {
                                case "Id":
                                    id = element.Value.ToString();
                                    break;

                                case "Secret":
                                    secretId = element.Value.ToString();
                                    break;

                                case "User Agent":
                                    userAgent = element.Value.ToString();
                                    break;

                                case "Refresh Token":
                                    refreshToken = element.Value.ToString();
                                    break;
                            }
                        }
                    }
                }

                RedditClient = new RedditClient(appId: id, appSecret: secretId, refreshToken: refreshToken, userAgent: userAgent);
            }

            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        protected SubredditInfo GetSubredditInfo(string subredditName)
        {
            SubredditInfo toReturn = Utility.MakeDefaultSubredditInfo();

            if(_subredditInfoContext is null)
            {
                return toReturn;
            }

            var subsQuery = from m in _subredditInfoContext.SubredditInfo select m;
            var results = subsQuery.Where(sub => sub.SubredditName == subredditName);

            if(results.Count() != 1)
            {
                SubredditInfo subredditInfo = RetrieveSubredditInfo(subredditName);

                if(!(subredditInfo is null))
                {
                    _subredditInfoContext.Add(subredditInfo);
                    _subredditInfoContext.SaveChanges();

                    toReturn = subredditInfo;
                }
            }

            else
            {
                toReturn = results.ToList().Cast<SubredditInfo>().ElementAt(0);
            }

            return toReturn;
        }

        public bool UpdateSubredditIcons()
        {
            if(_subredditInfoContext is null || !CreateRedditClient())
            {
                return false;
            }

            var subsQuery = from m in _subredditInfoContext.SubredditInfo select m;
            var subredditList = subsQuery.OrderBy(subreddit => subreddit.SubredditName.ToLower()).Select(subreddit => subreddit.SubredditName);

            foreach(string subredditName in subredditList)
            {
                SubredditInfo subInfo = RetrieveSubredditInfo(subredditName);
                _subredditInfoContext.Update(subInfo);
                _subredditInfoContext.SaveChanges();

                System.Threading.Thread.Sleep(700); // Sleep to avoid overloading the Reddit Api
            }

            System.Diagnostics.Debug.WriteLine("Finished updating Subreddits");
            return true;
        }

        private SubredditInfo RetrieveSubredditInfo(string subredditName)
        {
            if(!CreateRedditClient())
            {
                return Utility.MakeDefaultSubredditInfo();
            }

            try
            {
                System.Diagnostics.Debug.WriteLine("GETTING SUBREDDIT: " + subredditName);

                Subreddit subredditAbout = RedditClient.Subreddit(subredditName).About();

                string primaryColor = !String.IsNullOrEmpty(subredditAbout.PrimaryColor) ? subredditAbout.PrimaryColor : GenerateSubredditColor(subredditName);
                string communityIcon = "";
                string iconImg = "";
                string url = Utility.DefaultSubredditIcon;

                if(!(subredditAbout.SubredditData.CommunityIcon is null)) // Reddit's current icon system
                {
                    communityIcon = subredditAbout.SubredditData.CommunityIcon;

                    if(communityIcon.Contains("&amp;"))
                    {
                        communityIcon = communityIcon.Replace("&amp;", "&");
                    }
                }

                if(!(subredditAbout.SubredditData.IconImg is null)) // Reddit's old icon system (used as a backup and in outlier cases such as User posts)
                {
                    iconImg = subredditAbout.SubredditData.IconImg.ToString();

                    if(iconImg.Contains("&amp;"))
                    {
                        iconImg = iconImg.Replace("&amp;", "&");
                    }
                }

                if(!String.IsNullOrEmpty(iconImg) || !String.IsNullOrEmpty(communityIcon)) // Try and use new system, but if not available, use old one.
                {
                    url = !String.IsNullOrEmpty(communityIcon) ? communityIcon : iconImg;
                }

                return new SubredditInfo { SubredditName = subredditName, IconUrl = url, PrimaryColor = primaryColor };
            }

            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Could not get SubredditInfo: " + subredditName);
                return Utility.MakeDefaultSubredditInfo();
            }
        }

        private string GenerateSubredditColor(string subredditName) // Source: https://stackoverflow.com/a/57726983
        {
            string toReturn = "#000001";

            if(subredditName != null)
            {
                int hash = subredditName.GetHashCode();
                toReturn = $"#{hash:X8}";
            }

            return toReturn;
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