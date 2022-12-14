using System.Text.Json.Serialization;

namespace lab2.Models
{
    public class BankDetail : IEntity
    {
        [JsonPropertyName("id")]
        public virtual int Id { get; set; }
        [JsonPropertyName("bank_name")]
        public virtual string Name { get; set; }
        [JsonPropertyName("city")]
        public virtual City City { get; set; }
        [JsonPropertyName("TIN")]
        public virtual string TIN { get; set; }
        [JsonPropertyName("settlement_account")]
        public virtual string SettlementAccount { get; set; }
        [JsonPropertyName("provider")]
        public virtual Provider Provider { get; set; }
    }
}