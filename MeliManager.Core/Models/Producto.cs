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

        // --- Datos del Catálogo (Vidriera) ---
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
        public string Estado { get; set; } = "pausado"; // "publicado" o "pausado"
        public string ImagenUrl { get; set; } = string.Empty; // URL de tu foto de estudio generada por IA
        public string GaleriaJson { get; set; } = "[]";

        // --- Datos Logísticos y Aduaneros (Simulador) ---
        public decimal Fob { get; set; }
        public decimal PesoKg { get; set; }
        public decimal Largo { get; set; }
        public decimal Ancho { get; set; }
        public decimal Alto { get; set; }
        public decimal DerechosPorcentaje { get; set; } // Arancel aduanero específico del producto

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