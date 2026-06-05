using System.Text.Json.Serialization;

namespace MeliManager.Core.Models
{
    public class MensajeMeliRequest
    {
        [JsonPropertyName("from")]
        public UsuarioMensaje From { get; set; } = new UsuarioMensaje();

        [JsonPropertyName("to")]
        public UsuarioMensaje To { get; set; } = new UsuarioMensaje();

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        // Opcional: Acá irán los IDs de tus manuales en PDF cuando los subamos
        [JsonPropertyName("attachments")]
        public List<string>? Attachments { get; set; }
    }

    public class UsuarioMensaje
    {
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }
    }
}