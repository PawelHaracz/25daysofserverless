using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Day4.Models.CosmosModels
{
    public class PartyModel
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }
        
        [JsonProperty(PropertyName = "organizer")]
        public string Organizer { get; set; }
        
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        
        [JsonProperty(PropertyName = "location")]

        public string Location { get; set; }
        
        [JsonProperty(PropertyName = "foodList")]
        public IList<Food> FoodList { get; set; }
    }

    public class Food
    {
        [JsonProperty(PropertyName = "name")]
        public  string Name { get; set; }
        
        [JsonProperty(PropertyName = "owner")]
        public string Owner { get; set; }
    }
}