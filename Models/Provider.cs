using System.Text.Json.Serialization;

namespace lab2.Models
{
    public class Provider : IEntity
    {
        [JsonPropertyName("id")]
        public virtual int Id { get; set; }
        [JsonPropertyName("name")]
        public virtual string Name { get; set; }
        // public virtual string PostAddress { get; set; }
        [JsonPropertyName("post_address")]
        public virtual Address PostAddress { get; set; }
        [JsonPropertyName("phone_number")]
        public virtual string PhoneNumber { get; set; }
        [JsonPropertyName("fax")]
        public virtual string Fax { get; set; }
        [JsonPropertyName("email")]
        public virtual string Email { get; set; }
    }
}