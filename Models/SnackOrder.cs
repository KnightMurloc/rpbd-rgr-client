using System.Text.Json.Serialization;

namespace lab2.Models
{
    public class SnackOrder : IEntity
    {
        [JsonPropertyName("id")]
        public virtual int Id { get; set; }
        [JsonPropertyName("snack")]
        public virtual Snack Snack { get; set; }
        [JsonPropertyName("waiter")]
        public virtual Employees Waiter { get; set; }
        [JsonPropertyName("table")]
        public virtual int Table { get; set; }
    }
}