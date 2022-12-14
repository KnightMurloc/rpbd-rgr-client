using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace lab2.Models
{
    public class Drink : IEntity
    {
        [JsonPropertyName("id")]
        public virtual int Id { get; set; }
        [JsonPropertyName("name")]
        public virtual string Name { get; set; }
        [JsonPropertyName("strength")]
        public virtual int Strength { get; set; }
        [JsonPropertyName("size")]
        public virtual int Size { get; set; }
        [JsonPropertyName("container")]
        public virtual string Container { get; set; }
        private List<DrinkRecipes> _ingredients;

        [JsonPropertyName("ingredients")]
        public virtual List<DrinkRecipes> Ingredients
        {
            get
            {
                return _ingredients ??= new List<DrinkRecipes>();
            }
            set => _ingredients = value;
        }
    }
}