using System;
using System.ComponentModel.DataAnnotations;

namespace MeliManager.Core.Models
{
    public class Producto
    {
        [Key]
        // Usamos el SKU como llave principal porque es el código universal de tu negocio
        public string Sku { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        // El "Landed Cost" que vamos a calcular con tu simulador de importación
        public decimal CostoUnitario { get; set; }

        // --- Módulo de Atención al Cliente Automática ---

        // El texto que se enviará por mensaje privado de ML
        public string MensajePostVenta { get; set; } = string.Empty;

        // La ruta donde guardarás los renders/manuales en tu servidor (ej: "/manuales/alarma.pdf")
        public string? RutaManualPdf { get; set; }

        public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
    }
}