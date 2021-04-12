using Microsoft.EntityFrameworkCore.Migrations;

namespace RedditPosts.Migrations.SubredditInfoMigrations
{
    public partial class PrimaryColor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                table: "SubredditInfo",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryColor",
                table: "SubredditInfo");
        }
    }
}
