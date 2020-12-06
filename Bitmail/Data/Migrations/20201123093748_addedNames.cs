using Microsoft.EntityFrameworkCore.Migrations;

namespace Bitmail.Data.Migrations
{
    public partial class addedNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Contacts",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Contacts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Contacts");
        }
    }
}
