using System;
using System.Collections.Generic;
using System.Text;

using Autodesk.DesignScript.Runtime;

using RevitDB = Autodesk.Revit.DB;
using RevitDoc = Autodesk.Revit.DB.Document;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    [SupressImportIntoVM]
    public class SerialColor : SerialObject
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

        public SerialColor (RevitDB.Color color)
        {
            if (color.IsValid)
            {
                this.Red = color.Red;
                this.Green = color.Green;
                this.Blue = color.Blue;
            }
        }

        public static SerialColor ByJSON (string JSON)
        {
            return JsonConvert.DeserializeObject<SerialColor>(JSON);
        }

        public static string ToJSON (SerialColor color)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(color, Formatting.Indented);
        }

        public RevitDB.Color ToColor ()
        {
            return new RevitDB.Color(this.Red, this.Green, this.Blue);
        }
    }
}
