using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;
using revitMaterial = Autodesk.Revit.DB.Material;

using Revit.Elements;
using dynElem = Revit.Elements.Element;
using dynMaterial = Revit.Elements.Material;

namespace Synthetic.Revit
{
    /// <summary>
    /// Extensions of Dynamo Revit
    /// </summary>
    public class Material
    {
        internal Material () { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="document"></param>
        /// <returns name="material"></returns>
        public static revitMaterial GetByName (string Name,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            revitDB.FilteredElementCollector collector
                = new revitDB.FilteredElementCollector(document);

            collector
                .OfClass(typeof(revitDB.Material))
                .OfType<revitDB.Material>();

            return collector
                .Cast<revitMaterial>()
                .Where(mat => mat.Name == Name)
                .Select(elem =>
                {
                    return elem.ToDSType(true);
                })
                .ToList();
        }
    }
}
