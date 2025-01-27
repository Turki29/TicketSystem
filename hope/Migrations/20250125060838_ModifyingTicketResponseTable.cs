using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketSystem.Migrations
{
    /// <inheritdoc />
    public partial class ModifyingTicketResponseTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTech",
                table: "TicketResponses");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateSent",
                table: "TicketResponses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "SenderName",
                table: "TicketResponses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateSent",
                table: "TicketResponses");

            migrationBuilder.DropColumn(
                name: "SenderName",
                table: "TicketResponses");

            migrationBuilder.AddColumn<bool>(
                name: "IsTech",
                table: "TicketResponses",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
