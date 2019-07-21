using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;
using revitElem = Autodesk.Revit.DB.Element;
using revitElemId = Autodesk.Revit.DB.ElementId;
using revitMaterial = Autodesk.Revit.DB.Material;

using Revit.Elements;
using dynElem = Revit.Elements.Element;
using dynMaterial = Revit.Elements.Material;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerialListElement : SerialObject
    {
        public List<SerialElement> Elements { get; set; }

        public SerialListElement()
        {
            this.Elements = new List<SerialElement>();
        }

        public SerialListElement(List<SerialElement> ElementJSONs)
        {
            this.Elements = ElementJSONs;
        }

        public SerialListElement(List<revitElem> elements)
        {
            this.Elements = new List<SerialElement>();

            foreach (revitElem elem in elements)
            {
                Elements.Add(new SerialElement(elem));
            }
        }

        public static SerialListElement ByJSON(string JSON)
        {
            return JsonConvert.DeserializeObject<SerialListElement>(JSON);
        }

        public static string ToJSON(SerialListElement ListJSON)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(ListJSON, Formatting.Indented);
        }

        public static List<dynElem> ModifyElements(SerialListElement ListJSON, revitDoc doc)
        {
            List<dynElem> elems = new List<dynElem>();

            foreach (SerialElement e in ListJSON.Elements)
            {
                elems.Add(SerialElement.ModifyElement(e, doc));
            }

            return elems;
        }

        public override string ToString()
        {
            return string.Format("{0}(Count=\"{1}\")", this.GetType().Name, this.Elements.Count());
        }
    }

    
    public class SerialListMaterial : SerialListElement
    {
        new public List<SerialMaterial> Elements { get; set; }

        [JsonConstructor]
        public SerialListMaterial(List<SerialMaterial> MaterialJSONs) : base()
        {
            this.Elements = MaterialJSONs;
        }

        public SerialListMaterial(List<revitMaterial> materials)
        {
            this.Elements = new List<SerialMaterial>();

            foreach (revitMaterial mat in materials)
            {
                Elements.Add(new SerialMaterial(mat));
            }
        }

        public SerialListMaterial()
        {
            this.Elements = new List<SerialMaterial>();
        }

        public static SerialListMaterial ByJSON(string JSON)
        {
            SerialListMaterial materials = JsonConvert.DeserializeObject<SerialListMaterial>(JSON);
            return materials;
        }

        public static string ToJSON(SerialListMaterial ListJSON)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(ListJSON, Formatting.Indented);
        }

        public static List<dynElem> ModifyElements(SerialListMaterial listMaterialJSON, revitDoc doc)
        {
            List<dynElem> elems = new List<dynElem>();

            foreach (SerialMaterial m in listMaterialJSON.Elements)
            {
                elems.Add(SerialMaterial.ModifyMaterial(m, doc));
            }

            return elems;
        }

        public override string ToString()
        {
            return string.Format("{0}(Count=\"{1}\")", this.GetType().Name, this.Elements.Count());
        }
    }

    public class SerialListString
    {
        public List<string> Elements { get; set; }

        public SerialListString () {}
    }
}
