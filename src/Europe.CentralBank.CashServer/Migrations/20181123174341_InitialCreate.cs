using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Europe.CentralBank.CashServer.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cashs",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    amount = table.Column<int>(nullable: false),
                    created_at = table.Column<DateTimeOffset>(nullable: false),
                    digital = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cashs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "invalidations",
                columns: table => new
                {
                    cash_id = table.Column<int>(nullable: false),
                    cashid = table.Column<int>(nullable: true),
                    invalidated_cash_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invalidations", x => new { x.cash_id, x.invalidated_cash_id });
                    table.ForeignKey(
                        name: "FK_invalidations_cashs_cash_id",
                        column: x => x.cash_id,
                        principalTable: "cashs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invalidations_cashs_cashid",
                        column: x => x.cashid,
                        principalTable: "cashs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invalidations_cashs_invalidated_cash_id",
                        column: x => x.invalidated_cash_id,
                        principalTable: "cashs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_invalidations_cashid",
                table: "invalidations",
                column: "cashid");

            migrationBuilder.CreateIndex(
                name: "IX_invalidations_invalidated_cash_id",
                table: "invalidations",
                column: "invalidated_cash_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invalidations");

            migrationBuilder.DropTable(
                name: "cashs");
        }
    }
}
