using System;
using System.Collections.Generic;
using System.Linq;

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
    public class SerialCategoryId : SerialObject
    {
        public string Name { get; set; }
        public int Id { get; set; }

        [JsonIgnoreAttribute]
        public RevitCategory Category { get; set; }

        [JsonIgnoreAttribute]
        public RevitDoc Document { get; set; }

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


        public SerialCategoryId ()
        {
            this.IsTemplate = false;        
        }

        public SerialCategoryId (RevitCategory category,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc Document, bool IsTemplate)
        {
            this.Category = category;
            this.Document = Document;

            this.Name = category.Name;
            this.Id = category.Id.IntegerValue;
        }

        public RevitCategory GetCategory(
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc Document)
        {
            if(Category == null)
            {
                this.Category = RevitCategory.GetCategory(Document, new RevitElemId(this.Id));
            }
            return this.Category;
        }
    }
}
