using System.Text.Json.Serialization;

namespace MeliManager.Core.Models
{
    public class DolarResponse
    {
        [JsonPropertyName("venta")]
        public decimal Venta { get; set; }

        [JsonPropertyName("fechaActualizacion")]
        public DateTime FechaActualizacion { get; set; }
    }
}