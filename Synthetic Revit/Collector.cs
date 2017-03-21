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
    /// 
    /// </summary>
    public class Collector
    {
        internal revitFECollector internalCollector { get; private set; }

        internal Collector (revitDoc doc)
        {
            internalCollector = new revitFECollector(doc);
        }

        internal Collector (revitFECollector collector)
        {
            internalCollector = collector;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="revitCollector"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static Collector Wrap (revitFECollector revitCollector)
        {
            return new Collector(revitCollector);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collector"></param>
        /// <returns></returns>
        public static revitFECollector Unwrap (Collector collector)
        {
            return collector.internalCollector;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public static Collector ByDocument ([DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            return new Collector(document);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        public static Collector ByFilter (revitDB.ElementFilter filter,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            Collector collector = new Collector(document);
            collector.internalCollector.WherePasses(filter);
            return collector;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collector"></param>
        /// <returns></returns>
        public static IList<dynElem> ToElements (Collector collector)
        {
            Collector collectTemp = new Collector(collector.internalCollector);

            IList<revitDB.Element> elements = collectTemp.internalCollector.ToElements();
            IList<dynElem> dynamoElements = new List<dynElem>();

            foreach (revitDB.Element elem in elements)
            {
                try
                {
                    dynamoElements.Add(elem.ToDSType(true));
                }
                catch
                {
                    dynamoElements.Add(null);
                }
            }
            
            return dynamoElements;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collector"></param>
        /// <returns></returns>
        public static IList<revitDB.ElementId> ToElementIds(Collector collector)
        {
            return (IList<revitDB.ElementId>)collector.internalCollector.ToElementIds();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collector"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static Collector WherePasses (Collector collector,
            revitDB.ElementFilter filter)
        {
            collector.internalCollector.WherePasses(filter);
            return collector;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static bool FilterIsInverted(revitDB.ElementFilter filter)
        {
            return filter.Inverted;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filters"></param>
        /// <returns>An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterLogicalAnd(IList<revitDB.ElementFilter> filters)
        {
            return new revitDB.LogicalAndFilter(filters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filters"></param>
        /// <returns>An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterLogicalOr(IList<revitDB.ElementFilter> filters)
        {
            return new revitDB.LogicalOrFilter(filters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns>An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementCategory(int categoryId, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementCategoryFilter(new revitDB.ElementId(categoryId), inverted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns>An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementClass(Type type, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementClassFilter(type, inverted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="designOptionId"></param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns>An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementDesignOption(int designOptionId, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementDesignOptionFilter(new revitDB.ElementId(designOptionId), inverted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns>An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementIsElementType([DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementIsElementTypeFilter(inverted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryIds"></param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns>An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
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
        /// <returns>An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementWorkset(int worksetId, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementWorksetFilter(new revitDB.WorksetId(worksetId), inverted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementIds"></param>
        /// <returns>An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
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
        /// <returns>An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
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
        /// <returns>An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementLevel(revitDB.ElementId levelId, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementLevelFilter(levelId, inverted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
