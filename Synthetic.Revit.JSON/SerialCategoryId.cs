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
        public int CategoryId { get; set; }

        [JsonIgnoreAttribute]
        public RevitCategory Category { get; set; }

        [JsonIgnoreAttribute]
        public RevitDoc Document { get; set; }

        public SerialCategoryId () { }

        public SerialCategoryId (RevitCategory category,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc Document)
        {
            this.Category = category;
            this.Document = Document;

            this.Name = category.Name;
            this.CategoryId = category.Id.IntegerValue;
        }

        public RevitCategory GetCategory(
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc Document)
        {
            if(Category == null)
            {
                this.Category = RevitCategory.GetCategory(Document, new RevitElemId(this.CategoryId));
            }
            return this.Category;
        }
    }
}
