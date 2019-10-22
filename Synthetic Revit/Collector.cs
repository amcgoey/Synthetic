using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;
using Dynamo.Graph.Nodes;

using RevitDB = Autodesk.Revit.DB;
using RevitDoc = Autodesk.Revit.DB.Document;
using RevitCollector = Autodesk.Revit.DB.FilteredElementCollector;

using Revit.Elements;
using RevitServices.Transactions;
using RevitServices.Persistence;

//Need all these types so the Collectors can return these types of objects.
using DynElem = Revit.Elements.Element;
using DynCat = Revit.Elements.Category;
using DynFamilyType = Revit.Elements.FamilyType;
using DynFamily = Revit.Elements.Family;
using DynLevel = Revit.Elements.Level;
using DynView = Revit.Elements.Views.View;


namespace Synthetic.Revit
{
    /// <summary>
    /// Nodes for retrieving elements from a Revit project using filter criteria.  Refer to the RevitAPI documentation on FilteredElementCollectors and ElementFilters for optimizing the speed of the query.
    /// </summary>
    [IsDesignScriptCompatible]
    public class Collector
    {
        internal RevitDoc _document { get; private set; }
        internal List<RevitDB.ElementFilter> _filters { get; private set; }
        internal RevitDB.ElementId _viewId { get; private set; }
        internal List<RevitDB.ElementId> _elemIds { get; private set; }

        /// <summary>
        /// Constructor that takes a Revit Document as input.  Does not include filters.
        /// </summary>
        /// <param name="doc">A Revit Document</param>
        internal Collector (RevitDoc doc)
        {
            _document = doc;
            _filters = new List<RevitDB.ElementFilter>();
            _viewId = null;
            _elemIds = null;
        }

        /// <summary>
        /// Constructor that takes a list of ElementFilters and a Revit document as inputs.
        /// </summary>
        /// <param name="filters">A list of ElementFilters</param>
        /// <param name="doc">A Revit Document</param>
        internal Collector(List<RevitDB.ElementFilter> filters, RevitDoc doc)
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
        internal Collector(List<RevitDB.ElementFilter> filters, RevitDB.ElementId viewId, RevitDoc doc)
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
        internal Collector(List<RevitDB.ElementFilter> filters, List<RevitDB.ElementId> elemIds, RevitDoc doc)
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
        internal RevitCollector _ApplyFilters ()
        {
            RevitCollector rCollector;

            if (_viewId != null)
            {
                rCollector = new RevitCollector(this._document, _viewId);
            }
            else if (_elemIds != null)
            {
                rCollector = new RevitCollector(this._document, _elemIds);
            }
            else
            {
                rCollector = new RevitCollector(this._document);
            }

            foreach (RevitDB.ElementFilter filter in this._filters)
            {
                rCollector.WherePasses(filter);
            }
            return rCollector;
        }        

        /// <summary>
        /// Applies filters and returns the Revit FilteredElementCollector object.  This object gives quick access to the elements in the collector.
        /// </summary>
        /// <param name="collector">A Synthetic Collector object.</param>
        /// <returns name="Revit Collector">A Revit FilteredElementCollector object.</returns>
        public static RevitCollector ApplyFilters (Collector collector)
        {
            return collector._ApplyFilters();
        }

