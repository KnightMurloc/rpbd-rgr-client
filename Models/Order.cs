using System;
using System.Text.Json.Serialization;

namespace lab2.Models
{
    public class Order : IEntity
    {
        [JsonPropertyName("id")]
        public virtual int Id { get; set; }
        [JsonPropertyName("reason")]
        public virtual string Reason { get; set; }
        [JsonPropertyName("order_number")]
        public virtual int OrderNumber { get; set; }
        [JsonPropertyName("order_date")]
        public virtual DateTime OrderDate { get; set; }
        [JsonPropertyName("Employer")]
        public virtual Employees Employees { get; set; }
        [JsonPropertyName("Post")]
        public virtual Post Post { get; set; }
    }
}