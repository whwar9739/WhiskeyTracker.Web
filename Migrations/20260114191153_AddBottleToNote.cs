using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhiskeyTracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddBottleToNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BottleId",
                table: "TastingNotes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TastingNotes_BottleId",
                table: "TastingNotes",
                column: "BottleId");

            migrationBuilder.AddForeignKey(
                name: "FK_TastingNotes_Bottles_BottleId",
                table: "TastingNotes",
                column: "BottleId",
                principalTable: "Bottles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TastingNotes_Bottles_BottleId",
                table: "TastingNotes");

            migrationBuilder.DropIndex(
                name: "IX_TastingNotes_BottleId",
                table: "TastingNotes");

            migrationBuilder.DropColumn(
                name: "BottleId",
                table: "TastingNotes");
        }
    }
}
