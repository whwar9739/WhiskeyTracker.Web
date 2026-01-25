using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhiskeyTracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerToModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "TastingSessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "TastingNotes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Bottles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TastingSessions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TastingNotes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Bottles");
        }
    }
}
