using System.Text.Json.Serialization;

namespace lab2.Models
{
    public class SnackRecipes
    {
        [JsonPropertyName("id")]
        public virtual int Id { get; set; }
        [JsonPropertyName("ingredient")]
        public virtual Ingredient Ingredient { get; set; }
        [JsonPropertyName("count")]
        public virtual int Count { get; set; }
    }
}