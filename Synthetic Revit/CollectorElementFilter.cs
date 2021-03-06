﻿using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

using revitDB = Autodesk.Revit.DB;
using RevitDoc = Autodesk.Revit.DB.Document;
using RevitFECollector = Autodesk.Revit.DB.FilteredElementCollector;

using Revit.Elements;
using RevitServices.Transactions;
using RevitServices.Persistence;

using DynElem = Revit.Elements.Element;
using DynCat = Revit.Elements.Category;
using DynFamilyType = Revit.Elements.FamilyType;
using DynFamily = Revit.Elements.Family;
using DynLevel = Revit.Elements.Level;
using DynView = Revit.Elements.Views.View;

namespace Synthetic.Revit
{
    /// <summary>
    /// Nodes that create ElementFilters to be used with Collectors.
    /// </summary>
    [IsDesignScriptCompatible]
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
        public static revitDB.ElementFilter FilterElementCategory(DynCat category, [DefaultArgument("false")] bool inverted)
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
        public static revitDB.ElementFilter FilterElementMulticategory(ICollection<DynCat> categories, [DefaultArgument("false")] bool inverted)
        {
            IList<revitDB.ElementId> categoryIds = new List<revitDB.ElementId>();
            foreach (DynCat category in categories)
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
        /// Creates a ElementFilter that passes elements with the given status on the given phase.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="phaseId">ElementId of the phase</param>
        /// <param name="PhaseStatus">The status of elements select in the phase.</param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementPhaseStatus (int phaseId,
            revitDB.ElementOnPhaseStatus PhaseStatus,
            [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementPhaseStatusFilter(new revitDB.ElementId(phaseId), PhaseStatus, inverted);
        }

        /// <summary>
        /// Creates a ElementFilter that passes elements that interseect an element.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="Element">The element to look for intersections.</param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementIntersectsElement (revitDB.Element Element, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementIntersectsElementFilter(Element, inverted);
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
        public static revitDB.ElementFilter FilterFamilyType(DynFamily family)
        {
            return new revitDB.FamilySymbolFilter(new revitDB.ElementId(family.Id));
        }

        /// <summary>
        /// Creates a ElementFilter that passes types with the specified family.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="familyType">A dynamo wrapped family type</param>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterFamilyInstance(DynFamilyType familyType,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
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
        public static revitDB.ElementFilter FilterElementLevel(DynLevel level, [DefaultArgument("false")] bool inverted)
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
        public static revitDB.ElementFilter FilterElementOwnerView(DynView view, [DefaultArgument("false")] bool inverted)
        {
            return new revitDB.ElementOwnerViewFilter(new revitDB.ElementId(view.Id), inverted);
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="filterRules"></param>
        ///// <param name="inverted"></param>
        ///// <returns></returns>
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
        /// Creates a ElementFilter that passes elements with a string based parameter that match the provided FilterStringRuleEvaluator.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="parameterId">The ElementId as an int of the parameter to search.</param>
        /// <param name="value">Value of the parameter to search for.</param>
        /// <param name="filterStringRule">Revit FilterStringRuleEvaluator that determines how to search the parameter.</param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementStringParameter(
            int parameterId,
            string value,
            revitDB.FilterStringRuleEvaluator filterStringRule,
            [DefaultArgument("false")] bool inverted)
        {
            revitDB.ParameterValueProvider provider = new revitDB.ParameterValueProvider(new revitDB.ElementId(parameterId));

            if (filterStringRule != null)
            {
                revitDB.FilterRule filterRule = new revitDB.FilterStringRule(provider, filterStringRule, value, false);
                return new revitDB.ElementParameterFilter(filterRule, inverted);
            }
            else return null;
        }

        /// <summary>
        /// Creates a ElementFilter that passes elements with a number based parameter that match the provided FilterNumericRuleEvaluator.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="parameterId">The ElementId as an int of the parameter to search.</param>
        /// <param name="value">Value of the parameter to search for.</param>
        /// <param name="tolerance">Tolerance on how close tot he value a match should be.</param>
        /// <param name="filterNumberRule">Revit FilterNumberRuleEvaluator that determines how to search the parameter.</param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementNumberParameter (
            int parameterId,
            double value,
            [DefaultArgument("0.0001")] double tolerance,
            revitDB.FilterNumericRuleEvaluator filterNumberRule,
            [DefaultArgument("false")] bool inverted)
        {
            revitDB.ParameterValueProvider provider = new revitDB.ParameterValueProvider(new revitDB.ElementId(parameterId));
            
            if (filterNumberRule != null)
            {
                revitDB.FilterRule filterRule = new revitDB.FilterDoubleRule(provider, filterNumberRule, value, tolerance);
                return new revitDB.ElementParameterFilter(filterRule, inverted);
            }
            else return null;
        }

        /// <summary>
        /// Creates a ElementFilter that passes elements with a number based parameter that match the provided FilterNumericRuleEvaluator.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="parameterId">The ElementId as an int of the parameter to search.</param>
        /// <param name="value">Value of the parameter to search for.</param>
        /// <param name="filterNumberRule">Revit FilterNumberRuleEvaluator that determines how to search the parameter.</param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementIntegerParameter(
            int parameterId,
            int value,
            revitDB.FilterNumericRuleEvaluator filterNumberRule,
            [DefaultArgument("false")] bool inverted)
        {
            revitDB.ParameterValueProvider provider = new revitDB.ParameterValueProvider(new revitDB.ElementId(parameterId));
           
            if (filterNumberRule != null)
            {
                revitDB.FilterRule filterRule = new revitDB.FilterIntegerRule(provider, filterNumberRule, value);
                return new revitDB.ElementParameterFilter(filterRule, inverted);
            }
            else return null;
        }

        /// <summary>
        /// Creates a ElementFilter that passes elements with an ElementId based parameter that match the provided FilterNumericRuleEvaluator.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.
        /// </summary>
        /// <param name="parameterId">The ElementId as an int of the parameter to search.</param>
        /// <param name="value">Value of the parameter to search for.</param>
        /// <param name="filterNumberRule">Revit FilterNumberRuleEvaluator that determines how to search the parameter.</param>
        /// <param name="inverted">If true, the filter elements NOT matching the filter criteria are chosen.</param>
        /// <returns name="ElementFilter">An Element Filter.  The filter should then be passed to a Collector node and the Collector retrieves elements that pass the filter.</returns>
        public static revitDB.ElementFilter FilterElementIdParameter(
            int parameterId,
            int value,
            revitDB.FilterNumericRuleEvaluator filterNumberRule,
            [DefaultArgument("false")] bool inverted)
        {
            revitDB.ParameterValueProvider provider = new revitDB.ParameterValueProvider(new revitDB.ElementId(parameterId));
            
            if (filterNumberRule != null)
            {
                revitDB.FilterRule filterRule = new revitDB.FilterElementIdRule(provider, filterNumberRule, new revitDB.ElementId(value));
                return new revitDB.ElementParameterFilter(filterRule, inverted);
            }
            else return null;
        }
    }
}
