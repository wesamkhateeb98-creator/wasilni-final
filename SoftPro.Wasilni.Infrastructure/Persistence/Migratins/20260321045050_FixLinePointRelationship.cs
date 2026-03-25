using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftPro.Wasilni.Infrastructure.Persistence.Migratins
{
    /// <inheritdoc />
    public partial class FixLinePointRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Points_Lines_LineId",
                table: "Points");

            migrationBuilder.RenameColumn(
                name: "PointIds",
                table: "Lines",
                newName: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Points_Lines_LineId",
                table: "Points",
                column: "LineId",
                principalTable: "Lines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Points_Lines_LineId",
                table: "Points");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Lines",
                newName: "PointIds");

            migrationBuilder.AddForeignKey(
                name: "FK_Points_Lines_LineId",
                table: "Points",
                column: "LineId",
                principalTable: "Lines",
                principalColumn: "Id");
        }
    }
}
