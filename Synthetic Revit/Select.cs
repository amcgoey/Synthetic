using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;
using revitFECollector = Autodesk.Revit.DB.FilteredElementCollector;

using Revit.Elements;
using dynElem = Revit.Elements.Element;
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
        /// <param name="type"></param>
        /// <param name="inverted"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static IList<object> AllElementsOfType (Type type,
            [DefaultArgument("false")] bool inverted,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            synthCollect collector = new synthCollect(document);
            //synthCollect.WherePasses(collector, new revitDB.ElementClassFilter(type, inverted));
            synthCollect.WherePasses(collector, synthCollect.FilterElementClass(type, inverted));

            return synthCollect.ToElements(collector);
        }

        /// <summary>
        /// Selects all instance elements in a category, excludes element types.
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="inverted"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static IList<object> AllElementsOfCategory(int categoryId,
            [DefaultArgument("false")] bool inverted,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            synthCollect collector = new synthCollect(document);

            // Select only elements that are NOT Types (the filter is inverted)
            synthCollect.WherePasses(collector, synthCollect.FilterElementIsElementType(true));

            synthCollect.WherePasses(collector, synthCollect.FilterElementCategory(categoryId, inverted));

            return synthCollect.ToElements(collector);
        }

        /// <summary>
        /// Selects all element types in a category, but excludes instances of those elements.
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="inverted"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static IList<object> AllFamilyTypesOfCategory(int categoryId,
            [DefaultArgument("false")] bool inverted,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            synthCollect collector = new synthCollect(document);

            // Select only elements that are Family Symbols
            synthCollect.WherePasses(collector, synthCollect.FilterElementClass(typeof(revitDB.FamilySymbol), false));

            synthCollect.WherePasses(collector, synthCollect.FilterElementCategory(categoryId, inverted));

            return synthCollect.ToElements(collector);
        }
    }
}
