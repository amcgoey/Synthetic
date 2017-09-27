using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;

using revitDB = Autodesk.Revit.DB;
using revitDoc = Autodesk.Revit.DB.Document;
using revitFECollector = Autodesk.Revit.DB.FilteredElementCollector;

using Revit.Elements;
using RevitServices.Transactions;
using RevitServices.Persistence;

using dynElem = Revit.Elements.Element;
using dynCat = Revit.Elements.Category;
using dynFamilyType = Revit.Elements.FamilyType;
using dynFamily = Revit.Elements.Family;
using dynLevel = Revit.Elements.Level;
using dynView = Revit.Elements.Views.View;

namespace Synthetic.Revit
{
    /// <summary>
    /// Nodes that create ElementFilters to be used with Collectors.
    /// </summary>
    public class CollectorElementFilter
    {
        internal CollectorElementFilter() { }

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
        /// Creates a ElementFilter from a list of filters where elements must pass all filters in the set to be included.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="filters">A list of Element Filters</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter is then passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterLogicalAnd(IList<revitDB.ElementFilter> filters)
        {
            return new revitDB.LogicalAndFilter(filters);
        }

        /// <summary>
        /// Creates a ElementFilter from a list of filters where elements can pass any of the filters in the set to be included.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="filters">A list of Element Filters</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterLogicalOr(IList<revitDB.ElementFilter> filters)
        {
            return new revitDB.LogicalOrFilter(filters);
        }

        /// <summary>
        /// Creates a ElementFilter that passes elements in the category.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="categoryId">The category's Id as an integer.</param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementCategoryId(int categoryId, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementCategoryFilter(new revitDB.ElementId(categoryId), inverted);
        }

