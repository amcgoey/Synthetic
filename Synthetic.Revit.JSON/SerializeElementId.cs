using System;
using System.Collections.Generic;
using System.Text;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;
using revitElem = Autodesk.Revit.DB.Element;
using revitElemId = Autodesk.Revit.DB.ElementId;
using revitDoc = Autodesk.Revit.DB.Document;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerializeElementId
    {
        public const string ClassName = "ElementId";

        /// <summary>
        /// Value of the Element Id as an int
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the element the Id belongs too.
        /// </summary>
        public string Name { get; set; }

        public string Class { get; set; }
        public string Category { get; set; }
        public string UniqueId { get; set; }

        public SerializeElementId () { }

        public SerializeElementId (string Name, int ElementId, string Class, string Category)
        {
            this.Id = ElementId;
            this.Name = Name;
            this.Class = Class;
            this.Category = Category;
            //this.UniqueId = UniqueId;
        }

        public SerializeElementId (revitElemId Id,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc Document)
        {
            this.Id = Id.IntegerValue;

            revitElem elem = Document.GetElement(Id);

            if (elem != null)
            {
                this.Name = elem.Name;
                this.Class = elem.GetType().Name;
                this.UniqueId = elem.UniqueId;

                revitDB.Category cat = elem.Category;

                if(cat != null)
                {
                    this.Category = elem.Category.Name;
                }
                
            }
        }

        public static SerializeElementId ByJSON (string JSON)
        {
            return JsonConvert.DeserializeObject<SerializeElementId>(JSON);
        }

        public static string ToJSON (SerializeElementId IdJSON)
        {
            return JsonConvert.SerializeObject(IdJSON, Formatting.Indented);
        }

        public static revitElemId ToElementId (SerializeElementId IdJSON)
        {
            return new revitElemId(IdJSON.Id);
        }
    }
}
