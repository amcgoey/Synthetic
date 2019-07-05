using System;
using System.Collections.Generic;
using System.Text;

using revitDB = Autodesk.Revit.DB;
using revitMaterial = Autodesk.Revit.DB.Material;
using revitDoc = Autodesk.Revit.DB.Document;

using Newtonsoft.Json;

namespace Synthetic.Revit.JSON
{
    public class ListMaterialJSON : ListElementJSON
    {
        new public List<MaterialJSON> Elements { get; set; }

        [JsonConstructor]
        public ListMaterialJSON(List<MaterialJSON> MaterialJSONs) : base()
        {
            this.Elements = Elements;
        }

        public ListMaterialJSON(List<revitMaterial> materials)
        {
            this.Elements = new List<MaterialJSON>();

            foreach (revitMaterial mat in materials)
            {
                Elements.Add(new MaterialJSON(mat));
            }
        }

        public ListMaterialJSON()
        {
            this.Elements = new List<MaterialJSON>();
        }

        public static ListMaterialJSON ByJSON(string JSON)
        {
            return JsonConvert.DeserializeObject<ListMaterialJSON>(JSON);
        }

        public static string ToJSON(ListMaterialJSON ListJSON)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(ListJSON, Formatting.Indented);
        }

        public static List<revitMaterial> ModifyElements(ListMaterialJSON listMaterialJSON, revitDoc doc)
        {
            List<revitMaterial> elems = new List<revitMaterial>();

            foreach (MaterialJSON m in listMaterialJSON.Elements)
            {
                elems.Add(MaterialJSON.ModifyElement(m, doc));
            }

            return elems;
        }
    }

    public class MaterialJSON : ElementJSON
    {
        public new const string ClassName = "Material";

        //public ColorJSON Color { get; set; }
        //public int Transparency { get; set; }

        public ElementIdJSON AppearanceAssetId { get; set; }

        public ColorJSON CutForegroundPatternColor { get; set; }
        public ElementIdJSON CutForegroundPatternId { get; set; }

        public ColorJSON CutBackgroundPatternColor { get; set; }
        public ElementIdJSON CutBackgroundPatternId { get; set; }

        public ColorJSON SurfaceForegroundPatternColor { get; set; }
        public ElementIdJSON SurfaceForegroundPatternId { get; set; }

        public ColorJSON SurfaceBackgroundPatternColor { get; set; }
        public ElementIdJSON SurfaceBackgroundPatternId { get; set; }

        public MaterialJSON () : base () { }

        public MaterialJSON (revitMaterial material) : base (material)
        {
            revitDoc doc = material.Document;

            this.CutForegroundPatternColor = new ColorJSON(material.CutPatternColor);
            this.CutForegroundPatternId = new ElementIdJSON(material.CutPatternId, doc);
            this.SurfaceForegroundPatternColor = new ColorJSON(material.SurfacePatternColor);
            this.SurfaceForegroundPatternId = new ElementIdJSON(material.SurfacePatternId, doc);

            this.AppearanceAssetId = new ElementIdJSON(material.AppearanceAssetId, doc);
        }

        public static MaterialJSON ByJSON(string JSON)
        {
            return JsonConvert.DeserializeObject<MaterialJSON>(JSON);
        }

        public static string ToJSON(MaterialJSON materialJSON)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(materialJSON, Formatting.Indented);
        }

        public static revitMaterial ModifyElement(MaterialJSON materialJSON, revitDoc doc)
        {
            revitMaterial mat = (revitMaterial)doc.GetElement(materialJSON.UniqueId);

            mat.Name = materialJSON.Name;

            mat.CutPatternColor = ColorJSON.ToColor(materialJSON.CutForegroundPatternColor);
            mat.CutPatternId = ElementIdJSON.ToElementId(materialJSON.CutForegroundPatternId);

            mat.SurfacePatternColor = ColorJSON.ToColor(materialJSON.CutForegroundPatternColor);
            mat.SurfacePatternId = ElementIdJSON.ToElementId(materialJSON.CutForegroundPatternId);

            mat.AppearanceAssetId = ElementIdJSON.ToElementId(materialJSON.AppearanceAssetId);

            foreach (ParameterJSON paramJson in materialJSON.Parameters)
            {
                ParameterJSON.ModifyParameter(paramJson, mat);
            }

            return mat;
        }
    }
}
