using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeliManager.Migrations
{
    /// <inheritdoc />
    public partial class AgregaHistorialSimulacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistorialSimulaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SkuProducto = table.Column<string>(type: "TEXT", nullable: false),
                    FechaSimulacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CotizacionDolarUtilizada = table.Column<decimal>(type: "TEXT", nullable: false),
                    ValorFobUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    FleteUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    CashFlowRequeridoUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    IvaCreditoFiscalUsd = table.Column<decimal>(type: "TEXT", nullable: false),
                    CostoTotalEnDestinoArs = table.Column<decimal>(type: "TEXT", nullable: false),
                    CostoUnitarioFinalArs = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialSimulaciones", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistorialSimulaciones");
        }
    }
}
