using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Heritage_of_Turkey.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Museum_Category_CategoryId",
                table: "Museum");

            migrationBuilder.DropForeignKey(
                name: "FK_Ruin_Category_CategoryId",
                table: "Ruin");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Category",
                table: "Category");

            migrationBuilder.RenameTable(
                name: "Category",
                newName: "Categories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "CategoryId");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "CategoryName", "CreatedDate", "Description", "IsActive" },
                values: new object[,]
                {
                    { 1, "Archaeology Museum", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Museums featuring archaeological artifacts", true },
                    { 2, "Art Museum", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Museums displaying paintings and sculptures", true },
                    { 3, "Ethnography Museum", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Museums showcasing cultural heritage", true },
                    { 4, "Ancient City", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ruins of ancient cities", true },
                    { 5, "Ancient Theater", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ancient amphitheaters", true },
                    { 6, "Temple Ruins", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ancient temples and religious sites", true }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Museum_Categories_CategoryId",
                table: "Museum",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ruin_Categories_CategoryId",
                table: "Ruin",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Museum_Categories_CategoryId",
                table: "Museum");

            migrationBuilder.DropForeignKey(
                name: "FK_Ruin_Categories_CategoryId",
                table: "Ruin");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 6);

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Category");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Category",
                table: "Category",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Museum_Category_CategoryId",
                table: "Museum",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ruin_Category_CategoryId",
                table: "Ruin",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
