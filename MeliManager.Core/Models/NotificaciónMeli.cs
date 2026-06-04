using System.Text.Json.Serialization;

namespace MeliManager.Core.Models
{
    public class NotificacionMeli
    {
        // El ID de la cuenta que recibió la venta (para buscarla en SQLite)
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }

        // La ruta del recurso. Ejemplo: "/orders/2000001234"
        [JsonPropertyName("resource")]
        public string Resource { get; set; } = string.Empty;

        // El tipo de aviso. Nosotros filtraremos para que solo lea "orders_v2"
        [JsonPropertyName("topic")]
        public string Topic { get; set; } = string.Empty;

        [JsonPropertyName("application_id")]
        public long ApplicationId { get; set; }
    }
}