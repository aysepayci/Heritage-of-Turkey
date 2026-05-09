using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Heritage_of_Turkey.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoriteTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorite_AspNetUsers_UserId",
                table: "Favorite");

            migrationBuilder.DropForeignKey(
                name: "FK_Favorite_Museums_MuseumId",
                table: "Favorite");

            migrationBuilder.DropForeignKey(
                name: "FK_Favorite_Ruins_RuinId",
                table: "Favorite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Favorite",
                table: "Favorite");

            migrationBuilder.RenameTable(
                name: "Favorite",
                newName: "Favorites");

            migrationBuilder.RenameIndex(
                name: "IX_Favorite_UserId",
                table: "Favorites",
                newName: "IX_Favorites_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Favorite_RuinId",
                table: "Favorites",
                newName: "IX_Favorites_RuinId");

            migrationBuilder.RenameIndex(
                name: "IX_Favorite_MuseumId",
                table: "Favorites",
                newName: "IX_Favorites_MuseumId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Favorites",
                table: "Favorites",
                column: "FavoriteId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Favorite_MuseumOrRuin",
                table: "Favorites",
                sql: "([MuseumId] IS NOT NULL AND [RuinId] IS NULL) OR ([MuseumId] IS NULL AND [RuinId] IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_AspNetUsers_UserId",
                table: "Favorites",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Museums_MuseumId",
                table: "Favorites",
                column: "MuseumId",
                principalTable: "Museums",
                principalColumn: "MuseumId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Ruins_RuinId",
                table: "Favorites",
                column: "RuinId",
                principalTable: "Ruins",
                principalColumn: "RuinId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_AspNetUsers_UserId",
                table: "Favorites");

            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Museums_MuseumId",
                table: "Favorites");

            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Ruins_RuinId",
                table: "Favorites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Favorites",
                table: "Favorites");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Favorite_MuseumOrRuin",
                table: "Favorites");

            migrationBuilder.RenameTable(
                name: "Favorites",
                newName: "Favorite");

            migrationBuilder.RenameIndex(
                name: "IX_Favorites_UserId",
                table: "Favorite",
                newName: "IX_Favorite_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Favorites_RuinId",
                table: "Favorite",
                newName: "IX_Favorite_RuinId");

            migrationBuilder.RenameIndex(
                name: "IX_Favorites_MuseumId",
                table: "Favorite",
                newName: "IX_Favorite_MuseumId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Favorite",
                table: "Favorite",
                column: "FavoriteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorite_AspNetUsers_UserId",
                table: "Favorite",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Favorite_Museums_MuseumId",
                table: "Favorite",
                column: "MuseumId",
                principalTable: "Museums",
                principalColumn: "MuseumId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorite_Ruins_RuinId",
                table: "Favorite",
                column: "RuinId",
                principalTable: "Ruins",
                principalColumn: "RuinId");
        }
    }
}
