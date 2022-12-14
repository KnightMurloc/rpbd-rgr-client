using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace lab2.Models
{
    public class Snack : IEntity
    {
        [JsonPropertyName("id")]
        public virtual int Id { get; set; }
        [JsonPropertyName("name")]
        public virtual string Name { get; set; }
        [JsonPropertyName("size")]
        public virtual int Size { get; set; }

        
        private List<SnackRecipes> _ingredients;

        
        [JsonPropertyName("ingredients")]
        public virtual List<SnackRecipes> Ingredients
        {
            get
            {
                return _ingredients ??= new List<SnackRecipes>();
            }
            set => _ingredients = value;
        }
    }
}