        /// <summary>
        /// Creates a Synthetic Collector for a project without any filters.  By default, the current project is used.  Without filters applied, the collector will retrieve all objects in the Revit project.
        /// </summary>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns>A Synthetic Collector object without any filters.</returns>
        public static Collector ByDocument ([DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            return new Collector(document);
        }

        /// <summary>
        /// Creates a Synthetic Collector for a project with the inputed Element Filters.  By default, the current project is used.
        /// </summary>
        /// <param name="filters">A list of ElementFilter objects.</param>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns>A Synthetic Collector object</returns>
        public static Collector ByFilters (List<RevitDB.ElementFilter> filters,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
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
        public static Collector ByFiltersViewId(List<RevitDB.ElementFilter> filters,
            int viewId,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
        {
            Collector collector = new Collector(filters, new RevitDB.ElementId(viewId), document);
            return collector;
        }

        /// <summary>
        /// Creates a Synthetic Collector for a project with the inputed Element Filters.  By default, the current project is used.
        /// </summary>
        /// <param name="filters">A list of ElementFilter objects.</param>
        /// <param name="elementIds">The view's ElementId as an integer</param>
        /// <param name="document">A Autodesk.Revit.DB.Document object.  This does not work with Dynamo document objects.</param>
        /// <returns>A Synthetic Collector object</returns>
        public static Collector ByFiltersElemIds(List<RevitDB.ElementFilter> filters,
            List<RevitDB.ElementId> elementIds,
            [DefaultArgument("Synthetic.Revit.Document.Current()")] RevitDoc document)
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
        public static IList<DynElem> ToElements (Collector collector,
            [DefaultArgument("true")] bool toggle = true)
        {
            IList<RevitDB.Element> elements = collector._ApplyFilters().ToElements();

            IList<DynElem> dynamoElements = new List<DynElem>();

            foreach (RevitDB.Element elem in elements)
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
        public static IList<RevitDB.Element> ToRevitElements(Collector collector,
            [DefaultArgument("true")] bool toggle = true)
        {
            return (List<RevitDB.Element>)collector._ApplyFilters().ToElements();
        }

        /// <summary>
        /// Retrieves the ElementIds of elements that pass the Collector's filters.
        /// </summary>
        /// <param name="collector">A Syntehtic Collector</param>
        /// <returns name="ElementIds">Returns the ElementIds of the elements that pass the collector's filters.</returns>
        public static IList<RevitDB.ElementId> ToElementIds(Collector collector)
        {
            return (IList<RevitDB.ElementId>)collector._ApplyFilters().ToElementIds();
        }

        /// <summary>
        /// Searches the Collector for elements whose Name equals the string.
        /// </summary>
        /// <param name="collector">A Collector to search</param>
        /// <param name="name">Name of the elements to find.</param>
        /// <returns name="Elements">Returns a list of Dynamo wrapped elements that matches the query</returns>
        public static IList<DynElem> QueryNameEquals(Collector collector, string name)
        {
            RevitCollector rCollector = collector._ApplyFilters();

            IList<DynElem> dynamoElements = rCollector
                .Cast<RevitDB.Element>()
                .Where(elem => elem.Name == name)
                .Select(elem =>
                {
                    return elem.ToDSType(true);
                })
                .ToList();

            return dynamoElements;
        }

        /// <summary>
        /// Searches the Collector for elements whose Name contains the string.
        /// </summary>
        /// <param name="collector">A Collector to search</param>
        /// <param name="name">Name of the elements to find.</param>
        /// <returns name="Elements">Returns a list of Dynamo wrapped elements that matches the query</returns>
        public static IList<DynElem> QueryNameContains(Collector collector, string name)
        {
            RevitCollector rCollector = collector._ApplyFilters();

            IList<DynElem> dynamoElements = rCollector
                .Cast<RevitDB.Element>()
                .Where(elem => elem.Name.Contains(name))
                .Select(elem =>
                {
                    return elem.ToDSType(true);
                })
                .ToList();

            return dynamoElements;
        }

        /// <summary>
        /// Searches the Collector for elements whose Name does not contain the string.
        /// </summary>
        /// <param name="collector">A Collector to search</param>
        /// <param name="name">Name of the elements to find.</param>
        /// <returns name="Elements">Returns a list of Dynamo wrapped elements that matches the query</returns>
        public static IList<DynElem> QueryNameDoesNotContain(Collector collector, string name)
        {
            RevitCollector rCollector = collector._ApplyFilters();

            IList<DynElem> dynamoElements = rCollector
                .Cast<RevitDB.Element>()
                .Where(elem => !elem.Name.Contains(name))
                .Select(elem =>
                {
                    return elem.ToDSType(true);
                })
                .ToList();

            return dynamoElements;
        }

        /// <summary>
        /// Groups elements in the collector by their name
        /// </summary>
        /// <param name="collector">A Collector to search</param>
        /// <returns name="Elements">Returns a list of lists of Dynamo wrapped elements that are grouped by name</returns>
        public static List<List<DynElem>> QueryGroupByName(Collector collector)
        {
            RevitCollector rCollector = collector._ApplyFilters();

            List<List<DynElem>> dynamoElements = rCollector
                .Cast<RevitDB.Element>()
                .GroupBy(elem => elem.Name)
                .Select(grp => grp.Select( elem => elem.ToDSType(true)).ToList())
                .ToList();

            return dynamoElements;
        }

        /// <summary>
        /// Sets the ElementFilters for the collector.
        /// </summary>
        /// <param name="collector">A Synthetic Collector</param>
        /// <param name="filters">A list of ElementFilters</param>
        /// <returns name="collector">A Synthetic Collector with assigned ElementFilters.</returns>
        public static Collector SetFilters (Collector collector, List<RevitDB.ElementFilter> filters)
        {
            collector._filters = filters;
            return collector;
        }

        /// <summary>
        /// Gets a list of the filters for a collector.
        /// </summary>
        /// <param name="collector">A Syntehtic Collector</param>
        /// <returns name="ElementFilters">A list of ElementFilters</returns>
        public static List<RevitDB.ElementFilter> GetFilters(Collector collector)
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