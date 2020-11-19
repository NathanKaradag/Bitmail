using Microsoft.EntityFrameworkCore.Migrations;

namespace Bitmail.Data.Migrations
{
    public partial class addedCampaigns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MailChimpId = table.Column<string>(nullable: true),
                    TemplateId = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    SubjectLine = table.Column<string>(nullable: true),
                    PreviewText = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    FromName = table.Column<string>(nullable: true),
                    ReplyTo = table.Column<string>(nullable: true),
                    ToName = table.Column<string>(nullable: true),
                    CampaignTagsId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CampaignTags",
                columns: table => new
                {
                    TagId = table.Column<int>(nullable: false),
                    CampaignId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignTags", x => new { x.CampaignId, x.TagId });
                    table.ForeignKey(
                        name: "FK_CampaignTags_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTags_TagId",
                table: "CampaignTags",
                column: "TagId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignTags");

            migrationBuilder.DropTable(
                name: "Campaigns");
        }
    }
}
