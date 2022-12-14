using System.Text.Json.Serialization;

namespace lab2.Models
{
    public class Product : IEntity
    {
        [JsonPropertyName("id")]
        public virtual int Id { get; set; }
        [JsonPropertyName("ingredient")]
        public virtual Ingredient Ingredient { get; set; }
        [JsonPropertyName("price")]
        public virtual float Price { get; set; }
        [JsonPropertyName("delivery_terms")]
        public virtual string DeliveryTerms { get; set; }
        [JsonPropertyName("payment_terms")]
        public virtual string PaymentTerms { get; set; }
        [JsonPropertyName("provider")]
        public virtual Provider Provider { get; set; }
        [JsonPropertyName("name")]
        public virtual string Name { get; set; }
    }
}