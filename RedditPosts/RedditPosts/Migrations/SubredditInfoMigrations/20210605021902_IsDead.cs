using Microsoft.EntityFrameworkCore.Migrations;

namespace RedditPosts.Migrations.SubredditInfoMigrations
{
    public partial class IsDead : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDead",
                table: "SubredditInfo",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDead",
                table: "SubredditInfo");
        }
    }
}
