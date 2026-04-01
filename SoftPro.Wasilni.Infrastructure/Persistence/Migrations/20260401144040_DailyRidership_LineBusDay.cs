using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftPro.Wasilni.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DailyRidership_LineBusDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyRiderships_Buses_BusId",
                table: "DailyRiderships");

            migrationBuilder.DropIndex(
                name: "IX_DailyRiderships_BusId_Day",
                table: "DailyRiderships");

            migrationBuilder.DropIndex(
                name: "IX_DailyRiderships_Day",
                table: "DailyRiderships");

            migrationBuilder.DropIndex(
                name: "IX_DailyRiderships_LineId_Day",
                table: "DailyRiderships");

            migrationBuilder.CreateIndex(
                name: "IX_DailyRiderships_BusId",
                table: "DailyRiderships",
                column: "BusId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyRiderships_LineId_BusId_Day",
                table: "DailyRiderships",
                columns: new[] { "LineId", "BusId", "Day" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DailyRiderships_Buses_BusId",
                table: "DailyRiderships",
                column: "BusId",
                principalTable: "Buses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyRiderships_Lines_LineId",
                table: "DailyRiderships",
                column: "LineId",
                principalTable: "Lines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyRiderships_Buses_BusId",
                table: "DailyRiderships");

            migrationBuilder.DropForeignKey(
                name: "FK_DailyRiderships_Lines_LineId",
                table: "DailyRiderships");

            migrationBuilder.DropIndex(
                name: "IX_DailyRiderships_BusId",
                table: "DailyRiderships");

            migrationBuilder.DropIndex(
                name: "IX_DailyRiderships_LineId_BusId_Day",
                table: "DailyRiderships");

            migrationBuilder.CreateIndex(
                name: "IX_DailyRiderships_BusId_Day",
                table: "DailyRiderships",
                columns: new[] { "BusId", "Day" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyRiderships_Day",
                table: "DailyRiderships",
                column: "Day");

            migrationBuilder.CreateIndex(
                name: "IX_DailyRiderships_LineId_Day",
                table: "DailyRiderships",
                columns: new[] { "LineId", "Day" });

            migrationBuilder.AddForeignKey(
                name: "FK_DailyRiderships_Buses_BusId",
                table: "DailyRiderships",
                column: "BusId",
                principalTable: "Buses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
