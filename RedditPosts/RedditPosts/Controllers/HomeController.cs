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
                Utility.Print("Already running script!");
                return;
            }

            RunningScript = true;

            Thread startPythonThread = new Thread(RetrieveUpvotes);
            startPythonThread.Start();

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
                Utility.Print("Already active!");
                return;
            }

            string pythonPath = GetPythonExePath();
            string scriptPath = GetRedditScriptPath();

            var process = new Process // Source: https://stackoverflow.com/a/53380763
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "\"" + pythonPath + "\"",
                    Arguments = "\"" + scriptPath + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };
            process.ErrorDataReceived += Process_OutputDataReceived;
            process.OutputDataReceived += Process_OutputDataReceived;

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if(e is null || e.Data is null)
            {
                return;
            }

            Utility.Print(e.Data);

            if(e.Data.Contains("as been stored."))
            {
                UpvoteCount++;
            }

            else if(e.Data.Contains("Completed"))
            {
                RetrievingUpvotes = false;
                RunningScript = false;
                UpvoteCount = 0;
            }
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
            //return GetRedditScriptDirPath() + @"\PrintLoop.py";
        }

        public string GetResultsPath()
        {
            return GetRedditScriptDirPath() + @"\Results.txt";
        }
    }
}
