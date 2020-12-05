using Microsoft.EntityFrameworkCore.Migrations;

namespace Bitmail.Data.Migrations
{
    public partial class addedListid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ListId",
                table: "Campaigns",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ListId",
                table: "Campaigns");
        }
    }
}
