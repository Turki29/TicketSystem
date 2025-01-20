using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketSystem.Migrations
{
    /// <inheritdoc />
    public partial class AssigningFKsForUserSetionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserSections_SectionId",
                table: "UserSections",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSections_AspNetUsers_UserId",
                table: "UserSections",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSections_Sections_SectionId",
                table: "UserSections",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSections_AspNetUsers_UserId",
                table: "UserSections");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSections_Sections_SectionId",
                table: "UserSections");

            migrationBuilder.DropIndex(
                name: "IX_UserSections_SectionId",
                table: "UserSections");
        }
    }
}
