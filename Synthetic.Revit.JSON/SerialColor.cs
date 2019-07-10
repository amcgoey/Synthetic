using System;
using System.Collections.Generic;
using System.Text;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    [SupressImportIntoVM]
    public class SerialColor
    {
        public Byte Blue { get; set; }
        public Byte Green { get; set; }
        public Byte Red { get; set; }

        public SerialColor() { }

        public SerialColor (Byte Red, Byte Green, Byte Blue)
        {
            this.Red = Red;
            this.Green = Green;
            this.Blue = Blue;
        }

        public SerialColor (revitDB.Color color)
        {
            this.Red = color.Red;
            this.Green = color.Green;
            this.Blue = color.Blue;
        }

        public static SerialColor ByJSON (string JSON)
        {
            return JsonConvert.DeserializeObject<SerialColor>(JSON);
        }

        public static string ToJSON (SerialColor color)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(color, Formatting.Indented);
        }

        public revitDB.Color ToColor ()
        {
            return new revitDB.Color(this.Red, this.Green, this.Blue);
        }
    }
}
