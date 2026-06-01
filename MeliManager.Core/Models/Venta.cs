namespace MeliManager.Core.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string SkuProducto { get; set; } = string.Empty;
        public decimal IngresoBruto { get; set; } // Lo que pagó el comprador
        public decimal ComisionML { get; set; }
        public decimal CostoEnvio { get; set; }
        public decimal CostoUnitario { get; set; } // Copiamos el costo del producto al momento de la venta

        public decimal GananciaNeta => IngresoBruto - ComisionML - CostoEnvio - CostoUnitario;
    }
}