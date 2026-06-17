using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeliManager.Migrations
{
    /// <inheritdoc />
    public partial class AjusteFinalModelo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GaleriaJson",
                table: "Productos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GaleriaJson",
                table: "Productos");
        }
    }
}
