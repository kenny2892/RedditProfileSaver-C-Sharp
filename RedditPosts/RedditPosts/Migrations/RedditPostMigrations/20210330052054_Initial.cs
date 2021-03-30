using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RedditPosts.Migrations.RedditPostMigrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RedditPost",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Author = table.Column<string>(nullable: true),
                    Subreddit = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    UrlContent = table.Column<string>(nullable: true),
                    UrlPost = table.Column<string>(nullable: true),
                    UrlThumbnail = table.Column<string>(nullable: true),
                    IsSaved = table.Column<bool>(nullable: false),
                    IsNsfw = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RedditPost", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RedditPost");
        }
    }
}
