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

using Newtonsoft.Json;

namespace Synthetic.Revit.Serialize
{
    public class SerialElementType : SerializeElement
    {
        public new const string ClassName = "Element Type";

        public SerialElementType () : base () { }

        public SerialElementType (revitElemType elemType) : base (elemType) { }
        
        public SerialElementType (SerializeElement serialElement) : base (serialElement.Element)
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

        public static revitElemType CreateElementTypeByTemplate (SerialElementType serialElementType, revitElemType templateElem,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            revitElemType newType = templateElem.Duplicate(serialElementType.Name);

            serialElementType.UniqueId = newType.UniqueId;
            serialElementType.Id = newType.Id.IntegerValue;

            return (revitElemType)SerialElementType.ModifyElement(serialElementType, document);
        }

        public static revitElemType CreateElementType (SerialElementType serialElementType,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            Assembly assembly = typeof(revitElem).Assembly;
            Type elemClass = assembly.GetType(serialElementType.Class);

            revitDB.FilteredElementCollector collector = new revitDB.FilteredElementCollector(document);
            revitElemType template = collector.OfClass(elemClass).OfType<revitElemType>()
                .FirstOrDefault();

            return CreateElementTypeByTemplate(serialElementType, template, document);
        }
    }
}
