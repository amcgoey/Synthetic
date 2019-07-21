using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autodesk.DesignScript.Runtime;

using RevitDB = Autodesk.Revit.DB;
using RevitDoc = Autodesk.Revit.DB.Document;
using RevitElem = Autodesk.Revit.DB.Element;
using RevitView = Autodesk.Revit.DB.View;

using Revit.Elements;
using DynElem = Revit.Elements.Element;
using DynView = Revit.Elements.Views.View;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerialView : SerialElement
    {
        #region Public Properties

        public const string CategoryKey = "Category";
        public const string HiddenKey = "Hidden";
        public const string OverrideKey = "OverrideGraphicSettings";
        

        public bool IsTemplate { get; set; }
        public SerialEnum DisplayStyle { get; set; }
        
        public int ShadowIntesnity { get; set; }
        public int SunlightIntensity { get; set; }

        public List<SerialCategoryGraphicOverrides> CategoryGraphicOverrides { get; set; }

        [JsonIgnoreAttribute]
        public RevitView View { get; set; }

        [JsonIgnoreAttribute]
        public override RevitDB.Element Element
        {
            get => this.View;
            set => this.View = (RevitView)value;
        }

        #endregion
        #region Public Constructors

        public SerialView () { }

        public SerialView (RevitView view) : base (view)
        {
            _ApplyProperties(view);
        }

        public SerialView (DynView view) : base (view)
        {
            RevitView rView = (RevitView)view.InternalElement;
            _ApplyProperties(rView);
        }

        #endregion
        #region Constructor Helper Functions

        private void _ApplyProperties (RevitView view)
        {
            this.IsTemplate = view.IsTemplate;
            this.DisplayStyle = new SerialEnum(typeof(RevitDB.DisplayStyle), view.DisplayStyle);
            
            this.SunlightIntensity = view.SunlightIntensity;
            this.ShadowIntesnity = view.ShadowIntensity;

            this.CategoryGraphicOverrides = _GetCategoryGraphicOverrides(view);
        }

        private List<SerialCategoryGraphicOverrides> _GetCategoryGraphicOverrides(RevitView view)
        {
            List<SerialCategoryGraphicOverrides> overrides = new List<SerialCategoryGraphicOverrides>();

            RevitDoc document = view.Document;
            RevitDB.Categories categories = document.Settings.Categories;

            foreach (RevitDB.Category category in categories)
            {
                bool catModified = false;

                SerialCategoryGraphicOverrides catOverride = new SerialCategoryGraphicOverrides(category, view);
                if (catOverride.IsModified())
                {
                    overrides.Add(catOverride);
                }

                RevitDB.CategoryNameMap subcategories = category.SubCategories;
                foreach (RevitDB.Category subCategory in subcategories)
                {
                    SerialCategoryGraphicOverrides subCatOverride = new SerialCategoryGraphicOverrides(subCategory, view);
                    if (subCatOverride.IsModified())
                    {
                        overrides.Add(subCatOverride);
                    }
                }
            }
            return overrides;
        }

        //private Dictionary<string, SerialObject> _CreateCategoryOverride (RevitDB.Category cat, RevitView view, RevitDoc document)
        //{
        //    bool catModified = false;

        //    RevitDB.OverrideGraphicSettings overrideSettings = view.GetCategoryOverrides(cat.Id);
        //    SerialOverrideGraphicSettings ogs = new SerialOverrideGraphicSettings(cat, overrideSettings, document);

        //    SerialBoolean categoryHidden = new SerialBoolean(view.GetCategoryHidden(cat.Id));

        //    Dictionary<string, SerialObject> dict = new Dictionary<string, SerialObject>();
        //    dict.Add(CategoryKey, new SerialCategory(cat, document));

        //    if (categoryHidden.boolean)
        //    {
        //        catModified = true;

        //        dict.Add(HiddenKey, categoryHidden);
        //    }

        //    if (ogs.IsModified)
        //    {
        //        catModified = true;
        //        dict.Add(OverrideKey, ogs);
        //    }

        //    if (catModified)
        //    {
        //        return dict;
        //    }
        //    else return null;
        //}

        #endregion
        #region Public Methods

        public static DynElem ModifyView (SerialView serialView,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            DynElem dElem = null;

            if (serialView.Element == null)
            {
                serialView.Element = (RevitView)serialView.GetRevitElem(document);
            }

            if (serialView.Element != null)
            {
                serialView._ModifyProperties((RevitView)serialView.Element, document);
                dElem = serialView.Element.ToDSType(true);
            }

            return dElem;
        }

        #endregion
        #region Helper Functions

        private void _ModifyProperties(RevitView view, RevitDoc document)
        {
            view.Name = this.Name;

            view.DisplayStyle = (RevitDB.DisplayStyle) this.DisplayStyle.ToEnum();

            view.SunlightIntensity = this.SunlightIntensity;
            view.ShadowIntensity = this.ShadowIntesnity;

            foreach (SerialParameter paramJson in this.Parameters)
            {
                SerialParameter.ModifyParameter(paramJson, view);
            }

            if (view.AreGraphicsOverridesAllowed())
            {
                //_SetCategoryOverrideGraphics(view);
                foreach (SerialCategoryGraphicOverrides catOverride in this.CategoryGraphicOverrides)
                {
                    catOverride.ModifyOverrideGraphicSettings(view);
                }
            }
        }

        private void _SetCategoryOverrideGraphics(RevitView view)
        {
            foreach (SerialCategoryGraphicOverrides catOverride in this.CategoryGraphicOverrides)
            {
                catOverride.ModifyOverrideGraphicSettings(view);
                //RevitDB.Category category = null;
                //RevitDB.OverrideGraphicSettings overrideSettings = null;

                //if (oatOverride.ContainsKey(CategoryKey))
                //{
                //    SerialCategoryId serialCategory = (SerialCategoryId)oatOverride[CategoryKey];
                //    category = serialCategory.GetCategory(document);
                //}
                //if (oatOverride.ContainsKey(OverrideKey))
                //{
                //    SerialOverrideGraphicSettings serialOgs = (SerialOverrideGraphicSettings)oatOverride[OverrideKey];
                //    overrideSettings = serialOgs.ToOverrideGraphicSettings();
                //}

                //if (category != null && overrideSettings != null)
                //{
                //    view.SetCategoryOverrides(category.Id, overrideSettings);
                //}
            }
        }

        #endregion
    }
}
