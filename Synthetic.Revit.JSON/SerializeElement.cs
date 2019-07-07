using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;
using revitElem = Autodesk.Revit.DB.Element;
using revitElemId = Autodesk.Revit.DB.ElementId;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerializeElement
    {
        public const string ClassName = "Element";

        public string Class { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public string UniqueId { get; set; }
        public string Category { get; set; }
        public List<SerializeParameter> Parameters { get; set; }

        [JsonIgnoreAttribute]
        public revitElem Element { get; set; }

        [JsonIgnoreAttribute]
        public revitDoc Document { get; set; }

        public SerializeElement() { }

        public SerializeElement(revitElem element)
        {
            this.Element = element;
            this.Document = element.Document;

            this.Class = element.GetType().FullName;
            this.Name = element.Name;
            this.Id = element.Id.IntegerValue;
            this.UniqueId = element.UniqueId.ToString();

            revitDB.Category cat = element.Category;

            if (cat != null)
            {
                this.Category = element.Category.Name;
            }
            
            this.Parameters = new List<SerializeParameter>();

            //Iterate through parameters
            foreach (revitDB.Parameter param in element.Parameters)
            {
                //If the parameter has a value, the add it to the parameter list for export
                if (!param.IsReadOnly)
                {
                    this.Parameters.Add(new SerializeParameter(param, this.Document));
                }
            }
        }

        public static SerializeElement ByJSON(string JSON)
        {
            return JsonConvert.DeserializeObject<SerializeElement>(JSON);
        }

        public static string ToJSON(SerializeElement serialElement)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(serialElement, Formatting.Indented);
        }

        public static revitElem ModifyElement(SerializeElement serialElement, revitDoc document)
        {
            revitElem elem = null;

            if (serialElement.UniqueId != null)
            {
                elem = (revitElem)document.GetElement(serialElement.UniqueId);
            }
            else if (serialElement.Id != 0)
            {
                elem = (revitElem)document.GetElement(new revitElemId(serialElement.Id));
            }
            else if (serialElement.Name != null) 
            {
                //Type elemClass = Type.GetType(JSON.Class);
                Assembly assembly = typeof(revitElem).Assembly;
                Type elemClass = assembly.GetType(serialElement.Class);

                revitDB.FilteredElementCollector collector = new revitDB.FilteredElementCollector(document);
                elem = collector.OfClass(elemClass)
                    .FirstOrDefault(e => e.Name.Equals(serialElement.Name));

                //revitDB.ElementId elemId = revitElem.Create(doc, materialJSON.Name);
                //elem = (revitElem)doc.GetElement(elemId);
            }

            if(elem != null)
            {
                elem.Name = serialElement.Name;

                foreach (SerializeParameter paramJson in serialElement.Parameters)
                {
                    SerializeParameter.ModifyParameter(paramJson, elem);
                }
            }

            return elem;
        }

        public override string ToString()
        {
            return string.Format("{0}(Name=\"{1}\", ID={2})", this.GetType().Name, this.Name, this.Id);
        }
    }
}
