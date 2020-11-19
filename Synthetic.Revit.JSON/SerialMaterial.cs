using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Autodesk.DesignScript.Runtime;

using RevitDB = Autodesk.Revit.DB;
using RevitDoc = Autodesk.Revit.DB.Document;
using RevitMaterial = Autodesk.Revit.DB.Material;

using Revit.Elements;
using DynElem = Revit.Elements.Element;
using DynMaterial = Revit.Elements.Material;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerialMaterial : SerialElement
    {
        public new const string ClassName = "Material";

        #region Public Properties
        public SerialElementId AppearanceAssetId { get; set; }

        public SerialColor CutForegroundPatternColor { get; set; }
        public SerialElementId CutForegroundPatternId { get; set; }

        public SerialColor CutBackgroundPatternColor { get; set; }
        public SerialElementId CutBackgroundPatternId { get; set; }

        public SerialColor SurfaceForegroundPatternColor { get; set; }
        public SerialElementId SurfaceForegroundPatternId { get; set; }

        public SerialColor SurfaceBackgroundPatternColor { get; set; }
        public SerialElementId SurfaceBackgroundPatternId { get; set; }

        [JsonIgnoreAttribute]
        public RevitMaterial Material { get; set; }

        [JsonIgnoreAttribute]
        public override RevitDB.Element Element
        {
            get => this.Material;
            set => this.Material = (RevitMaterial)value;
        }
        #endregion

        #region Public Constructors

        public SerialMaterial () : base () { }

        public SerialMaterial (DynElem dynamoMaterial, [DefaultArgument("true")] bool IsTemplate) : base (dynamoMaterial, IsTemplate)
        {
            RevitMaterial mat = (RevitMaterial)dynamoMaterial.InternalElement;
            RevitDoc document = mat.Document;
            _ApplyProperties(mat);
            _ApplyDocProperties(mat, document);
        }

        public SerialMaterial (RevitMaterial revitMaterial, [DefaultArgument("true")] bool IsTemplate) : base (revitMaterial, IsTemplate)
        {
            RevitDoc document = revitMaterial.Document;
            _ApplyProperties(revitMaterial);
            _ApplyDocProperties(revitMaterial, document);
        }

        public SerialMaterial (SerialElement serialElement, [DefaultArgument("true")] bool IsTemplate) : base (serialElement.Element, IsTemplate)
        {
            if (serialElement.Element.GetType() == typeof(RevitMaterial))
            {
                RevitDoc document = serialElement.Document;
                if (document != null)
                {
                    RevitMaterial material = (RevitMaterial)serialElement.ElementId.GetElem(document);
                    _ApplyProperties(material);
                    _ApplyDocProperties(material, document);
                }
            }
        }
        #endregion

        #region Constructor Helper Functions
        private void _ApplyProperties(RevitMaterial material)
        {
            this.CutForegroundPatternColor = new SerialColor(material.CutForegroundPatternColor);
            this.CutBackgroundPatternColor = new SerialColor(material.CutBackgroundPatternColor);

            this.SurfaceForegroundPatternColor = new SerialColor(material.SurfaceForegroundPatternColor);          
            this.SurfaceBackgroundPatternColor = new SerialColor(material.SurfaceBackgroundPatternColor);
        }

        private void _ApplyDocProperties(RevitMaterial material, RevitDoc document)
        {
            this.CutForegroundPatternId = new SerialElementId(material.CutForegroundPatternId, document, this.IsTemplate);
            this.CutBackgroundPatternId = new SerialElementId(material.CutBackgroundPatternId, document, this.IsTemplate);

            this.SurfaceForegroundPatternId = new SerialElementId(material.SurfaceForegroundPatternId, document, this.IsTemplate);
            this.SurfaceBackgroundPatternId = new SerialElementId(material.SurfaceBackgroundPatternId, document, this.IsTemplate);

            this.AppearanceAssetId = new SerialElementId(material.AppearanceAssetId, document, this.IsTemplate);
        }
        #endregion

        #region Public Methods
        
        public static DynElem CreateMaterial (SerialMaterial serialMaterial,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            DynElem dElem = null;
            RevitMaterial mat = (RevitMaterial)serialMaterial.GetRevitElem(document);

            if (mat == null)
            {
                RevitDB.ElementId matId = RevitMaterial.Create(document, serialMaterial.Name);
                mat = (RevitMaterial)document.GetElement(matId);
            }

            if (mat != null)
            {
                serialMaterial._ModifyProperties(mat);
                dElem = mat.ToDSType(true);
            }

            return dElem;
        }

        public static DynElem ModifyMaterial(SerialMaterial serialMaterial,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            DynElem dElem = null;

            if (serialMaterial.Element == null)
            {
                serialMaterial.Element = (RevitMaterial)serialMaterial.GetRevitElem(document);
            }

            //revitMaterial mat = null;

            //if (serialMaterial.UniqueId != null)
            //{
            //    mat = (revitMaterial)document.GetElement(serialMaterial.UniqueId);
            //}
            //else if (serialMaterial.Id != 0)
            //{
            //    mat = (revitMaterial)document.GetElement(new revitDB.ElementId(serialMaterial.Id));
            //}
            //else if (serialMaterial.Name != null)
            //{
            //    mat = (revitMaterial)Select.ByNameClass(typeof(revitMaterial), serialMaterial.Name, document);
            //}
            //else
            //{
            //    revitDB.ElementId matId = revitMaterial.Create(document, serialMaterial.Name);
            //    mat = (revitMaterial)document.GetElement(matId);
            //}

            if (serialMaterial.Element != null)
            {
                serialMaterial._ModifyProperties((RevitMaterial)serialMaterial.Element);
                dElem = serialMaterial.Element.ToDSType(true);
            }

            return dElem;
        }

        public new static SerialMaterial ByJSON(string JSON)
        {
            return JsonConvert.DeserializeObject<SerialMaterial>(JSON);
        }

        public static string ToJSON(SerialMaterial materialJSON)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(materialJSON, Formatting.Indented);
        }
        #endregion

        #region Helper Functions
        private void _ModifyProperties (RevitMaterial material)
        {
            material.Name = this.Name;

            material.CutForegroundPatternColor = this.CutForegroundPatternColor.ToColor();
            material.CutForegroundPatternId = this.CutForegroundPatternId.ToElementId();

            material.CutBackgroundPatternColor = this.CutBackgroundPatternColor.ToColor();
            material.CutBackgroundPatternId = this.CutBackgroundPatternId.ToElementId();

            material.SurfaceForegroundPatternColor = this.CutForegroundPatternColor.ToColor();
            material.SurfaceForegroundPatternId = this.CutForegroundPatternId.ToElementId();

            material.SurfaceBackgroundPatternColor = this.CutBackgroundPatternColor.ToColor();
            material.SurfaceBackgroundPatternId = this.CutBackgroundPatternId.ToElementId();

            material.AppearanceAssetId = this.AppearanceAssetId.ToElementId();

            foreach (SerialParameter paramJson in this.Parameters)
            {
                SerialParameter.ModifyParameter(paramJson, material);
            }
        }
        #endregion
    }
}
