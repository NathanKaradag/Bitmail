using Microsoft.EntityFrameworkCore.Migrations;

namespace Bitmail.Data.Migrations
{
    public partial class addedHTMLToCampaign : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignTags_Campaigns_CampaignId",
                table: "CampaignTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Campaigns",
                table: "Campaigns");

            migrationBuilder.RenameTable(
                name: "Campaigns",
                newName: "Campaign");

            migrationBuilder.AddColumn<string>(
                name: "HTML",
                table: "Campaign",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Campaign",
                table: "Campaign",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignTags_Campaign_CampaignId",
                table: "CampaignTags",
                column: "CampaignId",
                principalTable: "Campaign",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignTags_Campaign_CampaignId",
                table: "CampaignTags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Campaign",
                table: "Campaign");

            migrationBuilder.DropColumn(
                name: "HTML",
                table: "Campaign");

            migrationBuilder.RenameTable(
                name: "Campaign",
                newName: "Campaigns");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Campaigns",
                table: "Campaigns",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignTags_Campaigns_CampaignId",
                table: "CampaignTags",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
