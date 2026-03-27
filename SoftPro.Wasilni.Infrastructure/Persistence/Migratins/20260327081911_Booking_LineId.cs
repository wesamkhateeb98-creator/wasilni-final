using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftPro.Wasilni.Infrastructure.Persistence.Migratins
{
    /// <inheritdoc />
    public partial class Booking_LineId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Buses_BusId",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "BusId",
                table: "Bookings",
                newName: "LineId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_BusId",
                table: "Bookings",
                newName: "IX_Bookings_LineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Lines_LineId",
                table: "Bookings",
                column: "LineId",
                principalTable: "Lines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Lines_LineId",
                table: "Bookings");

            migrationBuilder.RenameColumn(
                name: "LineId",
                table: "Bookings",
                newName: "BusId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_LineId",
                table: "Bookings",
                newName: "IX_Bookings_BusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Buses_BusId",
                table: "Bookings",
                column: "BusId",
                principalTable: "Buses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
