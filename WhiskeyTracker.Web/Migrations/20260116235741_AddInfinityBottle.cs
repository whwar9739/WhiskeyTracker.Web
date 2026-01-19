using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WhiskeyTracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddInfinityBottle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "TastingNotes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<int>(
                name: "CapacityMl",
                table: "Bottles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentVolumeMl",
                table: "Bottles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsInfinityBottle",
                table: "Bottles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BlendComponents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InfinityBottleId = table.Column<int>(type: "integer", nullable: false),
                    SourceBottleId = table.Column<int>(type: "integer", nullable: false),
                    AmountAddedMl = table.Column<int>(type: "integer", nullable: false),
                    DateAdded = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlendComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlendComponents_Bottles_InfinityBottleId",
                        column: x => x.InfinityBottleId,
                        principalTable: "Bottles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlendComponents_Bottles_SourceBottleId",
                        column: x => x.SourceBottleId,
                        principalTable: "Bottles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlendComponents_InfinityBottleId",
                table: "BlendComponents",
                column: "InfinityBottleId");

            migrationBuilder.CreateIndex(
                name: "IX_BlendComponents_SourceBottleId",
                table: "BlendComponents",
                column: "SourceBottleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlendComponents");

            migrationBuilder.DropColumn(
                name: "CapacityMl",
                table: "Bottles");

            migrationBuilder.DropColumn(
                name: "CurrentVolumeMl",
                table: "Bottles");

            migrationBuilder.DropColumn(
                name: "IsInfinityBottle",
                table: "Bottles");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "TastingNotes",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }
    }
}
