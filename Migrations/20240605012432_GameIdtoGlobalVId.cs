using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pokerapi.Migrations
{
    /// <inheritdoc />
    public partial class GameIdtoGlobalVId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommCards_GlobalVs_GlobalVId",
                table: "CommCards");

            migrationBuilder.DropForeignKey(
                name: "FK_DeckCards_GlobalVs_GlobalVId",
                table: "DeckCards");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "DeckCards");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "CommCards");

            migrationBuilder.RenameColumn(
                name: "GameId",
                table: "BetTracks",
                newName: "GlobalVId");

            migrationBuilder.AlterColumn<int>(
                name: "Suit",
                table: "PlayerCards",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "GlobalVId",
                table: "DeckCards",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GlobalVId",
                table: "CommCards",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CommCards_GlobalVs_GlobalVId",
                table: "CommCards",
                column: "GlobalVId",
                principalTable: "GlobalVs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeckCards_GlobalVs_GlobalVId",
                table: "DeckCards",
                column: "GlobalVId",
                principalTable: "GlobalVs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.RenameColumn(
                name: "GlobalVId",
                table: "BetTracks",
                newName: "GameId");

            migrationBuilder.AlterColumn<string>(
                name: "Suit",
                table: "PlayerCards",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "GlobalVId",
                table: "DeckCards",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "GameId",
                table: "DeckCards",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "GlobalVId",
                table: "CommCards",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "GameId",
                table: "CommCards",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

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
        }
    }
}
