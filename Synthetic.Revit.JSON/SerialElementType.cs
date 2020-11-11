using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

using Autodesk.DesignScript.Runtime;
using RevitServices.Transactions;

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

        public SerialElementType (RevitElemType revitElemType, [DefaultArgument("true")] bool IsTemplate) : base (revitElemType, IsTemplate) { }

        public SerialElementType (DynElem dynamoElemType, [DefaultArgument("true")] bool IsTemplate) : base (dynamoElemType, IsTemplate) { }
        
        public SerialElementType (SerialElement serialElement, [DefaultArgument("true")] bool IsTemplate) : base (serialElement.Element, IsTemplate) { }

        #endregion
        #region Public Methods

        public static DynElem CreateElementTypeByTemplate (SerialElementType serialElementType, RevitElemType templateElemType,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            string transactionName = "Duplicate Template Element Type";

            DynElem dElem = null;

            // Intialize an empty newType.
            RevitElemType newType = (RevitElemType)serialElementType.GetRevitElem(document);

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
                if(document.IsModifiable)
                {
                    TransactionManager.Instance.EnsureInTransaction(document);
                    newType = templateElemType.Duplicate(serialElementType.Name);
                    TransactionManager.Instance.TransactionTaskDone();
                }
                else
                {
                    using (Autodesk.Revit.DB.Transaction trans = new Autodesk.Revit.DB.Transaction(document))
                    {
                        trans.Start(transactionName);
                        newType = templateElemType.Duplicate(serialElementType.Name);
                        trans.Commit();
                    }
                }
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
            if (elemClass == typeof(RevitDB.TextElementType))
            {
                collector.WherePasses( new RevitDB.ElementClassFilter(typeof(RevitDB.TextNoteType), true) );
            }
            RevitElemType template = collector.OfClass(elemClass).OfType<RevitElemType>()
                .FirstOrDefault();

            return CreateElementTypeByTemplate(serialElementType, template, document);
        }
        #endregion
        /// <summary>
        /// Given a SerialElementType, changes all instances of aliases of that type to the type.  Will create the type if it isn't already in the project.
        /// </summary>
        /// <param name="serialElementType">A serialized version of an ElementType</param>
        /// <param name="document">A Revit document.  Defaults to the current document.</param>
        /// <returns name="Merged">A list of instances that were successfully changed to the SerialElement type</returns>
        /// <returns name="Failed">A list of instances that failed to changed to the SerialElement type</returns>
        [MultiReturn(new[] { "Merged", "Failed" })]
        public static IDictionary MergeAliasTypes(SerialElementType serialElementType,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            // Intialize list for elements that are successfully merged and failed to merge.
            List<RevitElem> elementsMerged = new List<RevitElem>();
            List<RevitElem> elementsFailed = new List<RevitElem>();

            RevitElemType elemType = null;

            List<RevitElem> aliasTypes = serialElementType.GetAliasElements_Revit(document);

            //  If the SerialElement isn't already associated with a Revit Element
            if (serialElementType.ElementType == null)
            {
                //  If the SerialElement has an 
                if (serialElementType.ElementId != null)
                {
                    elemType = serialElementType.ElementType = (RevitElemType)serialElementType.GetRevitElem(document);
                }
                //If the elemType is still null and there is a name of the type, look for it.
                if (serialElementType.Name != null && elemType == null)
                {
                    elemType = (RevitElemType) Synthetic.Revit.Select.GetElementTypeByName_Revit(serialElementType.Name, document);
                }
            }
            //  If there is an Revit Element associated with the SerialElement, return it.
            else
            {
                elemType = serialElementType.ElementType;
            }

            //  If elemType is still null, then make the element.
            if (elemType == null)
            {
                CreateElementType(serialElementType, document);
            }

            if (elemType != null)
            {
                foreach (RevitElem alias in aliasTypes)
                {
                    if (alias != null)
                    {
                        Dictionary<string, object> r = (Dictionary<string, object>)Synthetic.Revit.Elements.MergeElementTypesRevit(alias, elemType);
                        List<RevitElem> m = (List<RevitElem>)r["Merged"];
                        List<RevitElem> f = (List<RevitElem>)r["Failed"];
                        elementsMerged.AddRange(m);
                        elementsFailed.AddRange(f);
                    }
                }
            }

            return new Dictionary<string, object>
            {
                {"Merged", elementsMerged},
                {"Failed", elementsFailed}
            };
        }
    }
}
