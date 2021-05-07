using Microsoft.EntityFrameworkCore.Migrations;

namespace RedditPosts.Migrations.RedditPostMigrations
{
    public partial class IsFavorited : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFavorited",
                table: "RedditPost",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFavorited",
                table: "RedditPost");
        }
    }
}
