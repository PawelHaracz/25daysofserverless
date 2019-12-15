using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Day15.Models
{
    public class PictureDescriptionModel
    {
        public PictureDescriptionModel(IEnumerable<string> description, IEnumerable<string> categories,
            IDictionary<string, string> colors, IEnumerable<string> objects)
        {
            Description = description;
            Categories = categories;
            Colors = colors;
            Objects = objects;
        }

        public IEnumerable<string> Description { get; }
        public IEnumerable<string> Categories { get; }
        public IDictionary<string, string> Colors { get; }
        public IEnumerable<string> Objects { get; }
    }
}