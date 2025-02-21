using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketSystem.Migrations
{
    /// <inheritdoc />
    public partial class addingRoleIdColumnInUserSectionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          

            migrationBuilder.AddColumn<string>(
                name: "RoleId",
                table: "UserSections",
                type: "nvarchar(450)",
                nullable: true);

          

            

            migrationBuilder.CreateIndex(
                name: "IX_UserSections_RoleId",
                table: "UserSections",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSections_AspNetRoles_RoleId",
                table: "UserSections",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSections_AspNetRoles_RoleId",
                table: "UserSections");

            migrationBuilder.DropIndex(
                name: "IX_UserSections_RoleId",
                table: "UserSections");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "UserSections");

          

           
        }
    }
}
