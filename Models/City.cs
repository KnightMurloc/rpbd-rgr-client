using System.Text.Json.Serialization;

namespace lab2.Models
{
    public class City : IEntity
    {
        [JsonPropertyName("id")]
        public virtual int Id { get; set; }
        [JsonPropertyName("name")]
        public virtual string Name { get; set; }
    }
}