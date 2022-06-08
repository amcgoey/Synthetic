using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;

using RevitDB = Autodesk.Revit.DB;
using RevitDoc = Autodesk.Revit.DB.Document;

using SynthEnum = Synthetic.Core.Enumeration;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    [SupressImportIntoVM]
    public class SerialEnum : SerialObject
    {
        public string Type { get; set; }
        public string Value { get; set; }

        public SerialEnum () { }

        public SerialEnum (Type type, Enum value)
        {
            this.Type = type.FullName;
            this.Value = value.ToString();
        }

        public SerialEnum (string type, string value)
        {
            this.Type = type;
            this.Value = value;
        }

        public Enum ToEnum()
        {
            return (Enum)SynthEnum.Parse(this.Type, this.Value);
        }
    }
}
