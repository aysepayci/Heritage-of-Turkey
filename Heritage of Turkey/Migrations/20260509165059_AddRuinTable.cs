using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Heritage_of_Turkey.Migrations
{
    /// <inheritdoc />
    public partial class AddRuinTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorite_Ruin_RuinId",
                table: "Favorite");

            migrationBuilder.DropForeignKey(
                name: "FK_Ruin_Categories_CategoryId",
                table: "Ruin");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ruin",
                table: "Ruin");

            migrationBuilder.RenameTable(
                name: "Ruin",
                newName: "Ruins");

            migrationBuilder.RenameIndex(
                name: "IX_Ruin_CategoryId",
                table: "Ruins",
                newName: "IX_Ruins_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ruins",
                table: "Ruins",
                column: "RuinId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorite_Ruins_RuinId",
                table: "Favorite",
                column: "RuinId",
                principalTable: "Ruins",
                principalColumn: "RuinId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ruins_Categories_CategoryId",
                table: "Ruins",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorite_Ruins_RuinId",
                table: "Favorite");

            migrationBuilder.DropForeignKey(
                name: "FK_Ruins_Categories_CategoryId",
                table: "Ruins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ruins",
                table: "Ruins");

            migrationBuilder.RenameTable(
                name: "Ruins",
                newName: "Ruin");

            migrationBuilder.RenameIndex(
                name: "IX_Ruins_CategoryId",
                table: "Ruin",
                newName: "IX_Ruin_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ruin",
                table: "Ruin",
                column: "RuinId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorite_Ruin_RuinId",
                table: "Favorite",
                column: "RuinId",
                principalTable: "Ruin",
                principalColumn: "RuinId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ruin_Categories_CategoryId",
                table: "Ruin",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
