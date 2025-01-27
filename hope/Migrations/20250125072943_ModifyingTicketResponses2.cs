using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketSystem.Migrations
{
    /// <inheritdoc />
    public partial class ModifyingTicketResponses2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketResponses_AspNetUsers_SenderId",
                table: "TicketResponses");

            migrationBuilder.AddColumn<int>(
                name: "TicketId",
                table: "TicketResponses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TicketResponses_TicketId",
                table: "TicketResponses",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketResponses_AspNetUsers_SenderId",
                table: "TicketResponses",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketResponses_Tickets_TicketId",
                table: "TicketResponses",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketResponses_AspNetUsers_SenderId",
                table: "TicketResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketResponses_Tickets_TicketId",
                table: "TicketResponses");

            migrationBuilder.DropIndex(
                name: "IX_TicketResponses_TicketId",
                table: "TicketResponses");

            migrationBuilder.DropColumn(
                name: "TicketId",
                table: "TicketResponses");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketResponses_AspNetUsers_SenderId",
                table: "TicketResponses",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
