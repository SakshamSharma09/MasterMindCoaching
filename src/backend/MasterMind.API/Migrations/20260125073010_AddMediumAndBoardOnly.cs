using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasterMind.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMediumAndBoardOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Board",
                table: "Classes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "CBSE");

            migrationBuilder.AddColumn<string>(
                name: "Medium",
                table: "Classes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "English");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Board",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "Medium",
                table: "Classes");
        }
    }
}
