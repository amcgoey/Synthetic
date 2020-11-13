using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;

using RevitDB = Autodesk.Revit.DB;
using RevitDoc = Autodesk.Revit.DB.Document;
using RevitView = Autodesk.Revit.DB.View;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    [SupressImportIntoVM]
    public class SerialCategoryGraphicOverrides : SerialObject
    {
        public SerialCategoryId Category { get; set; }
        public bool IsHidden { get; set; }
        public SerialOverrideGraphicSettings GraphicOverride { get; set; }

        public SerialCategoryGraphicOverrides () { }

        public SerialCategoryGraphicOverrides (RevitDB.Category category, RevitView view)
        {
            RevitDoc document = view.Document;

            this.Category = new SerialCategoryId(category, document, false);
            this.IsHidden = view.GetCategoryHidden(category.Id);

            RevitDB.OverrideGraphicSettings overrideSettings = view.GetCategoryOverrides(category.Id);
            SerialOverrideGraphicSettings serialOverride = new SerialOverrideGraphicSettings(category, overrideSettings, document);

            if (serialOverride.IsModified)
            {
                this.GraphicOverride = serialOverride;
            }
        }

        public bool IsModified()
        {
            bool modified = false;

            if (this.GraphicOverride != null)
            {
                if (this.GraphicOverride.IsModified) { modified = true; }
            }
            if (this.IsHidden == true) { modified = true; }

            return modified;
        }

        public void ModifyOverrideGraphicSettings(RevitView view)
        {
            RevitDoc document = view.Document;
            RevitDB.Category category = Category.GetCategory(document);

            if (category != null)
            {
                view.SetCategoryHidden(category.Id, this.IsHidden);

                if (this.GraphicOverride != null)
                {
                    RevitDB.OverrideGraphicSettings ogs = this.GraphicOverride.ToOverrideGraphicSettings();
                    view.SetCategoryOverrides(category.Id, ogs);
                }
            }

        }
    }
}
