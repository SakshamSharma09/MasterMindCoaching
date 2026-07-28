using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasterMind.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class GuidedFinanceSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FirstDueDate",
                table: "StudentFees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Frequency",
                table: "StudentFees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccurrenceKey",
                table: "StudentFees",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PeriodEnd",
                table: "StudentFees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PeriodStart",
                table: "StudentFees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecurrenceIntervalMonths",
                table: "StudentFees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ScheduleEndDate",
                table: "StudentFees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Expenses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccurrenceKey",
                table: "Expenses",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentExpenseId",
                table: "Expenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "Expenses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodEnd",
                table: "Expenses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PeriodStart",
                table: "Expenses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecurrenceIntervalMonths",
                table: "Expenses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentFees_OccurrenceKey",
                table: "StudentFees",
                column: "OccurrenceKey",
                unique: true,
                filter: "[OccurrenceKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_OccurrenceKey",
                table: "Expenses",
                column: "OccurrenceKey",
                unique: true,
                filter: "[OccurrenceKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ParentExpenseId",
                table: "Expenses",
                column: "ParentExpenseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Expenses_ParentExpenseId",
                table: "Expenses",
                column: "ParentExpenseId",
                principalTable: "Expenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Expenses_ParentExpenseId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_StudentFees_OccurrenceKey",
                table: "StudentFees");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_OccurrenceKey",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_ParentExpenseId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "FirstDueDate",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "OccurrenceKey",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "PeriodEnd",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "PeriodStart",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "RecurrenceIntervalMonths",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "ScheduleEndDate",
                table: "StudentFees");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "OccurrenceKey",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ParentExpenseId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "PeriodEnd",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "PeriodStart",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "RecurrenceIntervalMonths",
                table: "Expenses");
        }
    }
}
