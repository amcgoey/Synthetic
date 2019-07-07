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
            //this.Class = serialElement.Class;
            //this.Name = serialElement.Name;
            //this.Id = serialElement.Id;
            //this.UniqueId = serialElement.UniqueId;
            //this.Category = serialElement.Category;
            //this.Parameters = serialElement.Parameters;

            if (serialElement.Element.GetType() == typeof(revitMaterial))
            {
                revitDoc document = serialElement.Document;
                if (document != null)
                {
                    revitMaterial material = (revitMaterial)document.GetElement(serialElement.UniqueId);
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
        public static SerialMaterial ByJSON(string JSON)
        {
            return JsonConvert.DeserializeObject<SerialMaterial>(JSON);
        }

        public static string ToJSON(SerialMaterial materialJSON)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(materialJSON, Formatting.Indented);
        }

        public static dynElem ModifyMaterial(SerialMaterial serialMaterial, revitDoc doc)
        {
            revitMaterial mat = null;

            if (serialMaterial.UniqueId != null)
            {
                mat = (revitMaterial)doc.GetElement(serialMaterial.UniqueId);
            }
            else if (serialMaterial.Id != 0)
            {
                mat = (revitMaterial)doc.GetElement(new revitDB.ElementId(serialMaterial.Id));
            }
            else if (serialMaterial.Name != null)
            {
                revitDB.FilteredElementCollector collector = new revitDB.FilteredElementCollector(doc);
                mat = collector.OfClass(typeof(revitMaterial))
                    .OfType<revitMaterial>()
                    .FirstOrDefault(e => e.Name.Equals(serialMaterial.Name));
            }
            else
            {
                revitDB.ElementId matId = revitMaterial.Create(doc, serialMaterial.Name);
                mat = (revitMaterial)doc.GetElement(matId);
            }

            if (mat != null)
            {
                serialMaterial._ModifyProperties(mat);
            }

            return mat.ToDSType(true);
        }
        #endregion

        #region Helper Functions
        private void _ModifyProperties (revitMaterial material)
        {
            material.Name = this.Name;

            material.CutPatternColor = SerialColor.ToColor(this.CutForegroundPatternColor);
            material.CutPatternId = SerialElementId.ToElementId(this.CutForegroundPatternId);

            material.SurfacePatternColor = SerialColor.ToColor(this.CutForegroundPatternColor);
            material.SurfacePatternId = SerialElementId.ToElementId(this.CutForegroundPatternId);

            material.AppearanceAssetId = SerialElementId.ToElementId(this.AppearanceAssetId);

            foreach (SerialParameter paramJson in this.Parameters)
            {
                SerialParameter.ModifyParameter(paramJson, material);
            }
        }
        #endregion
    }
}