        /// <summary>
        /// Creates a ElementFilter that passes elements in the category.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="category">A dynamo wrapped catogry.</param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementCategory(dynCat category, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementCategoryFilter(new revitDB.ElementId(category.Id), inverted);
        }

        /// <summary>
        /// Creates a ElementFilter that passes elements of the matching class or element type.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="type">An element type.</param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementClass(Type type, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementClassFilter(type, inverted);
        }

        /// <summary>
        /// Creates a ElementFilter that passes elements in a specified design option.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="designOptionId">A Design Option's Id as an integer.</param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementDesignOptionId(int designOptionId, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementDesignOptionFilter(new revitDB.ElementId(designOptionId), inverted);
        }

        /// <summary>
        /// Creates a ElementFilter that passes elements are are Element Types.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementIsElementType([DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementIsElementTypeFilter(inverted);
        }

        /// <summary>
        /// Creates a ElementFilter that passes elements in the categories.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="categories">A collection of dynamo wrapped categories.</param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementMulticategory(ICollection<dynCat> categories, [DefaultArgument("false")] bool inverted)
        {
            IList<revitDB.ElementId> categoryIds = new List<revitDB.ElementId>();
            foreach (dynCat category in categories)
            {
                categoryIds.Add(new revitDB.ElementId(category.Id));
            }

            return new revitDB.ElementMulticategoryFilter(categoryIds, inverted);
        }

        /// <summary>
        /// Creates a ElementFilter that passes elements in the categories.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="categoryIds">A collection of category Ids as integers.</param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementMulticategoryId(ICollection<int> categoryIds, [DefaultArgument("false")] bool inverted)
        {
            IList<revitDB.ElementId> categoryIdsTemp = new List<revitDB.ElementId>();
            foreach (int categoryId in categoryIds)
            {
                categoryIdsTemp.Add(new revitDB.ElementId(categoryId));
            }

            return new revitDB.ElementMulticategoryFilter(categoryIdsTemp, inverted);
        }

        /// <summary>
        /// Creates a ElementFilter that passes elements on a workset.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="worksetId">The workset ID of the workset to filter.</param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementWorkset(int worksetId, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementWorksetFilter(new revitDB.WorksetId(worksetId), inverted);
        }

        /// <summary>
        /// Creates a ElementFilter that excludes elements with the given elementIds.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="elementIds">ElementIds as integers of the elements to be excluded.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
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
        /// Creates a ElementFilter that passes types with the specified family.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="familyId">A family's Id as an integer.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterFamilyTypeId(int familyId)
        {
            return new revitDB.FamilySymbolFilter(new revitDB.ElementId(familyId));
        }

        /// <summary>
        /// Creates a ElementFilter that passes types with the specified family.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="family">A dynamo wrapped family</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterFamilyType(dynFamily family)
        {
            return new revitDB.FamilySymbolFilter(new revitDB.ElementId(family.Id));
        }

        /// <summary>
        /// Creates a ElementFilter that passes types with the specified family.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="familyType">A dynamo wrapped family type</param>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterFamilyInstance(dynFamilyType familyType,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            return new revitDB.FamilyInstanceFilter(document, new revitDB.ElementId(familyType.Id));
        }

        /// <summary>
        /// Creates a ElementFilter that passes elements on the specified level.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="levelId">Level Id as an integer</param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementLevelId(int levelId, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementLevelFilter(new revitDB.ElementId(levelId), inverted);
        }

        /// <summary>
        /// Creates a ElementFilter that passes elements on the specified level.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="level">A Dynamo wrapped Level.</param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementLevel(dynLevel level, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementLevelFilter(new revitDB.ElementId(level.Id), inverted);
        }

        /// <summary>
        /// Creates a ElementFilter that passes elements owned by the given view.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="viewId">The Element Id of the view as an integer</param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementOwnerViewById(int viewId, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementOwnerViewFilter(new revitDB.ElementId(viewId), inverted);
        }

        /// <summary>
        /// Creates a ElementFilter that passes elements owned by the given view.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="view">The Element Id of the view as an integer</param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementOwnerView(dynView view, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementOwnerViewFilter(new revitDB.ElementId(view.Id), inverted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterRules"></param>
        /// <param name="inverted"></param>
        /// <returns></returns>
        //public static revitDB.ElementFilter FilterElementParameter(IList<revitDB.FilterRule> filterRules, bool inverted)
        //{
        //    return new revitDB.ElementParameterFilter(filterRules, inverted);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterId"></param>
        /// <param name="value"></param>
        /// <param name="inverted"></param>
        /// <returns></returns>
        //public static revitDB.ElementFilter FilterElementParameterStringEquals(int parameterId, string value, bool inverted)
        //{
        //    revitDB.ParameterValueProvider provider = new revitDB.ParameterValueProvider(new revitDB.ElementId(parameterId));
        //    revitDB.FilterStringRuleEvaluator evaluator = new revitDB.FilterStringEquals();
        //    revitDB.FilterRule filterRule = new revitDB.FilterStringRule(provider, evaluator, value, false);
        //    return new revitDB.ElementParameterFilter(filterRule, inverted);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterId"></param>
        /// <param name="value"></param>
        /// <param name="filterStringRule"></param>
        /// <param name="inverted"></param>
        /// <returns></returns>
        public static revitDB.ElementFilter FilterElementStringParameter(int parameterId, string value, string filterStringRule, [DefaultArgument("false")] bool inverted)
        {
            revitDB.ParameterValueProvider provider = new revitDB.ParameterValueProvider(new revitDB.ElementId(parameterId));
            revitDB.FilterStringRuleEvaluator evaluator;

            CollectorFilterRules.StringRules rule = (CollectorFilterRules.StringRules)Enum.Parse(typeof(CollectorFilterRules.StringRules), filterStringRule);

            switch (rule)
            {
                case CollectorFilterRules.StringRules.FilterStringBeginsWith:
                    evaluator = new revitDB.FilterStringBeginsWith();
                    break;
                case CollectorFilterRules.StringRules.FilterStringContains:
                    evaluator = new revitDB.FilterStringContains();
                    break;
                case CollectorFilterRules.StringRules.FilterStringEndsWith:
                    evaluator = new revitDB.FilterStringEndsWith();
                    break;
                case CollectorFilterRules.StringRules.FilterStringEquals:
                    evaluator = new revitDB.FilterStringEquals();
                    break;
                case CollectorFilterRules.StringRules.FilterStringGreater:
                    evaluator = new revitDB.FilterStringGreater();
                    break;
                case CollectorFilterRules.StringRules.FilterStringGreaterOrEqual:
                    evaluator = new revitDB.FilterStringGreaterOrEqual();
                    break;
                case CollectorFilterRules.StringRules.FilterStringLess:
                    evaluator = new revitDB.FilterStringLess();
                    break;
                case CollectorFilterRules.StringRules.FilterStringLessOrEqual:
                    evaluator = new revitDB.FilterStringLessOrEqual();
                    break;
                default:
                    evaluator = null;
                    break;
            }
            if (evaluator != null)
            {
                revitDB.FilterRule filterRule = new revitDB.FilterStringRule(provider, evaluator, value, false);
                return new revitDB.ElementParameterFilter(filterRule, inverted);
            }
            else return null;
        }
    }
}
