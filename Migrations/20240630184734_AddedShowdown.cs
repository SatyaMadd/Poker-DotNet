using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pokerapi.Migrations
{
    /// <inheritdoc />
    public partial class AddedShowdown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Showdown",
                table: "GlobalVs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Showdown",
                table: "GlobalVs");
        }
    }
}
