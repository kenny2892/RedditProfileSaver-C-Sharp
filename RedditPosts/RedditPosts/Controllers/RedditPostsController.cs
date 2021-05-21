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
    public class RedditPostsController : BaseController
    {
        private const int BATCH_SIZE = 25;

        public RedditPostsController(RedditPostContext redditPostContext, SubredditInfoContext subredditInfoContext, IConfiguration configuration) : base(redditPostContext, subredditInfoContext, configuration)
        {

        }

        public IActionResult Index(RedditViewModel vm)
        {
            if(!HasPasswordAlready())
            {
                return RedirectToAction("Index", "Password");
            }

            vm.IsMobile = IsMobile();

            var posts = RetrieveFilteredPosts(vm);

            if(!vm.ShowHidden)
            {
                posts = posts.Where(post => !post.Hidden);
            }

            vm.PostCount = posts.Count();
            
            if(vm.PostCount > 0 && vm.MaxPostNumber < 0)
            {
                vm.MaxPostNumber = posts.Max(post => post.Number);
            }

            return View(vm);
        }

        [HttpPost]
        // Source for Keyword Search: https://stackoverflow.com/a/31383256 & https://stackoverflow.com/a/57971185
        public IActionResult _RedditPosts(RedditViewModel vm, int lastRowNumber = -1)
        {
            if(!HasPasswordAlready()) // If so, then they had the password at one point but the server lost their session
            {
                RedditPostsViewModel emptyModel = new RedditPostsViewModel()
                {
                    Posts = new List<RedditPost>(),
                    Subreddits = new Dictionary<string, SubredditInfo>()
                };

                return PartialView(emptyModel);
            }

            IEnumerable<RedditPost> filteredPosts = RetrieveFilteredPosts(vm);

            int toSkip = 0;
            if(lastRowNumber > -1)
            {
                int indexToSkip = filteredPosts.ToList().FindIndex(post => post.Number == lastRowNumber);
                toSkip = indexToSkip > -1 ? indexToSkip + 1 : 0;
                filteredPosts = filteredPosts.Skip(toSkip);
            }

            if(!vm.ShowHidden)
            {
                filteredPosts = filteredPosts.Where(post => !post.Hidden);
            }

            List<RedditPost> postsModel = filteredPosts.Take(BATCH_SIZE).ToList();
            if (postsModel.Count() == 0) return StatusCode(204);  // 204 := "No Content"

            CheckSubredditInfos(postsModel);

            RedditPostsViewModel model = new RedditPostsViewModel()
            {
                Posts = postsModel,
                Subreddits = GetSubredditDictionary()
            };

            return PartialView(model);
        }

        private IEnumerable<RedditPost> RetrieveFilteredPosts(RedditViewModel vm)
        {
            IQueryable<RedditPost> postsQuery = from m in _redditPostContext.RedditPost select m;
            IEnumerable<RedditPost> postsEnumerable = postsQuery.ToList().AsEnumerable();
            List<SubredditInfo> subreddits = null;

            if(vm.SubredditTypes != SubredditTypes.All)
            {
                IQueryable<SubredditInfo> subsQuery = from m in _subredditInfoContext.SubredditInfo select m;
                subreddits = subsQuery.ToList();
            }

            RedditPostFilter filter = new RedditPostFilter(vm, GetCookieString("AllowNsfw") == "false", subreddits);
            return filter.FilterPosts(postsEnumerable);
        }

        private void CheckSubredditInfos(List<RedditPost> posts)
        {
            var subredditNames = posts.Select(post => post.Subreddit).Distinct().ToList();
            foreach(string subredditName in subredditNames)
            {
                GetSubredditInfo(subredditName);
            }
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

            RedditPostsViewModel model = new RedditPostsViewModel()
            {
                Posts = new List<RedditPost>(){ redditPost },
                Subreddits = GetSubredditDictionary()
            };

            return View(model);
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

            Utility.Print("Hide Toggled for Post Id:" + id + " Post Hidden Value: " + redditPost.Hidden);
            return redditPost.Hidden;
        }

        public bool Favorite(int? id)
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

            redditPost.IsFavorited = !redditPost.IsFavorited;
            _redditPostContext.Update(redditPost);
            _redditPostContext.SaveChangesAsync();

            Utility.Print("Favorite Toggled for Post Id:" + id + " Post Favorite Value: " + redditPost.IsFavorited);
            return redditPost.IsFavorited;
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
