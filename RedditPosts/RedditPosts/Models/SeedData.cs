using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RedditPosts.Data;
using RedditPosts.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RedditPosts.Models
{
    public class SeedData
    {
        private static IConfiguration Configuration { get; set; }

        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (RedditPostContext context = new RedditPostContext(serviceProvider.GetRequiredService<DbContextOptions<RedditPostContext>>()))
            {
                //if (context.RedditPost.Any())
                //{
                //    return;   // DB has been seeded
                //}

                Configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", true, true)
               .Build();

                List<RedditPost> posts = GeneratePosts();

                var newIds = posts.Select(p => p.Number).Distinct().ToArray();
                var oldIds = context.RedditPost.Where(p => newIds.Contains(p.Number)).Select(p => p.Number).ToArray();
                var idsToAdd = posts.Where(p => !oldIds.Contains(p.Number)).ToList();

                if (idsToAdd.Count > 0)
                {
                    context.RedditPost.AddRange(idsToAdd);
                    context.SaveChanges();
                }

                //UpdateContentUrlWithinDatabase(context, posts);
            }
        }

        private static void UpdateContentUrlWithinDatabase(RedditPostContext context, List<RedditPost> posts)
        {
            IQueryable<RedditPost> postsQuery = from m in context.RedditPost select m;
            IEnumerable<RedditPost> postsEnumerable = postsQuery.ToList().AsEnumerable();

            foreach(RedditPost fromJson in posts)
            {
                RedditPost fromDatabase = postsEnumerable.Where(post => post.Number == fromJson.Number).FirstOrDefault();

                if(fromJson.UrlContent != fromDatabase.UrlContent)
                {
                    fromDatabase.UrlContent = fromJson.UrlContent;
                    context.Update(fromDatabase);
                    context.SaveChanges();
                }
            }
        }

        public static List<RedditPost> GeneratePosts()
        {
            List<RedditPost> posts = new List<RedditPost>();

            if (Configuration is null)
            {
                return posts;
            }

            using (StreamReader file = File.OpenText(Configuration.GetConnectionString("PostsJson")))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    JObject o2 = (JObject)JToken.ReadFrom(reader);
                    int count = 1;

                    foreach (var elements in o2)
                    {
                        var post = elements.Value;

                        string postName = "";
                        string author = "";
                        string subreddit = "";
                        DateTime dateCreated = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        string urlContent = "";
                        string urlPost = "";
                        string urlThumbnail = "";
                        bool isSaved = false;
                        bool isNsfw = false;
                        int number = 0;

                        foreach (JProperty value in post)
                        {
                            switch (value.Name)
                            {
                                case "Post Name":
                                    postName = value.Value.ToString();
                                    break;

                                case "Author":
                                    author = value.Value.ToString();
                                    break;

                                case "Subreddit":
                                    subreddit = value.Value.ToString();
                                    break;

                                case "Created (in Seconds)":
                                    dateCreated = dateCreated.AddSeconds(value.Value.Value<double>());
                                    break;

                                case "Url to Content":
                                    urlContent = value.Value.ToString();
                                    break;

                                case "Url to Post":
                                    urlPost = value.Value.ToString();
                                    break;

                                case "Url to Thumbnail":
                                    urlThumbnail = value.Value.ToString();
                                    break;

                                case "Is Saved":
                                    isSaved = (bool)value.Value;
                                    break;

                                case "Is NSFW":
                                    isNsfw = (bool)value.Value;
                                    break;

                                case "Number":
                                    if (Int32.TryParse(value.Value.ToString(), out int num))
                                    {
                                        number = num;
                                    }
                                    break;
                            }
                        }

                        posts.Add(
                            new RedditPost
                            {
                                Title = postName,
                                Number = number,
                                Author = author,
                                Subreddit = subreddit,
                                Date = dateCreated,
                                UrlContent = urlContent,
                                UrlPost = urlPost,
                                UrlThumbnail = urlThumbnail,
                                IsSaved = isSaved,
                                IsNsfw = isNsfw
                            });

                        Console.WriteLine("Post #" + count + " created.");
                        count++;
                    }
                }
            }

            return posts;
        }
    }
}
