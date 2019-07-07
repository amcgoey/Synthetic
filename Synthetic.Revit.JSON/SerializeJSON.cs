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
using revitElemType = Autodesk.Revit.DB.ElementType;
using revitMaterial = Autodesk.Revit.DB.Material;

using Revit.Elements;
using dynElem = Revit.Elements.Element;
using dynMaterial = Revit.Elements.Material;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerializeJSON
    {
        public Dictionary<string, SerialElementType> ElementTypes { get; set; }
        public Dictionary<string, SerialMaterial> Materials { get; set; }
        public Dictionary<string, SerialElement> Elements { get; set; }

        internal SerializeJSON () { }

        public static SerialElement ByRevitElement (revitElem revitElement)
        {
            SerialElement serializeElement = null;

            if (revitElement != null)
            {
                serializeElement = _serialByType(revitElement);
            }

            return serializeElement;
        }

        public static SerialElement ByDynamoElement(dynElem dynamoElement)
        {
            SerialElement serializeElement = null;

            revitElem revitElement = dynamoElement.InternalElement;
            

            if (dynamoElement != null)
            {
                serializeElement = _serialByType(revitElement);
            }

            return serializeElement;
        }

        public static SerialElement DeserializeByJson (string Json)
        {
            SerialElement serialElem = JsonConvert.DeserializeObject<SerialElement>(Json);

            switch (serialElem.Class)
            {
                case "Autodesk.Revit.DB.Material":
                    serialElem = JsonConvert.DeserializeObject<SerialMaterial>(Json);
                    break;
                case "Autodesk.Revit.DB.ElementType":
                case "Autodesk.Revit.DB.TextNoteType":
                case "Autodesk.Revit.DB DimensionType":
                    serialElem = JsonConvert.DeserializeObject<SerialElementType>(Json);
                    break;
                default:
                    break;
            }
            
            return serialElem;
        }

        public static IEnumerable<SerialElement> DeserializeListByJson (string Json)
        {
            //SerialListElement tempList = JsonConvert.DeserializeObject<SerialListElement>(Json);
            List<string> stringList = JsonConvert.DeserializeObject<SerialListString>(Json).Elements;

            List<SerialElement> serialList = new List<SerialElement>();

            foreach (string s in stringList)
            {
                serialList.Add(SerializeJSON.DeserializeByJson(s));
            }

            return serialList;
        }

        public static string SerializeToJson (SerialElement serialElement)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(serialElement, Formatting.Indented);
        }

        public static string SerializeToJson (List<SerialElement> serialList)
        {
            SerialListElement list = new SerialListElement(serialList);
            return Newtonsoft.Json.JsonConvert.SerializeObject(list, Formatting.Indented);
        }

        #region Helper Functions
        private static SerialElement _serialByType (revitElem revitElement)
        {
            SerialElement serializeElement = null;
            Type revitType = revitElement.GetType();

            if (revitType == typeof(revitMaterial))
            {
                serializeElement = new SerialMaterial((revitMaterial)revitElement);
            }
            else if (revitType == typeof(revitElemType))
            {
                serializeElement = new SerialElementType((revitElemType)revitElement);
            }
            else
            {
                serializeElement = new SerialElement(revitElement);
            }

            return serializeElement;
        }
        #endregion
    }
}
