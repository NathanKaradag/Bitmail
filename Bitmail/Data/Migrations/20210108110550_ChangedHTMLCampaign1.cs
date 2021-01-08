using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bitmail.Data.Migrations
{
    public partial class ChangedHTMLCampaign1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignHistory_Campaigns_CampaignId",
                table: "CampaignHistory");

            migrationBuilder.DropIndex(
                name: "IX_CampaignHistory_CampaignId",
                table: "CampaignHistory");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "CampaignHistory");

            migrationBuilder.AddColumn<string>(
                name: "Contacts",
                table: "CampaignHistory",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "CampaignHistory",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CampaignHistory",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubjectLine",
                table: "CampaignHistory",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "CampaignHistory",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "CampaignHistory",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contacts",
                table: "CampaignHistory");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "CampaignHistory");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CampaignHistory");

            migrationBuilder.DropColumn(
                name: "SubjectLine",
                table: "CampaignHistory");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "CampaignHistory");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "CampaignHistory");

            migrationBuilder.AddColumn<int>(
                name: "CampaignId",
                table: "CampaignHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignHistory_CampaignId",
                table: "CampaignHistory",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignHistory_Campaigns_CampaignId",
                table: "CampaignHistory",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
