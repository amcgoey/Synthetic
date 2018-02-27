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
        /// Gets a material given its name and document
        /// </summary>
        /// <param name="Name">Name of a material</param>
        /// <param name="Document">Document to get the material from</param>
        /// <returns name="Material">A Autodeks.Revit.DB.Material</returns>
        public static revitMaterial GetByNameDocument (string Name,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc Document)
        {
            revitDB.FilteredElementCollector collector
                = new revitDB.FilteredElementCollector(Document);

            collector
                .OfClass(typeof(revitDB.Material))
                .OfType<revitDB.Material>();

            return collector
                .OfType<revitDB.Material>()
                .FirstOrDefault(
                m => m.Name.Equals(Name));
        }

        /// <summary>
        /// Gets a material from the current doucment given its name
        /// </summary>
        /// <param name="Name">Name of material</param>
        /// <returns name="Material">A Autodeks.Revit.DB.Material</returns>
        public static revitMaterial GetByName (string Name)
        {
            return GetByNameDocument(Name, Synthetic.Revit.Document.Current());
        }
    }
}
