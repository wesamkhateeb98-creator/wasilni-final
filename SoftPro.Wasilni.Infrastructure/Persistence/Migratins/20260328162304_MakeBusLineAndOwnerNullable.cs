using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoftPro.Wasilni.Infrastructure.Persistence.Migratins
{
    /// <inheritdoc />
    public partial class MakeBusLineAndOwnerNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Buses_Accounts_OwnId",
                table: "Buses");

            migrationBuilder.AlterColumn<int>(
                name: "OwnId",
                table: "Buses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "LineId",
                table: "Buses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Buses_Accounts_OwnId",
                table: "Buses",
                column: "OwnId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Buses_Accounts_OwnId",
                table: "Buses");

            migrationBuilder.AlterColumn<int>(
                name: "OwnId",
                table: "Buses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LineId",
                table: "Buses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Buses_Accounts_OwnId",
                table: "Buses",
                column: "OwnId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
