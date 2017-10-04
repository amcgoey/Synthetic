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
    /// Nodes for retrieving elements from a Revit project using filter criteria.  Refer to the RevitAPI documentation on FilteredElementCollectors and ElementFilters for optimizing the speed of the query.
    /// </summary>
    public class Collector
    {
        internal revitDoc _document { get; private set; }
        internal List<revitDB.ElementFilter> _filters { get; private set; }
        internal revitDB.ElementId _viewId { get; private set; }
        internal List<revitDB.ElementId> _elemIds { get; private set; }

        /// <summary>
        /// Constructor that takes a Revit Document as input.  Does not include filters.
        /// </summary>
        /// <param name="doc">A Revit Document</param>
        internal Collector (revitDoc doc)
        {
            _document = doc;
            _filters = new List<revitDB.ElementFilter>();
            _viewId = null;
            _elemIds = null;
        }

        /// <summary>
        /// Constructor that takes a list of ElementFilters and a Revit document as inputs.
        /// </summary>
        /// <param name="filters">A list of ElementFilters</param>
        /// <param name="doc">A Revit Document</param>
        internal Collector(List<revitDB.ElementFilter> filters, revitDoc doc)
        {
            _document = doc;
            _filters = filters;
            _viewId = null;
            _elemIds = null;
        }

        /// <summary>
        /// Constructor that takes a list of ElementFilters and a Revit document as inputs.
        /// </summary>
        /// <param name="filters">A list of ElementFilters</param>
        /// <param name="viewId">The Element Id of the view</param>
        /// <param name="doc">A Revit Document</param>
        internal Collector(List<revitDB.ElementFilter> filters, revitDB.ElementId viewId, revitDoc doc)
        {
            _document = doc;
            _filters = filters;
            _viewId = viewId;
            _elemIds = null;
        }

        /// <summary>
        /// Constructor that takes a list of ElementFilters. ElementIds and a Revit document as inputs.
        /// </summary>
        /// <param name="filters">A list of ElementFilters</param>
        /// <param name="elemIds">The Element Id of the view</param>
        /// <param name="doc">A Revit Document</param>
        internal Collector(List<revitDB.ElementFilter> filters, List<revitDB.ElementId> elemIds, revitDoc doc)
        {
            _document = doc;
            _filters = filters;
            _viewId = null;
            _elemIds = elemIds;
        }

        /// <summary>
        /// Creates a Revit FilteredElementCollector and passes filters to it.  Returns the Collector.
        /// </summary>
        /// <returns>A FilteredElementCollector with filters applied</returns>
        internal revitFECollector ApplyFilters ()
        {
            revitFECollector rCollector;

            if (_viewId != null)
            {
                rCollector = new revitFECollector(this._document, _viewId);
            }
            else if (_elemIds != null)
            {
                rCollector = new revitFECollector(this._document, _elemIds);
            }
            else
            {
                rCollector = new revitFECollector(this._document);
            }

            foreach (revitDB.ElementFilter filter in this._filters)
            {
                rCollector.WherePasses(filter);
            }
            return rCollector;
        }        

        /// <summary>
        /// Provides access directly to Revit FilteredElementCollector object with all filters applied.
        /// </summary>
        /// <param name="collector">A Synthetic Collector object.</param>
        /// <returns>A Revit FilteredElementCollector object.</returns>
        public static revitFECollector UnwrapCollector (Collector collector)
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
        /// Creates a Synthetic Collector for a project with the inputed Element Filters.  By default, the current project is used.
        /// </summary>
        /// <param name="filters">A list of ElementFilter objects.</param>
        /// <param name="viewId">The view's ElementId as an integer</param>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns>A Synthetic Collector object</returns>
        public static Collector ByFiltersViewId(List<revitDB.ElementFilter> filters,
            int viewId,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            Collector collector = new Collector(filters, new revitDB.ElementId(viewId), document);
            return collector;
        }

        /// <summary>
        /// Creates a Synthetic Collector for a project with the inputed Element Filters.  By default, the current project is used.
        /// </summary>
        /// <param name="filters">A list of ElementFilter objects.</param>
        /// <param name="elementIds">The view's ElementId as an integer</param>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns>A Synthetic Collector object</returns>
        public static Collector ByFiltersElemIds(List<revitDB.ElementFilter> filters,
            List<revitDB.ElementId> elementIds,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] revitDoc document)
        {
            Collector collector = new Collector(filters, elementIds, document);
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
        /// Retrieves the Elements that pass the Collector's filters
        /// </summary>
        /// <param name="collector">A Synthetc Collector</param>
        /// <param name="toggle">Toggle will reset the Dynamo graph and rerun the collector.</param>
        /// <returns name="Elements">Autodesk.Revit.DB.Elements</returns>
        public static IList<revitDB.Element> ToRevitElements(Collector collector,
            [DefaultArgument("true")] bool toggle = true)
        {
            IList<revitDB.Element> elements = collector.ApplyFilters().ToElements();

            return elements;
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
        /// <param name="collector">A Synthetic Collector</param>
        /// <param name="filters">A list of ElementFilters</param>
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
        /// <returns name="ElementFilters">A list of ElementFilters</returns>
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
    }
}