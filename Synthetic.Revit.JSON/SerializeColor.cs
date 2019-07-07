using System;
using System.Collections.Generic;
using System.Text;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;
using revitMaterial = Autodesk.Revit.DB.Material;
using revitDoc = Autodesk.Revit.DB.Document;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    [SupressImportIntoVM]
    public class SerializeColor
    {
        public Byte Blue { get; set; }
        public Byte Green { get; set; }
        public Byte Red { get; set; }

        public SerializeColor() { }

        public SerializeColor (Byte Red, Byte Green, Byte Blue)
        {
            this.Red = Red;
            this.Green = Green;
            this.Blue = Blue;
        }

        public SerializeColor (revitDB.Color color)
        {
            this.Red = color.Red;
            this.Green = color.Green;
            this.Blue = color.Blue;
        }

        public static SerializeColor ByJSON (string JSON)
        {
            return JsonConvert.DeserializeObject<SerializeColor>(JSON);
        }

        public static string ToJSON (SerializeColor color)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(color, Formatting.Indented);
        }

        public static revitDB.Color ToColor (SerializeColor colorJSON)
        {
            return new revitDB.Color(colorJSON.Red, colorJSON.Green, colorJSON.Blue);
        }
    }
}
