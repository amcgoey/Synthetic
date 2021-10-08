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
using RevitDimType = Autodesk.Revit.DB.DimensionType;
using RevitElemId = Autodesk.Revit.DB.ElementId;

using Revit.Elements;
using DynElem = Revit.Elements.Element;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    /// <summary>
    /// Serialize and deserialize Revit DimensionTypes
    /// </summary>
    public class SerialDimensionType : SerialElementType
    {
        #region Public Properties
        /// <summary>
        /// Revit DimensionStyle Type Enum
        /// </summary>
        public RevitDB.DimensionStyleType DimensionStyleType { get; set; }

        [JsonIgnoreAttribute]
        public RevitDimType DimensionType { get; set; }

        [JsonIgnoreAttribute]
        public override RevitElem Element
        {
            get => this.DimensionType;
            set => this.DimensionType = (RevitDimType)value;
        }


        /// <summary>
        /// Constant for the hidden built in linear dimension style.
        /// These styles shouldn't be serialized or used as templates to create new styles.
        /// </summary>
        [JsonIgnoreAttribute]
        [SupressImportIntoVM]
        public const string InternalDimStyleName = "Linear Dimension Style";

        #endregion
        #region Public Constructors

        public SerialDimensionType() : base()
        {
            DimensionStyleType = RevitDB.DimensionStyleType.Linear;
        }

        public SerialDimensionType(RevitElemType revitElemType, [DefaultArgument("true")] bool IsTemplate) : base(revitElemType, IsTemplate)
        {
            this.DimensionStyleType = this.DimensionType.StyleType;
        }

        public SerialDimensionType(DynElem dynamoElemType, [DefaultArgument("true")] bool IsTemplate) : base(dynamoElemType, IsTemplate)
        {
            this.DimensionStyleType = this.DimensionType.StyleType;
        }

        public SerialDimensionType(SerialElementType serialElement, [DefaultArgument("true")] bool IsTemplate) : base(serialElement.ElementType, IsTemplate)
        {
            this.DimensionStyleType = this.DimensionType.StyleType;
        }

        #endregion
        #region Public Methods

        /// <summary>
        /// Creates a new DimensionType of DimensionStyleType based on a found existing type.
        /// </summary>
        /// <param name="serialDimensionType">A SerialDimenionType</param>
        /// <param name="document">A Revit document.</param>
        /// <returns name="DimensionType">Returns a new DimensionType</returns>
        public static DynElem CreateDimensionType(SerialDimensionType serialDimensionType,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            RevitElemType template;
            Assembly assembly = typeof(RevitElem).Assembly;
            Type elemClass = assembly.GetType(serialDimensionType.Class);

            RevitDB.FilteredElementCollector collector = new RevitDB.FilteredElementCollector(document);
            


            template = collector
                    .OfClass(elemClass)
                    .OfType<RevitDB.DimensionType>()
                    .Where(elem => elem.StyleType.Equals(serialDimensionType.DimensionStyleType))
                    .Where(elem => elem.Name != InternalDimStyleName)
                    .FirstOrDefault();

            return CreateElementTypeByTemplate(serialDimensionType, template, document);
        }

        #endregion
    }
}
