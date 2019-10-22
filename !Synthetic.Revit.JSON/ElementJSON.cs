using System;
using System.Collections.Generic;
using System.Text;

using Autodesk.Revit.DB;
using revitElem = Autodesk.Revit.DB.Element;
using revitDoc = Autodesk.Revit.DB.Document;

using Newtonsoft.Json;

namespace Synthetic.Revit.JSON
{
    public class ElementJSON
    {
        public const string ClassName = "Element";

        public string Class { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public string UniqueId { get; set; }
        public string Category { get; set; }
        public List<ParameterJSON> Parameters { get; set; }

        [JsonConstructor]
        public ElementJSON(string className, string name, int Id, string UniqueId, string category, List<ParameterJSON> parameters)
        {
            this.Class = className;
            this.Name = name;
            this.Id = Id;
            this.UniqueId = UniqueId;
            this.Category = category;
            this.Parameters = parameters;
        }

        public ElementJSON(revitElem element)
        {
            this.Class = element.GetType().Name;
            this.Name = element.Name;
            this.Id = element.Id.IntegerValue;
            this.UniqueId = element.UniqueId.ToString();
            this.Category = element.Category.Name;
            this.Parameters = new List<ParameterJSON>();

            //Iterate through parameters
            foreach (Parameter param in element.Parameters)
            {
                //If the parameter has a value, the add it to the parameter list for export
                if (!param.IsReadOnly)
                {
                    this.Parameters.Add(new ParameterJSON(param, element.Document));
                }
            }
        }

        public static ElementJSON ByJSON(string JSON)
        {
            return JsonConvert.DeserializeObject<ElementJSON>(JSON);
        }

        public static string ToJSON(ElementJSON material)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(material, Formatting.Indented);
        }

        public static revitElem ModifyElement(ElementJSON JSON, revitDoc doc)
        {
            revitElem elem = (revitElem)doc.GetElement(JSON.UniqueId);

            elem.Name = JSON.Name;

            foreach (ParameterJSON paramJson in JSON.Parameters)
            {
                ParameterJSON.ModifyParameter(paramJson, elem);
            }

            return elem;
        }
    }
}
