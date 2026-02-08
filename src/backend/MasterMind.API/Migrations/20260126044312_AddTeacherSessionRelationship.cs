using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasterMind.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherSessionRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Sessions_SessionId",
                table: "Classes");

            migrationBuilder.AlterColumn<int>(
                name: "SessionId",
                table: "Classes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Sessions_SessionId",
                table: "Classes",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Sessions_SessionId",
                table: "Classes");

            migrationBuilder.AlterColumn<int>(
                name: "SessionId",
                table: "Classes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Sessions_SessionId",
                table: "Classes",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
