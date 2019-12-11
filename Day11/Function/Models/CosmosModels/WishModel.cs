using System;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace Day11.Models.CosmosModels
{
    public class WishModel
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }
        
        [JsonProperty(PropertyName = "who")]
        public string Who { get; set; }
        
        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }
        
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        
        public static explicit operator WishModel(Document doc)
        {
            var model = new WishModel
            {
                Id = doc.GetPropertyValue<Guid>("id"),
                Type = doc.GetPropertyValue<string>("type"),
                Who = doc.GetPropertyValue<string>("who"),
                Address = doc.GetPropertyValue<string>("address"),
                Description = doc.GetPropertyValue<string>("description")
            };

            return model;
        }
        
    }
}