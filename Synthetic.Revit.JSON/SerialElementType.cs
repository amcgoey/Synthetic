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

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerialElementType : SerialElement
    {
        public new const string ClassName = "Element Type";

        public SerialElementType () : base () { }

        public SerialElementType (revitElemType revitElemType) : base (revitElemType) { }

        public SerialElementType (dynElem dynamoElemType) : base (dynamoElemType) { }
        
        public SerialElementType (SerialElement serialElement) : base (serialElement.Element)
        {
            //this.Element = serialElement.Element;
            //this.Document = serialElement.Document;

            //this.Class = serialElement.Class;
            //this.Name = serialElement.Name;
            //this.Id = serialElement.Id;
            //this.UniqueId = serialElement.UniqueId;
            //this.Category = serialElement.Category;
            //this.Parameters = serialElement.Parameters;
        }

        #region Public Methods

        public static dynElem CreateElementTypeByTemplate (SerialElementType serialElementType, revitElemType templateElemType,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            // Intialize an empty newType.
            revitElemType newType = null;
            
            // Get the Revit Class of the ElementType
            Assembly assembly = typeof(revitElem).Assembly;
            Type elemClass = assembly.GetType(serialElementType.Class);

            // Check if a ElementType of that name already exists
            revitDB.FilteredElementCollector collector = new revitDB.FilteredElementCollector(document);
            newType = (revitElemType)collector.OfClass(elemClass)
                .FirstOrDefault(e => e.Name.Equals(serialElementType.Name));

            // Intialize list for alias ElementTypes
            List<revitElemType> aliasTypes = new List<revitElemType>();

            // Check if Aliases of the ElementType already exist
            if (serialElementType.Aliases != null && newType == null)
            {
                foreach (string alias in serialElementType.Aliases)
                {
                    aliasTypes.Add(
                    (revitElemType)collector.OfClass(elemClass)
                    .FirstOrDefault(e => e.Name.Equals(alias))
                    );
                }

                if (aliasTypes.FirstOrDefault() != null)
                {
                    newType = aliasTypes.FirstOrDefault();
                }
            }

            // If the ElementType doesn't exist, create a new type based on the template
            if (newType == null)
            {
                newType = templateElemType.Duplicate(serialElementType.Name);
            }

            // Set the SerialElementType's Ids to the newType
            serialElementType.UniqueId = newType.UniqueId;
            serialElementType.Id = newType.Id.IntegerValue;

            // Return the modified Dynamo Element of the ElementType.
            return SerialElementType.ModifyElement(serialElementType, document);
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
