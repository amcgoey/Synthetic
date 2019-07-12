using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;
using revitMaterial = Autodesk.Revit.DB.Material;

using Revit.Elements;
using dynElem = Revit.Elements.Element;
using dynMaterial = Revit.Elements.Material;

using Select = Synthetic.Revit.Select;

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
        public revitMaterial Material { get; set; }

        [JsonIgnoreAttribute]
        public override revitDB.Element Element
        {
            get => this.Material;
            set => this.Material = (revitMaterial)value;
        }
        #endregion

        #region Public Constructors
        public SerialMaterial () : base () { }

        public SerialMaterial (dynElem dynamoMaterial) : base (dynamoMaterial)
        {
            revitMaterial mat = (revitMaterial)dynamoMaterial.InternalElement;
            revitDoc document = mat.Document;
            _ApplyProperties(mat);
            _ApplyDocProperties(mat, document);
        }

        public SerialMaterial (revitMaterial revitMaterial) : base (revitMaterial)
        {
            revitDoc document = revitMaterial.Document;
            _ApplyProperties(revitMaterial);
            _ApplyDocProperties(revitMaterial, document);
        }

        public SerialMaterial (SerialElement serialElement) : base (serialElement.Element)
        {
            if (serialElement.Element.GetType() == typeof(revitMaterial))
            {
                revitDoc document = serialElement.Document;
                if (document != null)
                {
                    revitMaterial material = (revitMaterial)serialElement.ElementId.GetElem(document);
                    _ApplyProperties(material);
                    _ApplyDocProperties(material, document);
                }
            }
        }
        #endregion

        #region Constructor Helper Functions
        private void _ApplyProperties(revitMaterial material)
        {
            this.CutForegroundPatternColor = new SerialColor(material.CutPatternColor);
            this.SurfaceForegroundPatternColor = new SerialColor(material.SurfacePatternColor);
        }

        private void _ApplyDocProperties(revitMaterial material, revitDoc document)
        {
            this.CutForegroundPatternId = new SerialElementId(material.CutPatternId, document);
            this.SurfaceForegroundPatternId = new SerialElementId(material.SurfacePatternId, document);
            this.AppearanceAssetId = new SerialElementId(material.AppearanceAssetId, document);
        }
        #endregion

        #region Public Methods
        
        public static dynElem CreateMaterial (SerialMaterial serialMaterial,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            revitMaterial mat = (revitMaterial)serialMaterial.GetElem(document);

            if (mat == null)
            {
                revitDB.ElementId matId = revitMaterial.Create(document, serialMaterial.Name);
                mat = (revitMaterial)document.GetElement(matId);
            }

            if (mat != null)
            {
                serialMaterial._ModifyProperties(mat);
            }

            return mat.ToDSType(true);
        }

        public static dynElem ModifyMaterial(SerialMaterial serialMaterial,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            if (serialMaterial.Element == null)
            {
                serialMaterial.Element = (revitMaterial)serialMaterial.GetElem(document);
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
                serialMaterial._ModifyProperties((revitMaterial)serialMaterial.Element);
            }

            return serialMaterial.Element.ToDSType(true);
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
        private void _ModifyProperties (revitMaterial material)
        {
            material.Name = this.Name;

            material.CutPatternColor = this.CutForegroundPatternColor.ToColor();
            material.CutPatternId = this.CutForegroundPatternId.ToElementId();

            material.SurfacePatternColor = this.CutForegroundPatternColor.ToColor();
            material.SurfacePatternId = this.CutForegroundPatternId.ToElementId();

            material.AppearanceAssetId = this.AppearanceAssetId.ToElementId();

            foreach (SerialParameter paramJson in this.Parameters)
            {
                SerialParameter.ModifyParameter(paramJson, material);
            }
        }
        #endregion
    }
}
