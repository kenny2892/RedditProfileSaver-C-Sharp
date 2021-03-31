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
    public class RedditPostsController : Controller
    {
        private readonly RedditPostContext _redditPostContext;
        private readonly SubredditInfoContext _subredditContext;
        private readonly IConfiguration _configuration;
        private const int BATCH_SIZE = 25;

        public RedditPostsController(RedditPostContext redditPostContext, SubredditInfoContext subredditContext, IConfiguration configuration)
        {
            _redditPostContext = redditPostContext;
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

        private bool IsMobile()
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

        public IActionResult Index(RedditViewModel vm)
        {
            if(!HasPasswordAlready())
            {
                return RedirectToAction("Index", "Password");
            }

            vm.IsMobile = IsMobile();
            return View(vm);
        }

        [HttpPost]
        // Source for Keyword Search: https://stackoverflow.com/a/31383256 & https://stackoverflow.com/a/57971185
        public IActionResult _RedditPosts(RedditViewModel vm, int firstItem = 0)
        {
            if (!HasPasswordAlready())
            {
                return RedirectToAction("Index", "ShowPassword");
            }

            var postsQuery = from m in _redditPostContext.RedditPost select m;
            var postsEnumerable = postsQuery.ToList().AsEnumerable();

            RedditPostFilter filter = new RedditPostFilter(vm, postsEnumerable);
            postsEnumerable = filter.FilterPosts();

            var model = postsEnumerable.Skip(firstItem).Take(BATCH_SIZE).ToList();
            if (model.Count() == 0) return StatusCode(204);  // 204 := "No Content"

            return PartialView(model);
        }

        // GET: RedditPosts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redditPost = await _redditPostContext.RedditPost
                .FirstOrDefaultAsync(m => m.ID == id);
            if (redditPost == null)
            {
                return NotFound();
            }

            return View(redditPost);
        }

        // GET: RedditPosts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RedditPosts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Number,Title,Author,Subreddit,Date,UrlContent,UrlPost,UrlThumbnail,IsSaved,IsNsfw")] RedditPost redditPost)
        {
            if (ModelState.IsValid)
            {
                _redditPostContext.Add(redditPost);
                await _redditPostContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(redditPost);
        }

        public bool Hide(int? id)
        {
            if(id == null)
            {
                return false;
            }

            var redditPost = _redditPostContext.RedditPost.Find(id);
            if(redditPost == null)
            {
                return false;
            }

            redditPost.Hidden = !redditPost.Hidden;
            _redditPostContext.Update(redditPost);
            _redditPostContext.SaveChangesAsync();

            System.Diagnostics.Debug.WriteLine("Hide Toggled for Post Id:" + id + " Post Hidden Value: " + redditPost.Hidden);
            return redditPost.Hidden;
        }

        // GET: RedditPosts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redditPost = await _redditPostContext.RedditPost.FindAsync(id);
            if (redditPost == null)
            {
                return NotFound();
            }
            return View(redditPost);
        }

        // POST: RedditPosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Number,Title,Author,Subreddit,Hidden,Date,UrlContent,UrlPost,UrlThumbnail,IsSaved,IsNsfw")] RedditPost redditPost)
        {
            if (id != redditPost.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _redditPostContext.Update(redditPost);
                    await _redditPostContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RedditPostExists(redditPost.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(redditPost);
        }

        // GET: RedditPosts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redditPost = await _redditPostContext.RedditPost
                .FirstOrDefaultAsync(m => m.ID == id);
            if (redditPost == null)
            {
                return NotFound();
            }

            return View(redditPost);
        }

        // POST: RedditPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var redditPost = await _redditPostContext.RedditPost.FindAsync(id);
            _redditPostContext.RedditPost.Remove(redditPost);
            await _redditPostContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RedditPostExists(int id)
        {
            return _redditPostContext.RedditPost.Any(e => e.ID == id);
        }
    }
}
