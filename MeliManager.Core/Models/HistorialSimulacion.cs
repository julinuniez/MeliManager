using System;
using System.ComponentModel.DataAnnotations;

namespace MeliManager.Core.Models
{
    public class HistorialSimulacion
    {
        [Key]
        public int Id { get; set; }

        // Relación con el producto
        public string SkuProducto { get; set; } = string.Empty;

        public DateTime FechaSimulacion { get; set; }
        public decimal CotizacionDolarUtilizada { get; set; }

        // Datos de entrada que cargaste
        public decimal ValorFobUsd { get; set; }
        public decimal FleteUsd { get; set; }

        // El desglose financiero guardado
        public decimal CashFlowRequeridoUsd { get; set; }
        public decimal IvaCreditoFiscalUsd { get; set; }
        public decimal CostoTotalEnDestinoArs { get; set; }
        public decimal CostoUnitarioFinalArs { get; set; }
    }
}