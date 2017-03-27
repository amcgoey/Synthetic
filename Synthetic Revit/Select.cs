using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;
using revitFECollector = Autodesk.Revit.DB.FilteredElementCollector;

using Revit.Elements;
using dynElem = Revit.Elements.Element;
using dynCat = Revit.Elements.Category;
using RevitServices.Transactions;
using RevitServices.Persistence;

using synthCollect = Synthetic.Revit.Collector;

namespace Synthetic.Revit
{
    /// <summary>
    /// Nodes that certain sets of elements using pre-configured Collectors and filters.
    /// </summary>
    public class Select
    {
        internal Select () { }

        /// <summary>
        /// Selects all elements of a type.  Works with documents other than the active document.
        /// </summary>
        /// <param name="type">The element type of the object, such as WallTypes or Walls.</param>
        /// <param name="inverted">If false, elements in the chosen category will be selected.  If true, elements NOT in the chosen category will be selected.</param>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns name="Elements">A list of Dynamo elements that pass the filer.</returns>
        public static IList<dynElem> AllElementsOfType (Type type,
            [DefaultArgument("false")] bool inverted,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            synthCollect collector = new synthCollect(document);
            List<revitDB.ElementFilter> filters = new List<Autodesk.Revit.DB.ElementFilter>();
            filters.Add(synthCollect.FilterElementClass(type, inverted));

            synthCollect.SetFilters(collector, filters);

            return synthCollect.ToElements(collector);
        }

        /// <summary>
        /// Selects all instance elements in a category, excludes element types.
        /// </summary>
        /// <param name="category">The categoryId of the elements you wish to select.</param>
        /// <param name="inverted">If false, elements in the chosen category will be selected.  If true, elements NOT in the chosen category will be selected.</param>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns name="Elements">A list of Dynamo elements that pass the filer.</returns>
        public static IList<dynElem> AllElementsOfCategory(dynCat category,
            [DefaultArgument("false")] bool inverted,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            synthCollect collector = new synthCollect(document);

            // Select only elements that are NOT Types (the filter is inverted)
            List<revitDB.ElementFilter> filters = new List<Autodesk.Revit.DB.ElementFilter>();
            filters.Add(synthCollect.FilterElementIsElementType(true));
            filters.Add(synthCollect.FilterElementCategory(category, inverted));

            synthCollect.SetFilters(collector, filters);

            return synthCollect.ToElements(collector);
        }

        /// <summary>
        /// Selects all Family Symbol types in a category, but excludes instances of those elements.  The node does not work with System familes because System Families do not have a Family Sybmol.
        /// </summary>
        /// <param name="category">The categoryId of the elements you wish to select.</param>
        /// <param name="inverted">If false, elements in the chosen category will be selected.  If true, elements NOT in the chosen category will be selected.</param>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns name="Elements">A list of Dynamo elements that pass the filer.</returns>
        public static IList<dynElem> AllFamilyTypesOfCategory(dynCat category,
            [DefaultArgument("false")] bool inverted,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            synthCollect collector = new synthCollect(document);

            // Select only elements that are Family Symbols
            List<revitDB.ElementFilter> filters = new List<Autodesk.Revit.DB.ElementFilter>();
            filters.Add(synthCollect.FilterElementClass(typeof(revitDB.FamilySymbol), false));
            filters.Add(synthCollect.FilterElementCategory(category, inverted));

            synthCollect.SetFilters(collector, filters);

            return synthCollect.ToElements(collector);
        }

        /// <summary>
        /// Retrieve all materials in the document.
        /// </summary>
        /// <param name="document">>A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns name="Materials">A list of Auotdesk.Revit.DB.Materials</returns>
        public static IEnumerable<revitDB.Material> AllMaterials([DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            {
                revitFECollector collector
                  = new revitFECollector(document);

                return collector
                  .OfClass(typeof(revitDB.Material))
                  .OfType<revitDB.Material>();
            }
        }

        /// <summary>
        /// Given a list of materials, returns the material that matches the given name.
        /// </summary>
        /// <param name="materials">A list of Autodesk.Revit.DB.Materials</param>
        /// <param name="materialName">The name of the material</param>
        /// <returns name="Material">A Autodesk.Revit.DB.Material that matches the given name.</returns>
        public static revitDB.Material GetMaterialByName(IEnumerable<revitDB.Material> materials, string materialName)
        {
            return materials
                .OfType<revitDB.Material>()
                .FirstOrDefault(
                m => m.Name.Equals(materialName));
        }
    }
}
