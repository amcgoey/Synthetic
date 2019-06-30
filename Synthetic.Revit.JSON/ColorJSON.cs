using System;
using System.Collections.Generic;
using System.Text;

using revitDB = Autodesk.Revit.DB;
using revitMaterial = Autodesk.Revit.DB.Material;
using revitDoc = Autodesk.Revit.DB.Document;

using Newtonsoft.Json;

namespace Synthetic.Revit.JSON
{
    public class ColorJSON
    {
        public Byte Blue { get; set; }
        public Byte Green { get; set; }
        public Byte Red { get; set; }

        [JsonConstructor]
        public ColorJSON (Byte Red, Byte Green, Byte Blue)
        {
            this.Red = Red;
            this.Green = Green;
            this.Blue = Blue;
        }

        public ColorJSON (revitDB.Color color)
        {
            this.Red = color.Red;
            this.Green = color.Green;
            this.Blue = color.Blue;
        }

        public static ColorJSON ByJSON (string JSON)
        {
            return JsonConvert.DeserializeObject<ColorJSON>(JSON);
        }

        public static string ToJSON (ColorJSON color)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(color, Formatting.Indented);
        }

        public static revitDB.Color ToColor (ColorJSON colorJSON)
        {
            return new revitDB.Color(colorJSON.Red, colorJSON.Green, colorJSON.Blue);
        }
    }
}
