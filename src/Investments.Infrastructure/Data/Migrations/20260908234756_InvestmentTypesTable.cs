using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Investments.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InvestmentTypesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Investments",
                newName: "InvestmentTypeId");

            migrationBuilder.CreateTable(
                name: "InvestmentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestmentTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "InvestmentTypes",
                columns: new[] { "Id", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Acoes", null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "RendaFixa", null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fundos", null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tesouro", null },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cripto", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Investments_InvestmentTypeId",
                table: "Investments",
                column: "InvestmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestmentTypes_Name",
                table: "InvestmentTypes",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Investments_InvestmentTypes_InvestmentTypeId",
                table: "Investments",
                column: "InvestmentTypeId",
                principalTable: "InvestmentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Investments_InvestmentTypes_InvestmentTypeId",
                table: "Investments");

            migrationBuilder.DropTable(
                name: "InvestmentTypes");

            migrationBuilder.DropIndex(
                name: "IX_Investments_InvestmentTypeId",
                table: "Investments");

            migrationBuilder.RenameColumn(
                name: "InvestmentTypeId",
                table: "Investments",
                newName: "Type");
        }
    }
}
