using System;
using System.Collections.Generic;
using System.Text;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;
using revitElem = Autodesk.Revit.DB.Element;
using revitElemId = Autodesk.Revit.DB.ElementId;
using revitDoc = Autodesk.Revit.DB.Document;

using Newtonsoft.Json;

namespace Synthetic.Revit.JSON
{
    public class ElementIdJSON
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
        //public string UniqueId { get; set; }


        [JsonConstructor]
        public ElementIdJSON (string Name, int ElementId, string Class, string Category)
        {
            this.Id = ElementId;
            this.Name = Name;
            this.Class = Class;
            this.Category = Category;
            //this.UniqueId = UniqueId;


        }

        public ElementIdJSON (revitElemId Id,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc Document)
        {
            this.Id = Id.IntegerValue;

            revitElem elem = Document.GetElement(Id);

            if (elem != null)
            {
                this.Name = elem.Name;
                this.Class = elem.GetType().Name;

                revitDB.Category cat = elem.Category;

                if(cat != null)
                {
                    this.Category = elem.Category.Name;
                }
                
            }
        }

        public static ElementIdJSON ByJSON (string JSON)
        {
            return JsonConvert.DeserializeObject<ElementIdJSON>(JSON);
        }

        public static string ToJSON (ElementIdJSON IdJSON)
        {
            return JsonConvert.SerializeObject(IdJSON, Formatting.Indented);
        }

        public static revitElemId ToElementId (ElementIdJSON IdJSON)
        {
            return new revitElemId(IdJSON.Id);
        }
    }
}
