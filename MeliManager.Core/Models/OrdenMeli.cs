using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MeliManager.Core.Models
{
    // 1. El molde principal del ticket de compra
    public class OrdenMeli
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("total_amount")]
        public decimal TotalAmount { get; set; }
        [JsonPropertyName("buyer")]
        public CompradorMeli Buyer { get; set; } = new CompradorMeli();
        [JsonPropertyName("order_items")]
        public List<OrderItemMeli> OrderItems { get; set; } = new List<OrderItemMeli>();
    }

    public class OrderItemMeli
    {
        [JsonPropertyName("item")]
        public ItemDetailsMeli Item { get; set; } = new ItemDetailsMeli();

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unit_price")]
        public decimal UnitPrice { get; set; }
    }

    public class ItemDetailsMeli
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("seller_sku")]
        public string? SellerSku { get; set; }
    }
    public class CompradorMeli
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }
}