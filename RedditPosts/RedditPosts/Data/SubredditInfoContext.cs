using Microsoft.EntityFrameworkCore;
using RedditPosts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedditPosts.Data
{
    public class SubredditInfoContext : DbContext
    {
        public SubredditInfoContext(DbContextOptions<SubredditInfoContext> options)
            : base(options)
        {
        }

        public DbSet<SubredditInfo> SubredditInfo { get; set; }
    }
}
