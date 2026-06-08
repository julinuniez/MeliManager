using System.Collections.Generic;

namespace MeliManager.Core.DTOs
{
    // DTO para la respuesta de búsqueda de Mercado Libre
    public class MlPagingResult
    {
        public List<string> Results { get; set; } = new(); // Lista de IDs (Ej: MLA123456)
    }

    // DTO para mapear el empaquetado múltiple de la API de ML
    public class MlItemResponse
    {
        public int Code { get; set; }
        public MlItem Body { get; set; } = new();
    }

    // DTO con los datos puros que nos importan de la publicación
    public class MlItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Available_quantity { get; set; }
        public string Status { get; set; } = string.Empty; // "active", "paused"
        public string Secure_thumbnail { get; set; } = string.Empty; // URL de la foto principal
    }
    public class MlPublicSearchResult
    {
        public List<MlPublicItem> Results { get; set; } = new();
    }

    public class MlPublicItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Available_quantity { get; set; }
        public string Thumbnail { get; set; } = string.Empty;
    }
}