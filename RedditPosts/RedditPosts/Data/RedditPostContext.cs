using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RedditPosts.Models;

namespace RedditPosts.Data
{
    public class RedditPostContext : DbContext
    {
        public RedditPostContext (DbContextOptions<RedditPostContext> options)
            : base(options)
        {
        }

        public DbSet<RedditPost> RedditPost { get; set; }
    }
}
