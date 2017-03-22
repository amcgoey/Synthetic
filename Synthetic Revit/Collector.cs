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

namespace Synthetic.Revit
{
    /// <summary>
    /// Nodes for retrieving elements from a Revit project using filter criteria.  Refer to the RevitAPI documentation on FilteredElementCollectors and ElementFilters for optimizing the speed of the query.
    /// </summary>
    public class Collector
    {
        internal revitDoc _document { get; private set; }
        internal List<revitDB.ElementFilter> _filters { get; private set; }

        internal Collector (revitDoc doc)
        {
            _document = doc;
            _filters = new List<revitDB.ElementFilter>();
        }

        internal Collector(List<revitDB.ElementFilter> filters, revitDoc doc)
        {
            _document = doc;
            _filters = filters;
        }

        internal revitFECollector ApplyFilters ()
        {
            revitFECollector rCollector = new revitFECollector(this._document);
            foreach (revitDB.ElementFilter filter in this._filters)
            {
                rCollector.WherePasses(filter);
            }
            return rCollector;
        }

        //internal IList<revitDB.Element> ToRevitElements ()
        //{
        //    IList<revitDB.Element> elements = new List<revitDB.Element>();
        //    revitFECollector rCollector = new revitFECollector(this._document);
        //    foreach (revitDB.ElementFilter filter in this._filters)
        //    {
        //        rCollector.WherePasses(filter);
        //    }
        //    elements = rCollector.ToElements();
        //    rCollector.Dispose();
        //    return elements;
        //}

        

        /// <summary>
        /// Provides access directly to Revit FilteredElementCollector object with all filters applied.
        /// </summary>
        /// <param name="collector">A Synthetic Collector object.</param>
        /// <returns>A Revit FilteredElementCollector object.</returns>
        public static revitFECollector Unwrap (Collector collector)
        {
            return collector.ApplyFilters();
        }

        /// <summary>
        /// Creates a Synthetic Collector for a project without any filters.  By default, the current project is used.  Without filters applied, the collector will retrieve all objects in the Revit project.
        /// </summary>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns>A Synthetic Collector object without any filters.</returns>
        public static Collector ByDocument ([DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            return new Collector(document);
        }

        /// <summary>
        /// Creates a Synthetic Collector for a project with the inputed Element Filters.  By default, the current project is used.
        /// </summary>
        /// <param name="filters">A list of ElementFilter objects.</param>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns>A Synthetic Collector object</returns>
        public static Collector ByFilters (List<revitDB.ElementFilter> filters,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            Collector collector = new Collector(filters, document);
            return collector;
        }

        /// <summary>
        /// Retrieves the Elements that pass the Collector's filters
        /// </summary>
        /// <param name="collector">A Synthetc Collector</param>
        /// <param name="toggle">Toggle will reset the Dynamo graph and rerun the collector.</param>
        /// <returns>A</returns>
        public static IList<dynElem> ToElements (Collector collector,
            [DefaultArgument("true")] bool toggle = true)
        {
            IList<revitDB.Element> elements = collector.ApplyFilters().ToElements();

            IList<dynElem> dynamoElements = new List<dynElem>();

            foreach (revitDB.Element elem in elements)
            {
                try
                {
                    dynamoElements.Add(elem.ToDSType(true));
                }
                catch { }
            }
            
            return dynamoElements;
        }

        /// <summary>
        /// Retrieves the ElementIds of elements that pass the Collector's filters.
        /// </summary>
        /// <param name="collector">A Syntehtic Collector</param>
        /// <returns name="ElementIds">Returns the ElementIds of the elements that pass the collector's filters.</returns>
        public static IList<revitDB.ElementId> ToElementIds(Collector collector)
        {
            return (IList<revitDB.ElementId>)collector.ApplyFilters().ToElementIds();
        }

        /// <summary>
        /// Sets the ElementFilters for the collector.
        /// </summary>
        /// <param name="collector"></param>
        /// <param name="filters"></param>
        /// <returns name="collector">A Synthetic Collector with assigned ElementFilters.</returns>
        public static Collector SetFilters (Collector collector, List<revitDB.ElementFilter> filters)
        {
            collector._filters = filters;
            return collector;
        }

        /// <summary>
        /// Gets a list of the filters for a collector.
        /// </summary>
        /// <param name="collector">A Syntehtic Collector</param>
        /// <returns name="ElementFilters"></returns>
        public static List<revitDB.ElementFilter> GetFilters(Collector collector)
        {
            return collector._filters;
        }

        /// <summary>
        /// Converts the object to a string representation.
        /// </summary>
        /// <returns name="String">A string representation of the object.</returns>
        public override string ToString()
        {
            return base.ToString();
        }

        /// <summary>
        /// Tests whether a ElementFilter is inverted or not.
        /// </summary>
        /// <param name="filter">A ElementFilter.</param>
        /// <returns>Returns true if inverted, false if not inverted.</returns>
        public static bool FilterIsInverted(revitDB.ElementFilter filter)
        {
            return filter.Inverted;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filters"></param>
        /// <returns name="ElementFilter">An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterLogicalAnd(IList<revitDB.ElementFilter> filters)
        {
            return new revitDB.LogicalAndFilter(filters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filters"></param>
        /// <returns name="ElementFilter">An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterLogicalOr(IList<revitDB.ElementFilter> filters)
        {
            return new revitDB.LogicalOrFilter(filters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementCategory(int categoryId, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementCategoryFilter(new revitDB.ElementId(categoryId), inverted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementClass(Type type, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementClassFilter(type, inverted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="designOptionId"></param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementDesignOption(int designOptionId, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementDesignOptionFilter(new revitDB.ElementId(designOptionId), inverted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementIsElementType([DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementIsElementTypeFilter(inverted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryIds"></param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementMulticategory(ICollection<int> categoryIds, [DefaultArgument("false")] bool inverted)
        {
            IList<revitDB.ElementId> categoryElemIds = new List<revitDB.ElementId>();
            foreach(int categoryId in categoryIds)
            {
                categoryElemIds.Add(new revitDB.ElementId(categoryId));
            }

            return new revitDB.ElementMulticategoryFilter(categoryElemIds, inverted);
        }

        /// <summary>
        /// Creates a filter for elements on a workset.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="worksetId">The workset ID of the workset to filter.</param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementWorkset(int worksetId, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementWorksetFilter(new revitDB.WorksetId(worksetId), inverted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementIds"></param>
        /// <returns name="ElementFilter">An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterExclusion(ICollection<int> elementIds)
        {
            IList<revitDB.ElementId> elemIds = new List<revitDB.ElementId>();
            foreach (int elemId in elementIds)
            {
                elemIds.Add(new revitDB.ElementId(elemId));
            }

            return new revitDB.ExclusionFilter(elemIds);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="familyId"></param>
        /// <param name="document"></param>
        /// <returns name="ElementFilter">An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterFamilySymbol(revitDB.ElementId familyId,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            return new revitDB.FamilySymbolFilter(familyId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="levelId"></param>
        /// <param name="inverted"></param>
        /// <returns name="ElementFilter">An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementLevel(revitDB.ElementId levelId, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementLevelFilter(levelId, inverted);
        }
    }
}
