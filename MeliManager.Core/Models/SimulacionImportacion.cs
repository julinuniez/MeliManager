using System;
using System.Text.Json.Serialization;

namespace MeliManager.Core.Models
{
    public class SimulacionImportacion
    {
        // Las variables de entrada idénticas a tu archivo
        public int Cantidad { get; set; }
        public decimal FobUnitario { get; set; }
        public decimal GastosBancarios { get; set; }
        public decimal Derechos { get; set; }
        public decimal Flete { get; set; }
        public decimal Servicio { get; set; } // Courier / Despachante
        [JsonIgnore]
        public decimal TipoCambio { get; set; }
        public decimal PorcentajeIva { get; set; } = 0.21m;

        // --- Cálculos Automáticos (El motor de tu Excel) ---

        public decimal FobTotal => FobUnitario * Cantidad;
        public decimal IvaAduaneroUsd => (FobTotal + Derechos + Flete) * PorcentajeIva;
        public decimal CashFlowTotalUsd => FobTotal + GastosBancarios + Derechos + Flete + Servicio + IvaAduaneroUsd;
        public decimal CostoTotalUsd => FobTotal + GastosBancarios + Derechos + Flete + Servicio;
        public decimal CostoTotalArs => CostoTotalUsd * TipoCambio;
        public decimal CostoUnitarioArs => Cantidad > 0 ? CostoTotalArs / Cantidad : 0;
    }
}