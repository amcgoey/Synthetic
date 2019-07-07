using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;
using revitMaterial = Autodesk.Revit.DB.Material;

using Newtonsoft.Json;

namespace Synthetic.Serialize.Revit
{
    public class SerializeMaterial : SerializeElement
    {
        public new const string ClassName = "Material";

        public SerializeElementId AppearanceAssetId { get; set; }

        public SerializeColor CutForegroundPatternColor { get; set; }
        public SerializeElementId CutForegroundPatternId { get; set; }

        public SerializeColor CutBackgroundPatternColor { get; set; }
        public SerializeElementId CutBackgroundPatternId { get; set; }

        public SerializeColor SurfaceForegroundPatternColor { get; set; }
        public SerializeElementId SurfaceForegroundPatternId { get; set; }

        public SerializeColor SurfaceBackgroundPatternColor { get; set; }
        public SerializeElementId SurfaceBackgroundPatternId { get; set; }

        public SerializeMaterial () : base () { }

        public SerializeMaterial (revitMaterial material) : base (material)
        {
            revitDoc document = material.Document;

            this.CutForegroundPatternColor = new SerializeColor(material.CutPatternColor);
            this.CutForegroundPatternId = new SerializeElementId(material.CutPatternId, document);
            this.SurfaceForegroundPatternColor = new SerializeColor(material.SurfacePatternColor);
            this.SurfaceForegroundPatternId = new SerializeElementId(material.SurfacePatternId, document);

            this.AppearanceAssetId = new SerializeElementId(material.AppearanceAssetId, document);
        }

        public SerializeMaterial (SerializeElement serialElement) : base (serialElement.Element)
        {
            //this.Class = serialElement.Class;
            //this.Name = serialElement.Name;
            //this.Id = serialElement.Id;
            //this.UniqueId = serialElement.UniqueId;
            //this.Category = serialElement.Category;
            //this.Parameters = serialElement.Parameters;

            if (serialElement.Element.GetType() == typeof(revitMaterial))
            {
                revitMaterial material = (revitMaterial)this.Document.GetElement(serialElement.UniqueId);

                this.CutForegroundPatternColor = new SerializeColor(material.CutPatternColor);
                this.CutForegroundPatternId = new SerializeElementId(material.CutPatternId, this.Document);
                this.SurfaceForegroundPatternColor = new SerializeColor(material.SurfacePatternColor);
                this.SurfaceForegroundPatternId = new SerializeElementId(material.SurfacePatternId, this.Document);

                this.AppearanceAssetId = new SerializeElementId(material.AppearanceAssetId, this.Document);
            }
        }

        public static SerializeMaterial ByJSON(string JSON)
        {
            return JsonConvert.DeserializeObject<SerializeMaterial>(JSON);
        }

        public static string ToJSON(SerializeMaterial materialJSON)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(materialJSON, Formatting.Indented);
        }

        public static revitMaterial ModifyElement(SerializeMaterial serialMaterial, revitDoc doc)
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
                mat.Name = serialMaterial.Name;

                mat.CutPatternColor = SerializeColor.ToColor(serialMaterial.CutForegroundPatternColor);
                mat.CutPatternId = SerializeElementId.ToElementId(serialMaterial.CutForegroundPatternId);

                mat.SurfacePatternColor = SerializeColor.ToColor(serialMaterial.CutForegroundPatternColor);
                mat.SurfacePatternId = SerializeElementId.ToElementId(serialMaterial.CutForegroundPatternId);

                mat.AppearanceAssetId = SerializeElementId.ToElementId(serialMaterial.AppearanceAssetId);

                foreach (SerializeParameter paramJson in serialMaterial.Parameters)
                {
                    SerializeParameter.ModifyParameter(paramJson, mat);
                }
            }

            return mat;
        }
    }
}
