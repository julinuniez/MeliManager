namespace MeliManager.Core.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Sku { get; set; } = string.Empty; // Ej: ALARMA-BICI-WIFI
        public string Nombre { get; set; } = string.Empty;
        public decimal CostoUnitario { get; set; } // Lo que te costó traerla
    }
}