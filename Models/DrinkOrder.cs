using System.Text.Json.Serialization;

namespace lab2.Models
{
    public class DrinkOrder : IEntity
    {
        [JsonPropertyName("id")]
        public virtual int Id { get; set; }
        [JsonPropertyName("drink")]
        public virtual Drink Drink { get; set; }
        [JsonPropertyName("waiter")]
        public virtual Employees Waiter { get; set; }
        [JsonPropertyName("table")]
        public virtual int Table { get; set; }
    }
}