using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftPro.Wasilni.Infrastructure.Persistence.Migratins
{
    /// <inheritdoc />
    public partial class AddLinePoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old separate Points table (replaced by JSON column on Lines)
            migrationBuilder.DropTable(
                name: "Points");

            // Drop Permission column (removed from Account model)
            migrationBuilder.DropColumn(
                name: "Permission",
                table: "Accounts");

            // Add Points JSON column to Lines table
            migrationBuilder.AddColumn<string>(
                name: "Points",
                table: "Lines",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Points",
                table: "Lines");

            migrationBuilder.AddColumn<int>(
                name: "Permission",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Points",
                columns: table => new
                {
                    Id        = table.Column<int>(type: "int", nullable: false)
                                     .Annotation("SqlServer:Identity", "1, 1"),
                    Latitude  = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    LineId    = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Points", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Points_Lines_LineId",
                        column: x => x.LineId,
                        principalTable: "Lines",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Points_LineId",
                table: "Points",
                column: "LineId");
        }
    }
}
