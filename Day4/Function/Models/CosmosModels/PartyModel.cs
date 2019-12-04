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
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public IList<Food> FoodList { get; set; }
    }

    public class Food
    {
        public  string Name { get; set; }
        public string Owner { get; set; }
    }
}