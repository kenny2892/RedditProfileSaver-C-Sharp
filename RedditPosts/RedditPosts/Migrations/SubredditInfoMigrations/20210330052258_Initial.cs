using Microsoft.EntityFrameworkCore.Migrations;

namespace RedditPosts.Migrations.SubredditInfoMigrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubredditInfo",
                columns: table => new
                {
                    SubredditName = table.Column<string>(nullable: false),
                    IconUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubredditInfo", x => x.SubredditName);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubredditInfo");
        }
    }
}
