using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;
using revitElem = Autodesk.Revit.DB.Element;
using revitElemType = Autodesk.Revit.DB.ElementType;
using revitElemId = Autodesk.Revit.DB.ElementId;
using revitMaterial = Autodesk.Revit.DB.Material;

using Revit.Elements;
using dynElem = Revit.Elements.Element;

using Select = Synthetic.Revit.Select;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerialElementType : SerialElement
    {
        #region Public Properties

        [JsonIgnoreAttribute]
        public revitElemType ElementType { get; set; }

        [JsonIgnoreAttribute]
        public override revitElem Element
        {
            get => this.ElementType;
            set => this.ElementType = (revitElemType)value;
        }

        #endregion
        #region Public Constructors

        public SerialElementType () : base () { }

        public SerialElementType (revitElemType revitElemType) : base (revitElemType) { }

        public SerialElementType (dynElem dynamoElemType) : base (dynamoElemType) { }
        
        public SerialElementType (SerialElement serialElement) : base (serialElement.Element) { }

        #endregion
        #region Public Methods

        public static dynElem CreateElementTypeByTemplate (SerialElementType serialElementType, revitElemType templateElemType,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            // Intialize an empty newType.
            revitElemType newType = (revitElemType)serialElementType.GetElem(document);

            SerialElementType newSerial = (SerialElementType)serialElementType.MemberwiseClone();
            
            //// Get the Revit Class of the ElementType
            //Assembly assembly = typeof(revitElem).Assembly;
            //Type elemClass = assembly.GetType(serialElementType.Class);

            //// Check if a ElementType of that name already exists
            //newType = (revitElemType)Select.ByNameClass(elemClass, serialElementType.Name, document);

            //// Intialize list for alias ElementTypes
            //List<revitElemType> aliasTypes = new List<revitElemType>();

            //// Check if Aliases of the ElementType already exist
            //if (serialElementType.Aliases != null && newType == null)
            //{
            //    foreach (string alias in serialElementType.Aliases)
            //    {
            //        aliasTypes.Add( (revitElemType) Select.ByNameClass(elemClass, alias, document));
            //    }

            //    if (aliasTypes.FirstOrDefault() != null)
            //    {
            //        newType = aliasTypes.FirstOrDefault();
            //    }
            //}

            // If the ElementType doesn't exist, create a new type based on the template
            if (newType == null)
            {
                newType = templateElemType.Duplicate(serialElementType.Name);
            }

            newSerial.Element = newType;
            newSerial.UniqueId = newType.UniqueId;
            newSerial.Id = newType.Id.IntegerValue;


            // Return the modified Dynamo Element of the ElementType.

            return SerialElementType.ModifyElement(newSerial, document);
        }

        public static dynElem CreateElementType (SerialElementType serialElementType,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            Assembly assembly = typeof(revitElem).Assembly;
            Type elemClass = assembly.GetType(serialElementType.Class);

            revitDB.FilteredElementCollector collector = new revitDB.FilteredElementCollector(document);
            revitElemType template = collector.OfClass(elemClass).OfType<revitElemType>()
                .FirstOrDefault();

            return CreateElementTypeByTemplate(serialElementType, template, document);
        }
        #endregion


    }
}
