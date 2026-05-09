using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Heritage_of_Turkey.Migrations
{
    /// <inheritdoc />
    public partial class AddMuseumTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorite_Museum_MuseumId",
                table: "Favorite");

            migrationBuilder.DropForeignKey(
                name: "FK_Museum_Categories_CategoryId",
                table: "Museum");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Museum",
                table: "Museum");

            migrationBuilder.RenameTable(
                name: "Museum",
                newName: "Museums");

            migrationBuilder.RenameIndex(
                name: "IX_Museum_CategoryId",
                table: "Museums",
                newName: "IX_Museums_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Museums",
                table: "Museums",
                column: "MuseumId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorite_Museums_MuseumId",
                table: "Favorite",
                column: "MuseumId",
                principalTable: "Museums",
                principalColumn: "MuseumId");

            migrationBuilder.AddForeignKey(
                name: "FK_Museums_Categories_CategoryId",
                table: "Museums",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorite_Museums_MuseumId",
                table: "Favorite");

            migrationBuilder.DropForeignKey(
                name: "FK_Museums_Categories_CategoryId",
                table: "Museums");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Museums",
                table: "Museums");

            migrationBuilder.RenameTable(
                name: "Museums",
                newName: "Museum");

            migrationBuilder.RenameIndex(
                name: "IX_Museums_CategoryId",
                table: "Museum",
                newName: "IX_Museum_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Museum",
                table: "Museum",
                column: "MuseumId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorite_Museum_MuseumId",
                table: "Favorite",
                column: "MuseumId",
                principalTable: "Museum",
                principalColumn: "MuseumId");

            migrationBuilder.AddForeignKey(
                name: "FK_Museum_Categories_CategoryId",
                table: "Museum",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
