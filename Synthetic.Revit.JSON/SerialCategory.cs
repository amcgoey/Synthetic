using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;

using RevitDB = Autodesk.Revit.DB;
using RevitDoc = Autodesk.Revit.DB.Document;
using RevitCategory = Autodesk.Revit.DB.Category;
using RevitElemId = Autodesk.Revit.DB.ElementId;

using Revit.Elements;
using DynCategory = Revit.Elements.Category;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerialCategory : SerialObject
    {
        public string Name
        {
            get => this.CategoryId.Name;
            set => this.CategoryId.Name = value;
        }

        public int Id
        {
            get => this.CategoryId.Id;
            set => this.CategoryId.Id = value;
        }

        public int? LineWeightProjection { get; set; }

        public int? LineWeightCut { get; set; }

        public SerialElementId LinePatternProjection { get; set; }

        public SerialElementId LinePatternCut { get; set; }

        [JsonIgnoreAttribute]
        public SerialCategoryId CategoryId { get; set; }

        [JsonIgnoreAttribute]
        public RevitCategory Category
        {
            get => this.CategoryId.Category;
            set => this.CategoryId.Category = value;
        }

        [JsonIgnoreAttribute]
        public RevitDoc Document
        {
            get => this.CategoryId.Document;
            set => this.CategoryId.Document = value;
        }

        /// <summary>
        /// If true, SerialElement is intended to be deserialized as a template for use as standards or transfer to another project.
        /// If false, SerialElement is intended to modify an element inside the project and will include ElementIds and UniqueIds.
        /// </summary>
        [JsonIgnoreAttribute]
        public bool IsTemplate { get; set; }

        /// <summary>
        /// If IsTemplate, don't serialize the Element Id
        /// </summary>
        /// <returns>True if not a template</returns>
        public bool ShouldSerializeId()
        {
            return !IsTemplate;
        }

        public SerialCategory() : base()
        {
            this.CategoryId = new SerialCategoryId();
            this.IsTemplate = true;
        }

        public SerialCategory(RevitCategory category,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc Document,
            bool IsTemplate)
        {
            this.IsTemplate = IsTemplate;
            this.CategoryId = new SerialCategoryId(category, Document, IsTemplate);

            this.LineWeightCut = category.GetLineWeight(RevitDB.GraphicsStyleType.Cut);
            this.LineWeightProjection = category.GetLineWeight(RevitDB.GraphicsStyleType.Projection);

            this.LinePatternCut = new SerialElementId(category.GetLinePatternId(RevitDB.GraphicsStyleType.Cut), Document, IsTemplate);
            this.LinePatternProjection = new SerialElementId(category.GetLinePatternId(RevitDB.GraphicsStyleType.Projection), Document, IsTemplate);
        }
    }
}
