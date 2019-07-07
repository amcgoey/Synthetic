using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;
using revitElem = Autodesk.Revit.DB.Element;
using revitElemId = Autodesk.Revit.DB.ElementId;
using revitMaterial = Autodesk.Revit.DB.Material;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerializeJSON
    {
        internal SerializeJSON () { }

        public static SerializeElement ByRevitElement (revitElem element)
        {
            SerializeElement serializeElement = null;

            if (element != null)
            {
                if (element.GetType() == typeof(revitMaterial))
                {
                    serializeElement = new SerializeMaterial((revitMaterial)element);
                }
                else
                {
                    serializeElement = new SerializeElement(element);
                }
            }

            return serializeElement;
        }

        public static SerializeElement DeserializeByJson (string Json)
        {
            SerializeElement serialElem = JsonConvert.DeserializeObject<SerializeElement>(Json);

            if(serialElem.Class == "Autodesk.Revit.DB.Material")
            {
                serialElem = JsonConvert.DeserializeObject<SerializeMaterial>(Json);
            }
            
            return serialElem;
        }

        public static IEnumerable<SerializeElement> DeserializeListByJson (string Json)
        {
            SerializeListElement stringList = JsonConvert.DeserializeObject<SerializeListElement>(Json);
            List<SerializeElement> serialList = stringList.Elements;

            //foreach(string s in stringList)
            //{
            //    serialList.Add(SerializeJSON.DeserializeByJson(s));
            //}

            return serialList;
        }

        public static string SerializeElementToJson (SerializeElement serialElement)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(serialElement, Formatting.Indented);
        }

        public static string SerializeListToJson (List<SerializeElement> serialList)
        {
            SerializeListElement list = new SerializeListElement(serialList);
            return Newtonsoft.Json.JsonConvert.SerializeObject(list, Formatting.Indented);
        }
    }
}
