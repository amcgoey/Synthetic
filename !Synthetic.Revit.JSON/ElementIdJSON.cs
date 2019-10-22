using System;
using System.Collections.Generic;
using System.Text;

using Autodesk.DesignScript.Runtime;

using Autodesk.Revit.DB;
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
        public string ElementName { get; set; }

        public string UniqueId { get; set; }
        public string ElementClass { get; set; }
        public string ElementCategory { get; set; }

        [JsonConstructor]
        public ElementIdJSON (int ElementId, string ElementName, string UniqueId, string ElementClass, string ElementCategory)
        {
            this.Id = ElementId;
            this.ElementName = ElementName;
            this.UniqueId = UniqueId;
            this.ElementClass = ElementClass;
            this.ElementCategory = ElementCategory;
        }

        public ElementIdJSON (revitElemId Id,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc Document)
        {
            this.Id = Id.IntegerValue;

            revitElem elem = Document.GetElement(Id);

            this.ElementName = elem.Name;
            this.ElementClass = elem.GetType().Name;
        }

        public static ElementIdJSON ByJSON (string JSON)
        {
            return JsonConvert.DeserializeObject<ElementIdJSON>(JSON);
        }

        public static string ToJSON (ElementIdJSON Id)
        {
            return JsonConvert.SerializeObject(Id, Formatting.Indented);
        }

        
    }
}
