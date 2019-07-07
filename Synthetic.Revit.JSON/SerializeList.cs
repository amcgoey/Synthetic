using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;
using revitElem = Autodesk.Revit.DB.Element;
using revitElemId = Autodesk.Revit.DB.ElementId;
using revitMaterial = Autodesk.Revit.DB.Material;


using Newtonsoft.Json;

namespace Synthetic.Revit.Serialize
{
    public class SerializeListElement
    {
        public List<SerializeElement> Elements { get; set; }

        public SerializeListElement()
        {
            this.Elements = new List<SerializeElement>();
        }

        public SerializeListElement(List<SerializeElement> ElementJSONs)
        {
            this.Elements = ElementJSONs;
        }

        public SerializeListElement(List<revitElem> elements)
        {
            this.Elements = new List<SerializeElement>();

            foreach (revitElem elem in elements)
            {
                Elements.Add(new SerializeElement(elem));
            }
        }

        public static SerializeListElement ByJSON(string JSON)
        {
            return JsonConvert.DeserializeObject<SerializeListElement>(JSON);
        }

        public static string ToJSON(SerializeListElement ListJSON)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(ListJSON, Formatting.Indented);
        }

        public static List<revitElem> ModifyElements(SerializeListElement ListJSON, revitDoc doc)
        {
            List<revitElem> elems = new List<revitElem>();

            foreach (SerializeElement e in ListJSON.Elements)
            {
                elems.Add(SerializeElement.ModifyElement(e, doc));
            }

            return elems;
        }

        public override string ToString()
        {
            return string.Format("{0}(Count=\"{1}\")", this.GetType().Name, this.Elements.Count());
        }
    }

    
    public class SerializeListMaterial : SerializeListElement
    {
        new public List<SerializeMaterial> Elements { get; set; }

        [JsonConstructor]
        public SerializeListMaterial(List<SerializeMaterial> MaterialJSONs) : base()
        {
            this.Elements = MaterialJSONs;
        }

        public SerializeListMaterial(List<revitMaterial> materials)
        {
            this.Elements = new List<SerializeMaterial>();

            foreach (revitMaterial mat in materials)
            {
                Elements.Add(new SerializeMaterial(mat));
            }
        }

        public SerializeListMaterial()
        {
            this.Elements = new List<SerializeMaterial>();
        }

        public static SerializeListMaterial ByJSON(string JSON)
        {
            SerializeListMaterial materials = JsonConvert.DeserializeObject<SerializeListMaterial>(JSON);
            return materials;
        }

        public static string ToJSON(SerializeListMaterial ListJSON)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(ListJSON, Formatting.Indented);
        }

        public static List<revitMaterial> ModifyElements(SerializeListMaterial listMaterialJSON, revitDoc doc)
        {
            List<revitMaterial> elems = new List<revitMaterial>();

            foreach (SerializeMaterial m in listMaterialJSON.Elements)
            {
                elems.Add(SerializeMaterial.ModifyElement(m, doc));
            }

            return elems;
        }
    }
}
