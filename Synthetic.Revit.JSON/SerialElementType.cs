using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

using Autodesk.DesignScript.Runtime;

using RevitDB = Autodesk.Revit.DB;
using RevitDoc = Autodesk.Revit.DB.Document;
using RevitElem = Autodesk.Revit.DB.Element;
using RevitElemType = Autodesk.Revit.DB.ElementType;
using RevitElemId = Autodesk.Revit.DB.ElementId;

using Revit.Elements;
using DynElem = Revit.Elements.Element;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerialElementType : SerialElement
    {
        #region Public Properties

        [JsonIgnoreAttribute]
        public RevitElemType ElementType { get; set; }

        [JsonIgnoreAttribute]
        public override RevitElem Element
        {
            get => this.ElementType;
            set => this.ElementType = (RevitElemType)value;
        }

        #endregion
        #region Public Constructors

        public SerialElementType () : base () { }

        public SerialElementType (RevitElemType revitElemType) : base (revitElemType) { }

        public SerialElementType (DynElem dynamoElemType) : base (dynamoElemType) { }
        
        public SerialElementType (SerialElement serialElement) : base (serialElement.Element) { }

        #endregion
        #region Public Methods

        public static DynElem CreateElementTypeByTemplate (SerialElementType serialElementType, RevitElemType templateElemType,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            DynElem dElem = null;

            // Intialize an empty newType.
            RevitElemType newType = (RevitElemType)serialElementType.GetElem(document);

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

            if (newType != null)
            {
                newSerial.Element = newType;
                newSerial.UniqueId = newType.UniqueId;
                newSerial.Id = newType.Id.IntegerValue;
                dElem = SerialElementType.ModifyElement(newSerial, document);
            }

            // Return the modified Dynamo Element of the ElementType.
            return dElem;
        }

        public static DynElem CreateElementType (SerialElementType serialElementType,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            Assembly assembly = typeof(RevitElem).Assembly;
            Type elemClass = assembly.GetType(serialElementType.Class);

            RevitDB.FilteredElementCollector collector = new RevitDB.FilteredElementCollector(document);
            RevitElemType template = collector.OfClass(elemClass).OfType<RevitElemType>()
                .FirstOrDefault();

            return CreateElementTypeByTemplate(serialElementType, template, document);
        }
        #endregion


    }
}
