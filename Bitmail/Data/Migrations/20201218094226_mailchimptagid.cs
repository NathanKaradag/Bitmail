using Microsoft.EntityFrameworkCore.Migrations;

namespace Bitmail.Data.Migrations
{
    public partial class mailchimptagid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MailChimpTagId",
                table: "Tags",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Organisations",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Organisations",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MailChimpTagId",
                table: "Tags");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Organisations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Organisations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
