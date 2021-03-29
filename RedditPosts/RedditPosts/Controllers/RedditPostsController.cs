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
        private readonly RedditPostContext _context;
        private readonly IConfiguration _configuration;
        private const int BATCH_SIZE = 25;

        private RedditViewModel RedditViewModel { get; set; }

        public RedditPostsController(RedditPostContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            RedditViewModel = new RedditViewModel();
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

            RedditViewModel = vm;
            RedditViewModel.IsMobile = IsMobile();
            return View(RedditViewModel);
        }

        [HttpPost]
        public IActionResult _RedditPosts(RedditViewModel vm, int firstItem = 0) // Source for Keyword Search: https://stackoverflow.com/a/31383256 & https://stackoverflow.com/a/57971185
        {
            if (!HasPasswordAlready())
            {
                return RedirectToAction("Index", "ShowPassword");
            }

            RedditViewModel = vm;
            var postsQuery = from m in _context.RedditPost select m;
            var posts = postsQuery.ToList().AsEnumerable();

            if (!String.IsNullOrEmpty(RedditViewModel.TitleFilter))
            {
                List<string> keywords = new List<string> { RedditViewModel.TitleFilter };

                if(RedditViewModel.TitleFilter.Contains(" "))
                {
                    keywords = RedditViewModel.TitleFilter.Split(" ").ToList();
                }

                posts = posts.Where(post => keywords.Any(word => post.Title.ToLower().Contains(word.ToLower())));
            }

            if (!String.IsNullOrEmpty(RedditViewModel.AuthorFilter))
            {
                List<string> keywords = new List<string> { RedditViewModel.AuthorFilter };

                if (RedditViewModel.AuthorFilter.Contains(" "))
                {
                    keywords = RedditViewModel.AuthorFilter.Split(" ").ToList();
                }

                posts = posts.Where(post => keywords.Any(word => post.Author.ToLower().Contains(word.ToLower())));
            }

            if (!String.IsNullOrEmpty(RedditViewModel.SubredditFilter))
            {
                List<string> keywords = new List<string> { RedditViewModel.SubredditFilter };

                if (RedditViewModel.SubredditFilter.Contains(" "))
                {
                    keywords = RedditViewModel.SubredditFilter.Split(" ").ToList();
                }

                posts = posts.Where(post => keywords.Any(word => post.Subreddit.ToLower().Contains(word.ToLower())));
            }

            switch (RedditViewModel.NsfwSetting)
            {
                case NsfwSettings.No_Filter:
                    break;

                case NsfwSettings.Nsfw_Only:
                    posts = posts.Where(s => s.IsNsfw);
                    break;

                case NsfwSettings.No_Nsfw:
                    posts = posts.Where(s => !s.IsNsfw);
                    break;
            }

            if (RedditViewModel.SavedOnly)
            {
                posts = posts.Where(s => s.IsSaved == true);
            }

            // Extract a portion of data
            if (posts.Count() > 1 && posts.Count() - 1 == firstItem && BATCH_SIZE >= posts.Count())
            {
                firstItem = posts.Count();
            }

            if (firstItem != 0)
            {
                firstItem += 1;
            }

            List<RedditPost> postList = ContentTypeWhiteList(posts.ToList());

            if (RedditViewModel.Randomize)
            {
                Random r = new Random(RedditViewModel.RandomizeSeed);

                int curr = postList.Count;
                while (curr > 1)
                {
                    curr--;
                    int toSwap = r.Next(curr + 1);
                    RedditPost value = postList[toSwap];
                    postList[toSwap] = postList[curr];
                    postList[curr] = value;
                }
            }

            else
            {
                postList = postList.OrderByDescending(post => post.Number).ToList();
            }

            var model = postList.Skip(firstItem).Take(BATCH_SIZE).ToList();
            if (model.Count() == 0) return StatusCode(204);  // 204 := "No Content"

            RedditViewModel.RedditPosts = model;
            return PartialView(RedditViewModel.RedditPosts);
        }

        private List<RedditPost> ContentTypeWhiteList(List<RedditPost> posts)
        {
            for(int i = posts.Count - 1; i >= 0; i--)
            {
                RedditPost post = posts[i];
                ContentType type = post.GetContentType();

                if(!RedditViewModel.Mp4 && type == ContentType.Mp4)
                {
                    posts.RemoveAt(i);
                }

                else if(!RedditViewModel.Twitter && type == ContentType.Twitter)
                {
                    posts.RemoveAt(i);
                }

                else if(!RedditViewModel.Youtube && type == ContentType.Youtube)
                {
                    posts.RemoveAt(i);
                }

                else if(!RedditViewModel.Image && type == ContentType.Image)
                {
                    posts.RemoveAt(i);
                }

                else if(!RedditViewModel.Gifv && type == ContentType.Gifv)
                {
                    posts.RemoveAt(i);
                }

                else if(!RedditViewModel.ImgurGallery && type == ContentType.ImgurGallery)
                {
                    posts.RemoveAt(i);
                }

                else if(!RedditViewModel.GfyCat && type == ContentType.GfyCat)
                {
                    posts.RemoveAt(i);
                }

                else if(!RedditViewModel.RedGifWatch && type == ContentType.RedGifWatch)
                {
                    posts.RemoveAt(i);
                }

                else if(!RedditViewModel.Gallery && type == ContentType.Gallery)
                {
                    posts.RemoveAt(i);
                }

                else if(!RedditViewModel.Vreddit && type == ContentType.Vreddit)
                {
                    posts.RemoveAt(i);
                }

                else if(!RedditViewModel.UrlPreview && type == ContentType.UrlPreview)
                {
                    posts.RemoveAt(i);
                }

                else if(!RedditViewModel.Blank && type == ContentType.Blank)
                {
                    posts.RemoveAt(i);
                }
            }

            return posts;
        }

        // GET: RedditPosts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redditPost = await _context.RedditPost
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
                _context.Add(redditPost);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(redditPost);
        }

        // GET: RedditPosts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var redditPost = await _context.RedditPost.FindAsync(id);
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
        public async Task<IActionResult> Edit(int id, [Bind("ID,Number,ShuffleNum,Title,Author,Subreddit,Date,UrlContent,UrlPost,UrlThumbnail,IsSaved,IsNsfw")] RedditPost redditPost)
        {
            if (id != redditPost.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(redditPost);
                    await _context.SaveChangesAsync();
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

            var redditPost = await _context.RedditPost
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
            var redditPost = await _context.RedditPost.FindAsync(id);
            _context.RedditPost.Remove(redditPost);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RedditPostExists(int id)
        {
            return _context.RedditPost.Any(e => e.ID == id);
        }
    }
}
