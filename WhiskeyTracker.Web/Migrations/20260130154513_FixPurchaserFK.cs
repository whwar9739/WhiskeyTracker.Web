using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhiskeyTracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixPurchaserFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bottles_AspNetUsers_PurchaserId",
                table: "Bottles");

            migrationBuilder.DropIndex(
                name: "IX_Bottles_PurchaserId",
                table: "Bottles");

            migrationBuilder.DropColumn(
                name: "PurchaserId",
                table: "Bottles");

            migrationBuilder.CreateIndex(
                name: "IX_Bottles_UserId",
                table: "Bottles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bottles_AspNetUsers_UserId",
                table: "Bottles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bottles_AspNetUsers_UserId",
                table: "Bottles");

            migrationBuilder.DropIndex(
                name: "IX_Bottles_UserId",
                table: "Bottles");

            migrationBuilder.AddColumn<string>(
                name: "PurchaserId",
                table: "Bottles",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bottles_PurchaserId",
                table: "Bottles",
                column: "PurchaserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bottles_AspNetUsers_PurchaserId",
                table: "Bottles",
                column: "PurchaserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
