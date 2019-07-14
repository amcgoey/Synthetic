using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;

using RevitDB = Autodesk.Revit.DB;
using RevitDoc = Autodesk.Revit.DB.Document;
using RevitElem = Autodesk.Revit.DB.Element;
using RevitElemId = Autodesk.Revit.DB.ElementId;
using RevitElemType = Autodesk.Revit.DB.ElementType;
using RevitMaterial = Autodesk.Revit.DB.Material;
using RevitHostObjType = Autodesk.Revit.DB.HostObjAttributes;

using Revit.Elements;
using dynElem = Revit.Elements.Element;
using dynMaterial = Revit.Elements.Material;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerializeJSON
    {
        #region Public Properties
        public Dictionary<string, SerialMaterial> Materials { get; set; }
        public Dictionary<string, SerialElementType> ElementTypes { get; set; }
        public Dictionary<string, SerialHostObjType> HostObjTypes { get; set; }
        public List<SerialElement> Elements { get; set; }

        #endregion
        #region Internal Constructors

        internal SerializeJSON ()
        {
            Materials = new Dictionary<string, SerialMaterial>();
            ElementTypes = new Dictionary<string, SerialElementType>();
            HostObjTypes = new Dictionary<string, SerialHostObjType>();
            Elements = new List<SerialElement>();
        }

        #endregion
        #region Public SerialElement Creation Methods

        public static SerialElement ByRevitElement (RevitElem revitElement)
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

            RevitElem revitElement = dynamoElement.InternalElement;
            

            if (dynamoElement != null)
            {
                serializeElement = _serialByType(revitElement);
            }

            return serializeElement;
        }

        #endregion
        #region Serialize and Deserialize Methods

        public static IEnumerable<SerialElement> DeserializeByJson (string Json)
        {
            SerializeJSON serializeJSON = JsonConvert.DeserializeObject<SerializeJSON>(Json);

            return serializeJSON.Materials.Values.ToList<SerialElement>()
                .Concat(serializeJSON.ElementTypes.Values.ToList <SerialElement>())
                .Concat(serializeJSON.HostObjTypes.Values.ToList<SerialElement>())
                .Concat(serializeJSON.Elements);
        }
        
        public static string SerializeToJson (List<SerialElement> serialList)
        {
            SerializeJSON serializeJSON = new SerializeJSON();
            
            foreach (SerialElement se in serialList)
            {
                serializeJSON._sortSerialElement(se);
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(serializeJSON, Formatting.Indented);
        }

        //public static SerialElement DeserializeByJson (string Json)
        //{
        //    SerialElement serialElem = JsonConvert.DeserializeObject<SerialElement>(Json);

        //    switch (serialElem.Class)
        //    {
        //        case "Autodesk.Revit.DB.Material":
        //            serialElem = JsonConvert.DeserializeObject<SerialMaterial>(Json);
        //            break;
        //        case "Autodesk.Revit.DB.ElementType":
        //        case "Autodesk.Revit.DB.TextNoteType":
        //        case "Autodesk.Revit.DB DimensionType":
        //            serialElem = JsonConvert.DeserializeObject<SerialElementType>(Json);
        //            break;
        //        default:
        //            break;
        //    }

        //    return serialElem;
        //}

        //public static string SerializeToJson (SerialElement serialElement)
        //{
        //    SerializeJSON serializeJSON = new SerializeJSON();
        //    serializeJSON._sortSerialElement(serialElement);
        //    return Newtonsoft.Json.JsonConvert.SerializeObject(serializeJSON, Formatting.Indented);
        //}

        #endregion
        #region Serialization Helper Functions

        private static SerialElement _serialByType(RevitElem revitElement)
        {
            SerialElement serializeElement = null;
            string revitType = revitElement.GetType().FullName;

            switch (revitType)
            {
                case "Autodesk.Revit.DB.Material":
                    serializeElement = new SerialMaterial((RevitMaterial)revitElement);
                    break;
                case "Autodesk.Revit.DB.ElementType":
                case "Autodesk.Revit.DB.TextNoteType":
                case "Autodesk.Revit.DB DimensionType":
                    serializeElement = new SerialElementType((RevitElemType)revitElement);
                    break;
                case "Autodesk.Revit.DB.WallType":
                    serializeElement = new SerialHostObjType((RevitHostObjType)revitElement);
                    break;
                default:
                    serializeElement = new SerialElement(revitElement);
                    break;
            }

            return serializeElement;
        }

        private void _sortSerialElement(SerialElement serialElement)
        {
            Type type = serialElement.GetType();

            if (type == typeof(SerialMaterial))
            {
                this.Materials.Add(serialElement.Name, (SerialMaterial)serialElement);
            }
            else if (type == typeof(SerialElementType))
            {
                this.ElementTypes.Add(serialElement.Name, (SerialElementType)serialElement);
            }
            else if (type == typeof(SerialHostObjType))
            {
                this.HostObjTypes.Add(serialElement.Name, (SerialHostObjType)serialElement);
            }
            else
            {
                this.Elements.Add((SerialElement)serialElement);
            }
        }

        #endregion
        #region Element Creation and Modification Methods

        public static dynElem ModifyElement (SerialElement serialElement,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            dynElem elem = null;
            Type type = serialElement.GetType();

            if (type == typeof(SerialMaterial))
            {
                elem = SerialMaterial.ModifyMaterial((SerialMaterial)serialElement, document);

            }
            else if (type == typeof(SerialElementType))
            {
                elem = SerialElementType.ModifyElement((SerialElementType)serialElement, document);
            }
            else if (type == typeof(SerialHostObjType))
            {
                elem = SerialHostObjType.ModifyWallType((SerialHostObjType)serialElement, document);
            }
            else
            {
                elem = SerialElement.ModifyElement((SerialElement)serialElement, document);
            }

            return elem;
        }

        public static dynElem CreateElementType (SerialElementType serialElementType, dynElem templateType)
        {
            RevitDoc document = templateType.InternalElement.Document;

            dynElem elem = SerialElementType.CreateElementTypeByTemplate(serialElementType, (RevitElemType)templateType.InternalElement, document);

            return elem;
        }

        #endregion
        #region Creation and Modification Helper Functions

        

        #endregion
    }
}
