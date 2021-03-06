﻿using System;
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
using RevitView = Autodesk.Revit.DB.View;
using RevitCat = Autodesk.Revit.DB.Category;

using Revit.Elements;
using dynElem = Revit.Elements.Element;
using dynMaterial = Revit.Elements.Material;
using dynCat = Revit.Elements.Category;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerializeJSON
    {
        #region Public Properties
        public Dictionary<string, SerialMaterial> Materials { get; set; }
        public Dictionary<string, SerialElementType> ElementTypes { get; set; }
        public Dictionary<string, SerialHostObjType> HostObjTypes { get; set; }
        public Dictionary<string, SerialView> Views { get; set; }
        public Dictionary<string, SerialCategory> Categories { get; set; }
        public List<SerialElement> Elements { get; set; }

        #endregion
        #region Internal Constructors

        internal SerializeJSON ()
        {
            Materials = new Dictionary<string, SerialMaterial>();
            ElementTypes = new Dictionary<string, SerialElementType>();
            HostObjTypes = new Dictionary<string, SerialHostObjType>();
            Views = new Dictionary<string, SerialView>();
            Categories = new Dictionary<string, SerialCategory>();
            Elements = new List<SerialElement>();
        }

        #endregion
        #region Public SerialElement Creation Methods

        public static SerialElement ByRevitElement (RevitElem revitElement, [DefaultArgument("true")] bool IsTemplate)
        {
            SerialElement serializeElement = null;

            if (revitElement != null)
            {
                serializeElement = _serialByType(revitElement, IsTemplate);
            }

            return serializeElement;
        }

        public static SerialElement ByDynamoElement(dynElem dynamoElement, [DefaultArgument("true")] bool IsTemplate)
        {
            SerialElement serializeElement = null;

            RevitElem revitElement = dynamoElement.InternalElement;
            

            if (dynamoElement != null)
            {
                serializeElement = _serialByType(revitElement, IsTemplate);
            }

            return serializeElement;
        }

        /// <summary>
        /// Creates a SerialCategory object from a Revit Category
        /// </summary>
        /// <param name="Category">A Revit Category object</param>
        /// <param name="document">The document the category belongs too.</param>
        /// <param name="IsTemplate">If true, SerialElement is intended to be deserialized as a template for use as standards or transfer to another project.
        /// If false, SerialElement is intended to modify an element inside the project and will include ElementIds and UniqueIds.</param>
        /// <returns>A SertialCategory object</returns>
        public static SerialCategory ByCategory(RevitCat Category,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document,
            [DefaultArgument("true")] bool IsTemplate)
        {
            SerialCategory serialCategory = null;

            if (Category != null)
            {
                new SerialCategory(Category, document, IsTemplate);
            }

            return serialCategory;
        }

        #endregion
        #region Serialize and Deserialize Methods

        public static IEnumerable<SerialElement> DeserializeByJson (string Json)
        {
            SerializeJSON serializeJSON = JsonConvert.DeserializeObject<SerializeJSON>(Json);

            return serializeJSON.Materials.Values.ToList<SerialElement>()
                .Concat(serializeJSON.ElementTypes.Values.ToList <SerialElement>())
                .Concat(serializeJSON.HostObjTypes.Values.ToList<SerialElement>())
                .Concat(serializeJSON.Views.Values.ToList<SerialElement>())
                .Concat(serializeJSON.Elements);
        }
        
        public static string SerializeToJson (List<SerialObject> serialList)
        {
            SerializeJSON serializeJSON = new SerializeJSON();
            
            foreach (SerialObject se in serialList)
            {
                serializeJSON._sortSerialObject(se);
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

        private static SerialElement _serialByType(RevitElem revitElement, bool IsTemplate)
        {
            SerialElement serializeElement = null;
            string revitType = revitElement.GetType().FullName;

            switch (revitType)
            {
                case "Autodesk.Revit.DB.Material":
                    serializeElement = new SerialMaterial((RevitMaterial)revitElement, IsTemplate);
                    break;
                case "Autodesk.Revit.DB.ElementType":
                case "Autodesk.Revit.DB.TextNoteType":
                case "Autodesk.Revit.DB.TextElementType":
                case "Autodesk.Revit.DB DimensionType":
                    serializeElement = new SerialElementType((RevitElemType)revitElement, IsTemplate);
                    break;
                case "Autodesk.Revit.DB.WallType":
                case "Autodesk.Revit.DB.FloorType":
                case "Autodesk.Revit.DB.CeilingType":
                case "Autodesk.Revit.DB.RoofType":
                case "Autodesk.Revit.DB.BuildingPadType":
                    serializeElement = new SerialHostObjType((RevitHostObjType)revitElement, IsTemplate);
                    break;
                case "Autodesk.Revit.DB.View":
                case "Autodesk.Revit.DB.TableView":
                case "Autodesk.Revit.DB.View3D":
                case "Autodesk.Revit.DB.ViewDrafting":
                case "Autodesk.Revit.DB.ViewPlan":
                case "Autodesk.Revit.DB.ViewSection":
                case "Autodesk.Revit.DB.ViewSheet":
                    serializeElement = new SerialView((RevitView)revitElement, IsTemplate);
                    break;
                default:
                    serializeElement = new SerialElement(revitElement, IsTemplate);
                    break;
            }

            return serializeElement;
        }

        private void _sortSerialObject(SerialObject serialElement)
        {
            Type type = serialElement.GetType();

            if (type == typeof(SerialMaterial))
            {
                SerialMaterial elem = (SerialMaterial)serialElement;
                this.Materials.Add(elem.Name, elem);
            }
            else if (type == typeof(SerialElementType))
            {
                SerialElementType elem = (SerialElementType)serialElement;
                this.ElementTypes.Add(elem.Name, elem);
            }
            else if (type == typeof(SerialHostObjType))
            {
                SerialHostObjType elem = (SerialHostObjType)serialElement;
                this.HostObjTypes.Add(elem.Name, elem);
            }
            else if (type == typeof(SerialView))
            {
                SerialView elem = (SerialView)serialElement;
                this.Views.Add(elem.Name, elem);
            }
            else if (type == typeof(SerialCategory))
            {
                SerialCategory elem = (SerialCategory)serialElement;
                this.Categories.Add(elem.Name, elem);
            }
            else
            {
                SerialElement elem = (SerialElement)serialElement;
                this.Elements.Add(elem);
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
            else if (type == typeof(SerialView))
            {
                elem = SerialView.ModifyView((SerialView)serialElement, document);
            }
            else
            {
                elem = SerialElement.ModifyElement((SerialElement)serialElement, document);
            }

            return elem;
        }

        public static dynElem CreateElementTypeByTemplate (SerialElementType serialElementType, dynElem templateType)
        {
            RevitDoc document = templateType.InternalElement.Document;

            dynElem elem = SerialElementType.CreateElementTypeByTemplate(serialElementType, (RevitElemType)templateType.InternalElement, document);

            return elem;
        }

        public static dynElem CreateElementType(SerialElementType serialElementType, [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            dynElem elem = SerialElementType.CreateElementType(serialElementType, document);

            return elem;
        }

        #endregion
        #region Creation and Modification Helper Functions



        #endregion
    }
}
