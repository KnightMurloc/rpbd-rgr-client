using System;
using System.Text.Json.Serialization;

namespace lab2.Models
{
    public class Employees : IEntity
    {
        [JsonPropertyName("id")]
        public virtual int Id { get; set; }
        [JsonPropertyName("first_name")]
        public virtual string FirstName { get; set; }
        [JsonPropertyName("last_name")]

        public virtual string LastName { get; set; }

        [JsonPropertyName("patronymic")]

        public virtual string Patronymic { get; set; }
        
        [JsonPropertyName("address")]
        public virtual Address Address { get; set; }
        [JsonPropertyName("birth_date")]
        public virtual DateTime BirthDate { get; set; }
        [JsonPropertyName("salary")]
        public virtual float Salary { get; set; }
        [JsonPropertyName("post")]
        public virtual Post Post { get; set; }
    }
}