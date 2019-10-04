using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using revitMaterial = Autodesk.Revit.DB.Material;
using revitDoc = Autodesk.Revit.DB.Document;

using Newtonsoft.Json;

namespace Synthetic.Revit.JSON
{
    public class MaterialJSON
    {
        public const string ClassName = "Material";

        public string Class { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public string UniqueId { get; set; }
        public string Category { get; set; }
        public List<ParameterJSON> Parameters { get; set; }

        [JsonConstructor]
        public MaterialJSON(string className, string name, int Id, string UniqueId, string category, List<ParameterJSON> parameters)
        {
            this.Class = className;
            this.Name = name;
            this.Id = Id;
            this.UniqueId = UniqueId;
            this.Category = category;
            this.Parameters = parameters;
        }

        public MaterialJSON(revitMaterial material)
        {

            this.Class = material.GetType().Name;
            this.Name = material.Name;
            this.Id = material.Id.IntegerValue;
            this.UniqueId = material.UniqueId.ToString();
            this.Category = material.Category.Name;
            this.Parameters = new List<ParameterJSON>();

            //Iterate through parameters
            foreach (Parameter param in material.Parameters)
            {
                //If the parameter has a value, the add it to the parameter list for export
                if (!param.IsReadOnly)
                {
                    this.Parameters.Add(new ParameterJSON(param, material.Document));
                }
            }
        }

        public static string ToJSON(MaterialJSON material)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(material, Formatting.Indented);
        }

        public static revitMaterial ToMaterial(MaterialJSON MatJSON, revitDoc doc)
        {
            revitMaterial rMat = (revitMaterial)doc.GetElement(MatJSON.UniqueId);

            rMat.Name = MatJSON.Name;

            foreach (ParameterJSON paramJson in MatJSON.Parameters)
            {
                ParameterJSON.ModifyParameter(paramJson, rMat);
            }

            return rMat;
        }
    }
}
