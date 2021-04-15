using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RedditPosts.Data;
using RedditPosts.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RedditPosts.Controllers
{
    public class HomeController : BaseController
    {
        private static bool RetrievingUpvotes { get; set; } = false;
        private static bool RunningScript { get; set; } = false;
        private static bool FinishedScript { get; set; } = false;
        private static int UpvoteCount { get; set; }

        public HomeController(RedditPostContext redditPostContext, SubredditInfoContext subredditInfoContext, IConfiguration configuration) : base(redditPostContext, subredditInfoContext, configuration)
        {

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public bool IsRetrievingUpvotes()
        {
            return RetrievingUpvotes;
        }

        public int RetrieveUpvoteCount()
        {
            return UpvoteCount;
        }

        public void Retrieve()
        {
            if(RunningScript)
            {
                System.Diagnostics.Debug.WriteLine("Already running script!");
                return;
            }

            RunningScript = true;

            Thread startPythonThread = new Thread(RetrieveUpvotes);
            Thread watchFileThread = new Thread(WatchUpvoteFile);
            startPythonThread.Start();
            watchFileThread.Start();

            Thread.Sleep(100); // Pause to let the thread start before changing condition
            RetrievingUpvotes = true;
        }

        private void EmptyResultsFile()
        {
            string txtPath = GetResultsPath();
            System.IO.File.WriteAllText(txtPath, "Results:\n");
        }

        private void RetrieveUpvotes()
        {
            if (RetrievingUpvotes)
            {
                System.Diagnostics.Debug.WriteLine("Already active!");
                return;
            }

            string pythonPath = GetPythonExePath();
            string scriptPath = GetRedditScriptPath();

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = pythonPath;
            start.Arguments = string.Format("\"{0}\"", scriptPath);
            start.UseShellExecute = false;
            start.CreateNoWindow = true;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;

            using(Process process = Process.Start(start))
            {
                using(StreamReader reader = process.StandardOutput)
                {
                    //string stderr = process.StandardError.ReadToEnd();
                    //string result = reader.ReadToEnd();

                    while (!FinishedScript)
                    {

                    }

                    RunningScript = false;
                    RetrievingUpvotes = false;

                    System.Diagnostics.Debug.WriteLine("Closing Python Thread");
                }
            }
        }

        private void WatchUpvoteFile()
        {
            if(RetrievingUpvotes)
            {
                System.Diagnostics.Debug.WriteLine("Already active 2!");
                return;
            }

            string dirPath = GetRedditScriptDirPath();
            string txtPath = GetResultsPath();
            using var watcher = new FileSystemWatcher(dirPath);

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Size;

            watcher.Path = Path.GetDirectoryName(txtPath);
            watcher.Filter = Path.GetFileName(txtPath);

            watcher.Changed += OnChanged;
            watcher.EnableRaisingEvents = true;

            while(RunningScript)
            {

            }

            System.Diagnostics.Debug.WriteLine("Closing Watch Thread");
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            string txtPath = GetResultsPath();
            var lastLine = System.IO.File.ReadLines(txtPath).Last();
            System.Diagnostics.Debug.WriteLine(lastLine);

            if (lastLine.Contains("as been stored."))
            {
                UpvoteCount++;
            }

            if (lastLine == "Completed")
            {
                FinishedScript = true;
            }
        }

        public bool UpdatePosts()
        {
            bool toReturn = false;

            try
            {
                List<RedditPost> posts = SeedData.GeneratePosts();

                var newIds = posts.Select(p => p.Number).Distinct().ToArray();
                var oldIds = _redditPostContext.RedditPost.Where(p => newIds.Contains(p.Number)).Select(p => p.Number).ToArray();
                var idsToAdd = posts.Where(p => !oldIds.Contains(p.Number)).ToList();

                if(idsToAdd.Count > 0)
                {
                    _redditPostContext.RedditPost.AddRange(idsToAdd);
                    _redditPostContext.SaveChanges();

                    System.Diagnostics.Debug.WriteLine("Added Posts");
                }

                UpvoteCount = 0;
                EmptyResultsFile();
                toReturn = true;
            }

            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("ERROR ADDING POSTS: " + "\n" + e.Message);
            }

            return toReturn;
        }

        public bool UpdateIcons()
        {
            return Utility.UpdateSubredditIcons();
        }

        public string GetPythonExePath()
        {
            return _configuration.GetConnectionString("PythonExe");
        }

        public string GetRedditScriptDirPath()
        {
            return _configuration.GetConnectionString("PythonScriptDirectory");
        }

        public string GetRedditScriptPath()
        {
            return GetRedditScriptDirPath() + @"\Reddit.py";
        }

        public string GetResultsPath()
        {
            return GetRedditScriptDirPath() + @"\Results.txt";
        }
    }
}
