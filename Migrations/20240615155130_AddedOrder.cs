using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pokerapi.Migrations
{
    /// <inheritdoc />
    public partial class AddedOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Turn",
                table: "Players",
                newName: "TurnOrder");

            migrationBuilder.AddColumn<bool>(
                name: "IsTurn",
                table: "Players",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTurn",
                table: "Players");

            migrationBuilder.RenameColumn(
                name: "TurnOrder",
                table: "Players",
                newName: "Turn");
        }
    }
}
