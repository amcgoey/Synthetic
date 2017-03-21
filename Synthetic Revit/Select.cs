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
        public static IList<dynElem> AllElementsOfType (Type type,
            [DefaultArgument("false")] bool inverted,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            synthCollect collector = new synthCollect(document);
            collector.internalCollector.WherePasses(new revitDB.ElementClassFilter(type, inverted));

            IList<revitDB.Element> elements = collector.internalCollector.ToElements();
            IList<dynElem> dynamoElements = new List<dynElem>();

            foreach (revitDB.Element elem in elements)
            {
                try
                {
                    dynamoElements.Add(document.GetElement(elem.Id).ToDSType(true));
                }
                catch
                {
                    dynamoElements.Add(null);
                }
            }

            return dynamoElements;
        }
    }
}
