using System.Collections.Generic;

namespace lab2.Models
{
    public class Query
    {
        public string Table { get; set; }
        public int StartId { get; set; }
        public int EndId { get; set; }
        
        public int Count { get; set; }
        public string Condition { get; set; }
        
        public Query()
        {
            StartId = -1;
            EndId = -1;
            Condition = "";
            Count = -1;
        }
    }
}