using Microsoft.EntityFrameworkCore.Migrations;

namespace RedditPosts.Migrations.SubredditInfoMigrations
{
    public partial class IsNsfw : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNsfw",
                table: "SubredditInfo",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNsfw",
                table: "SubredditInfo");
        }
    }
}
