using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pokerapi.Migrations
{
    /// <inheritdoc />
    public partial class VirtualReferenceUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BetTracks_GlobalVs_GameId",
                table: "BetTracks");

            migrationBuilder.DropForeignKey(
                name: "FK_CommCards_GlobalVs_GameId",
                table: "CommCards");

            migrationBuilder.DropForeignKey(
                name: "FK_DeckCards_GlobalVs_GameId",
                table: "DeckCards");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_GlobalVs_GameId",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Players_GameId",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_DeckCards_GameId",
                table: "DeckCards");

            migrationBuilder.DropIndex(
                name: "IX_CommCards_GameId",
                table: "CommCards");

            migrationBuilder.DropIndex(
                name: "IX_BetTracks_GameId",
                table: "BetTracks");

            migrationBuilder.AddColumn<int>(
                name: "GlobalVId",
                table: "Players",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "GlobalVId",
                table: "DeckCards",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GlobalVId",
                table: "CommCards",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_GlobalVId",
                table: "Players",
                column: "GlobalVId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckCards_GlobalVId",
                table: "DeckCards",
                column: "GlobalVId");

            migrationBuilder.CreateIndex(
                name: "IX_CommCards_GlobalVId",
                table: "CommCards",
                column: "GlobalVId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommCards_GlobalVs_GlobalVId",
                table: "CommCards",
                column: "GlobalVId",
                principalTable: "GlobalVs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeckCards_GlobalVs_GlobalVId",
                table: "DeckCards",
                column: "GlobalVId",
                principalTable: "GlobalVs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_GlobalVs_GlobalVId",
                table: "Players",
                column: "GlobalVId",
                principalTable: "GlobalVs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommCards_GlobalVs_GlobalVId",
                table: "CommCards");

            migrationBuilder.DropForeignKey(
                name: "FK_DeckCards_GlobalVs_GlobalVId",
                table: "DeckCards");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_GlobalVs_GlobalVId",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Players_GlobalVId",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_DeckCards_GlobalVId",
                table: "DeckCards");

            migrationBuilder.DropIndex(
                name: "IX_CommCards_GlobalVId",
                table: "CommCards");

            migrationBuilder.DropColumn(
                name: "GlobalVId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "GlobalVId",
                table: "DeckCards");

            migrationBuilder.DropColumn(
                name: "GlobalVId",
                table: "CommCards");

            migrationBuilder.CreateIndex(
                name: "IX_Players_GameId",
                table: "Players",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckCards_GameId",
                table: "DeckCards",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_CommCards_GameId",
                table: "CommCards",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_BetTracks_GameId",
                table: "BetTracks",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_BetTracks_GlobalVs_GameId",
                table: "BetTracks",
                column: "GameId",
                principalTable: "GlobalVs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommCards_GlobalVs_GameId",
                table: "CommCards",
                column: "GameId",
                principalTable: "GlobalVs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeckCards_GlobalVs_GameId",
                table: "DeckCards",
                column: "GameId",
                principalTable: "GlobalVs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Players_GlobalVs_GameId",
                table: "Players",
                column: "GameId",
                principalTable: "GlobalVs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
